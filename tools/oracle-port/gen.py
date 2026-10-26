#!/usr/bin/env python3
"""
Generates Capitra entities and EF Core configurations from the FINOVA Oracle metadata export
(apex-export/data/_COLUMNS.csv, _CONSTRAINTS.csv, _INDEXES.csv and one CSV of data per table),
so every table keeps every Oracle column, key, unique rule and index.

Capitra naming rules applied on top of the Oracle names:
  FNV -> CAP; Y/N flags -> IS_x booleans; history tables -> _HIST;
  DateTime columns end in _AT; DateOnly columns end in _DATE;
  CREATED_BY/CREATED_DT/UPDATED_BY/UPDATED_DT come from AuditableEntity.

It also writes map-<module>.json, which the data importer uses to load the Oracle CSV files.

usage: gen.py <data dir> <module> <namespace module> <table prefix>[,<prefix>...] [--only T1,T2]
example: gen.py /mnt/project-files/dotnet-migration/apex-export/data Gl Gl GL_
"""
import csv
import json
import os
import re
import sys
from collections import defaultdict
from decimal import Decimal, InvalidOperation

csv.field_size_limit(1 << 30)
REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
AUDIT = {'CREATED_BY', 'CREATED_DT', 'UPDATED_BY', 'UPDATED_DT', 'CREATED_AT', 'UPDATED_AT',
         'CREATION_DATE', 'LAST_UPDATE_DATE', 'LAST_UPDATED_BY', 'CREATED_ON', 'UPDATED_ON', 'ROW_VERSION_NO',
         'CREATED_DATE', 'UPDATED_DATE', 'LAST_MODIFIED_BY', 'LAST_MODIFIED_DATE'}
AUDIT_MAP = {'CREATED_BY': 'CreatedBy', 'CREATED_DT': 'CreatedAt', 'CREATED_AT': 'CreatedAt',
             'CREATION_DATE': 'CreatedAt', 'UPDATED_BY': 'UpdatedBy', 'LAST_UPDATED_BY': 'UpdatedBy',
             'UPDATED_DT': 'UpdatedAt', 'UPDATED_AT': 'UpdatedAt', 'LAST_UPDATE_DATE': 'UpdatedAt',
             'CREATED_ON': 'CreatedAt', 'UPDATED_ON': 'UpdatedAt', 'ROW_VERSION_NO': 'RowVersion',
             'CREATED_DATE': 'CreatedAt', 'UPDATED_DATE': 'UpdatedAt', 'LAST_MODIFIED_BY': 'UpdatedBy', 'LAST_MODIFIED_DATE': 'UpdatedAt'}
# Oracle table -> [Capitra table, class] where the default rules below do not give the agreed name.
NAMES = {k: v for k, v in json.load(open(os.path.join(os.path.dirname(__file__), 'names.json'))).items() if not k.startswith('_')}
# generic column names the naming standard does not allow alone: they are qualified by the table
BARE = {'ID', 'NAME', 'TYPE', 'STATUS', 'DATE', 'VALUE', 'CODE_ID', 'DESC'}
CS_KEYWORDS = {'Type', 'Object', 'String', 'Decimal', 'Base', 'Event', 'Checked', 'Operator', 'Params', 'Lock'}
AMOUNT_WORDS = ('AMOUNT', 'AMT', 'BALANCE', 'TOTAL', 'DR', 'CR', 'DEBIT', 'CREDIT', 'PRICE', 'VALUE',
                'COST', 'FEE', 'PRINCIPAL', 'INTEREST', 'VARIANCE', 'TOLERANCE', 'LIMIT', 'DIFFERENCE')
RATE_WORDS = ('RATE', 'PERCENT', 'PCT', 'FACTOR', 'RATIO')


def rd(path):
    with open(path, encoding='utf-8', errors='replace', newline='') as f:
        return list(csv.DictReader(f))


def pascal(name):
    return ''.join(p[:1] + p[1:].lower() for p in name.split('_') if p)


def upper_snake(prop):
    """The column name Capitra's EF naming convention (DbNames.ToUpperSnake) gives a property."""
    out = []
    for i, c in enumerate(prop):
        if c.isupper() and i > 0:
            prev = prop[i - 1]
            next_lower = i + 1 < len(prop) and prop[i + 1].islower()
            if prev.islower() or prev.isdigit() or (prev.isupper() and next_lower):
                out.append('_')
        out.append(c.upper())
    return ''.join(out)


def singular(word):
    w = word
    for a, b in (('IES', 'Y'), ('SSES', 'SS'), ('STATUSES', 'STATUS'), ('CHES', 'CH'), ('SHES', 'SH'),
                 ('XES', 'X'), ('ANALYSES', 'ANALYSIS'), ('ALIASES', 'ALIAS')):
        if w.endswith(a):
            return w[: -len(a)] + b
    if w.endswith('S') and not w.endswith('SS') and not w.endswith('US') and not w.endswith('IS'):
        return w[:-1]
    return w


# Oracle prefix -> Capitra prefix for whole modules (REF_ lookups become <module>_LKP_)
PREFIXES = {'FNV_': 'CAP_', 'REF_MFF_': 'MFF_LKP_'}


def reprefix(oracle):
    for a, b in PREFIXES.items():
        if oracle.startswith(a):
            return b + oracle[len(a):]
    return oracle


def table_name(oracle):
    if oracle in NAMES:
        return NAMES[oracle][0]
    t = reprefix(oracle)
    t = re.sub(r'_HISTORY$', '_HIST', t)
    last = t.split('_')[-1]
    if last not in ('HIST',) and not (last.endswith('S') and last not in ('STATUS', 'ADDRESS', 'CLASS', 'BASIS')):
        if re.search(r'[^AEIOU]Y$', last):
            t = t[:-1] + 'IES'      # PROPERTY -> PROPERTIES, CITY -> CITIES
        else:
            t += 'ES' if last.endswith(('CH', 'SH', 'X', 'SS', 'STATUS')) else 'S'
    return t


def class_name(oracle):
    if oracle in NAMES:
        return NAMES[oracle][1]
    t = reprefix(oracle)
    parts = t.split('_')
    if parts[-1] == 'HISTORY':
        return pascal('_'.join(parts))
    parts[-1] = singular(parts[-1])
    return pascal('_'.join(parts))


def is_flag_check(cond, col):
    if not cond:
        return False
    c = cond.replace(' ', '').upper()
    return bool(re.fullmatch(r'%s IN\(\'Y\',\'N\'\)|%sIN\(\'N\',\'Y\'\)' % (col, col), c.replace(col + 'IN', col + ' IN'))) \
        or c in (f"{col}IN('Y','N')", f"{col}IN('N','Y')", f"NVL({col},'N')IN('Y','N')")


def in_list(cond, col):
    """Values of a  col IN ('A','B')  check, or None."""
    if not cond:
        return None
    m = re.fullmatch(r'\s*"?%s"?\s+IN\s*\((.*)\)\s*' % re.escape(col), cond, re.I | re.S)
    if not m:
        return None
    vals = re.findall(r"'([^']*)'", m.group(1))
    return vals or None


def scan_data(path, cols):
    """Per column: max decimals, max integer digits, any time part, max length."""
    info = {c: {'dec': 0, 'int': 0, 'time': False, 'len': 0, 'nonempty': 0, 'vals': set()} for c in cols}
    if not os.path.exists(path):
        return info
    for r in rd(path):
        for c in cols:
            v = r.get(c)
            if v is None or v == '':
                continue
            i = info[c]
            i['nonempty'] += 1
            i['len'] = max(i['len'], len(v))
            if len(i['vals']) < 4:
                i['vals'].add(v.strip())
            if re.fullmatch(r'-?\d*\.?\d+(E[-+]?\d+)?', v, re.I):
                try:
                    d = Decimal(v)
                    t = d.as_tuple()
                    i['dec'] = max(i['dec'], -t.exponent if t.exponent < 0 else 0)
                    i['int'] = max(i['int'], len(str(abs(int(d)))))
                except InvalidOperation:
                    pass
            m = re.match(r'\d{4}-\d\d-\d\d (\d\d:\d\d:\d\d)', v)
            if m and m.group(1) != '00:00:00':
                i['time'] = True
    return info


def main():
    data_dir, module, ns_module, prefixes = sys.argv[1:5]
    prefixes = prefixes.split(',')
    only = None
    if '--only' in sys.argv:
        only = set(sys.argv[sys.argv.index('--only') + 1].split(','))

    columns = defaultdict(list)
    for r in rd(os.path.join(data_dir, '_COLUMNS.csv')):
        columns[r['TABLE_NAME']].append(r)
    cons = defaultdict(list)
    for r in rd(os.path.join(data_dir, '_CONSTRAINTS.csv')):
        cons[r['TABLE_NAME']].append(r)
    idx = defaultdict(list)
    for r in rd(os.path.join(data_dir, '_INDEXES.csv')):
        idx[r['TABLE_NAME']].append(r)

    tables = sorted(t for t in columns
                    if any(t.startswith(p) for p in prefixes)
                    and os.path.exists(os.path.join(data_dir, t + '.csv'))
                    and (only is None or t in only))

    # Every table with an export becomes a class; foreign keys may point into other modules.
    all_tables = {t for t in columns if os.path.exists(os.path.join(data_dir, t + '.csv'))}
    pk_of = {}
    for t, cl in cons.items():
        for c in cl:
            if c['CONSTRAINT_TYPE'] == 'P':
                pk_of[t] = c['COLUMNS']
    cons_by_name = {c['CONSTRAINT_NAME']: c for cl in cons.values() for c in cl}

    dom_dir = os.path.join(REPO, 'src/Capitra.Domain/Modules', ns_module)
    cfg_dir = os.path.join(REPO, 'src/Capitra.Infrastructure/Persistence/Configurations', ns_module)
    os.makedirs(dom_dir, exist_ok=True)
    os.makedirs(cfg_dir, exist_ok=True)

    mapping = {}
    module_of = json.load(open(os.path.join(os.path.dirname(__file__), 'modules.json')))

    for t in tables:
        cols = sorted(columns[t], key=lambda r: int(r['COLUMN_ID']))
        tcons = cons[t]
        checks = [c for c in tcons if c['CONSTRAINT_TYPE'] == 'C' and c['COLUMNS'] and ',' not in c['COLUMNS']]
        info = scan_data(os.path.join(data_dir, t + '.csv'), [c['COLUMN_NAME'] for c in cols])
        pk = pk_of.get(t)
        cls = class_name(t)
        tbl = table_name(t)

        props = []      # (prop, cstype, nullable, comment, init, oracle col, kind, extra)
        used = set()
        for c in cols:
            on = c['COLUMN_NAME']
            dt = c['DATA_TYPE']
            nullable = c['NULLABLE'] == 'Y'
            if on in AUDIT:
                mapping.setdefault(t, {'table': tbl, 'class': cls, 'columns': {}})
                continue
            ci = info[on]
            colchecks = [x for x in checks if x['COLUMNS'] == on]
            vals = None
            flag = False
            for x in colchecks:
                cond = x['SEARCH_CONDITION_VC']
                if is_flag_check(cond, on):
                    flag = True
                v = in_list(cond, on)
                if v:
                    vals = v
            if vals and sorted(vals) == ['N', 'Y']:
                flag = True
            # a one-character IS_ column, or one that only ever holds Y or N, is a Y/N flag too
            if dt in ('CHAR', 'VARCHAR2') and int(c['CHAR_LENGTH'] or 0) == 1 and (on.startswith('IS_') or (ci['vals'] and ci['vals'] <= {'Y', 'N'})):
                flag = True
            if on.endswith(('_FLAG', '_YN')) and dt in ('CHAR', 'VARCHAR2') and int(c['CHAR_LENGTH'] or 0) == 1:
                flag = True

            name = on.replace('FNV', 'CAP')
            if name in BARE:  # MFF_DELINQUENCIES.STATUS -> DELINQUENCY_STATUS
                name = '_'.join(tbl.split('_')[1:-1] + [singular(tbl.split('_')[-1]), name])
            comment = None
            init = None
            dflt = (c['DATA_DEFAULT_VC'] or '').strip().strip('\n').strip()
            if flag:
                base = re.sub(r'_(FLAG|YN)$', '', name)
                if base.startswith('REQUIRE_'):
                    base = base[len('REQUIRE_'):] + '_REQUIRED'
                elif base.startswith('ALLOW_'):
                    base = base[len('ALLOW_'):] + '_ALLOWED'
                elif base.startswith('REQUIRES_'):
                    base = base[len('REQUIRES_'):] + '_REQUIRED'
                name = base if base.startswith('IS_') else 'IS_' + base
                if name.endswith('_AMOUNT'):  # IS_UNLIMITED_AMOUNT -> IS_AMOUNT_UNLIMITED: _AMOUNT columns are money
                    name = 'IS_AMOUNT_' + name[3:-len('_AMOUNT')]
                cstype, kind = 'bool', 'flag'
                if dflt.strip("'") == 'Y':
                    init = 'true'
                nullable = False
            elif dt == 'NUMBER':
                p, s = c['DATA_PRECISION'], c['DATA_SCALE']
                if p and s and int(s) > 0:
                    cstype, kind = 'decimal', 'decimal'
                elif p and (s in ('', '0')):
                    cstype, kind = ('int' if int(p) <= 9 else 'long'), 'int'
                else:
                    up = on.upper()
                    if on == pk or up.endswith('_ID') or up == 'ID':
                        cstype, kind = 'long', 'int'
                    elif up.split('_')[-1] in ('COUNT', 'NUM', 'NO', 'SEQ', 'ORDER', 'DAYS', 'LEVEL') and ci['dec'] == 0:
                        cstype, kind = ('long' if ci['int'] > 9 else 'int'), 'int'
                    elif any(w in up.split('_') for w in RATE_WORDS + AMOUNT_WORDS) or ci['dec'] > 0:
                        cstype, kind = 'decimal', 'decimal'
                    else:
                        cstype, kind = ('long' if ci['int'] > 9 else 'int'), 'int'
                if dflt and re.fullmatch(r'-?\d+(\.\d+)?', dflt) and Decimal(dflt) != 0:
                    init = dflt + ('m' if cstype == 'decimal' else '')
            elif dt == 'DATE' or dt.startswith('TIMESTAMP'):
                is_dt = dt.startswith('TIMESTAMP') or ci['time'] or name.endswith('_DT') or name.endswith('_AT')
                if name.endswith('_DATE') and dt == 'DATE' and not ci['time']:
                    is_dt = False
                if is_dt:
                    cstype, kind = 'DateTime', 'datetime'
                    if name.endswith('_DT'):
                        name = name[:-3] + '_AT'
                    elif name.endswith('_DATE'):
                        name = name[:-5] + '_AT'
                    elif not name.endswith('_AT'):
                        name = name + '_AT'
                else:
                    cstype, kind = 'DateOnly', 'date'
                    if not name.endswith('_DATE'):
                        name = re.sub(r'_DT$', '', name) + '_DATE'
            elif dt in ('VARCHAR2', 'CHAR', 'NVARCHAR2', 'NCHAR'):
                cstype, kind = 'string', 'string'
                if dflt.startswith("'") and dflt.endswith("'"):
                    init = '"' + dflt[1:-1].replace('"', '\\"') + '"'
            elif dt in ('CLOB', 'NCLOB', 'LONG'):
                cstype, kind = 'string', 'text'
            elif dt == 'BLOB':
                cstype, kind = 'byte[]', 'blob'
            else:
                cstype, kind = 'string', 'string'

            prop = pascal(name)
            if prop == cls or prop in CS_KEYWORDS:
                prop = prop + 'Value' if prop != cls else prop + 'Code'
                name = re.sub(r'(?<=[a-z0-9])([A-Z])', r'_\1', prop).upper()
            while prop in used:
                prop += '2'
            used.add(prop)
            if vals and not flag:
                comment = 'One of: ' + ', '.join(vals) + '.'
            ex = None
            if kind == 'decimal':
                p, s = c['DATA_PRECISION'], c['DATA_SCALE']
                if p and s and int(s) > 0:
                    ex = (int(p), int(s))
                else:
                    up = on.upper()
                    ex = (28, max(10, ci['dec'])) if any(w in up.split('_') for w in RATE_WORDS) else (28, max(2, ci['dec']))
                # Capitra standard: _AMOUNT is numeric(18,2), _RATE and _PCT numeric(9,6), unless the data needs more.
                if name.endswith('_AMOUNT') and ci['dec'] <= 2 and ci['int'] <= 16:
                    ex = (18, 2)
                elif name.endswith(('_RATE', '_PCT')) and ci['dec'] <= 6 and ci['int'] <= 3:
                    ex = (9, 6)
            if kind in ('string',):
                ex = int(c['CHAR_LENGTH'] or c['DATA_LENGTH'] or 0) or None
            props.append(dict(prop=prop, col=name, cstype=cstype, nullable=nullable and on != pk, comment=comment,
                              init=init, oracle=on, kind=kind, extra=ex, pk=(on == pk)))

        mapping[t] = {'table': tbl, 'class': cls, 'columns': {p['oracle']: {'column': p['col'], 'kind': p['kind']} for p in props}}
        for a in (c['COLUMN_NAME'] for c in cols if c['COLUMN_NAME'] in AUDIT):
            mapping[t]['columns'][a] = {'column': re.sub(r'(?<=[a-z])([A-Z])', r'_\1', AUDIT_MAP[a]).upper(), 'kind': 'audit'}
        mapping[t]['pk'] = next((p['col'] for p in props if p['pk']), None)

        # Domain class.
        lines = ['using Capitra.Domain.Common;', '', f'namespace Capitra.Domain.Modules.{ns_module};', '',
                 '/// <summary>', f'/// {tbl}: copy of FINOVA {t} (APEX), all columns kept.', '/// </summary>',
                 f'public class {cls} : AuditableEntity', '{']
        for i, p in enumerate(props):
            if i:
                lines.append('')
            if p['comment']:
                lines.append(f'    /// <summary>{p["comment"]}</summary>')
            t_ = p['cstype']
            nullable = p['nullable']
            if t_ in ('string', 'byte[]'):
                decl = f'{t_}?' if nullable else t_
                init = p['init'] or ('string.Empty' if t_ == 'string' and not nullable else ('[]' if t_ == 'byte[]' and not nullable else None))
            else:
                decl = f'{t_}?' if nullable and t_ != 'bool' else t_
                init = p['init'] if not nullable or t_ == 'bool' else None
                if init and t_ == 'decimal' and not init.endswith('m'):
                    init += 'm'
            lines.append(f'    public {decl} {p["prop"]} {{ get; set; }}' + (f' = {init};' if init else ''))
        lines.append('}')
        open(os.path.join(dom_dir, cls + '.cs'), 'w').write('\n'.join(lines) + '\n')

        # Configuration.
        bylist = {p['oracle']: p for p in props}
        cfg = [f'    public void Configure(EntityTypeBuilder<{cls}> builder)', '    {',
               f'        builder.ToTable("{tbl}");']
        pkp = next((p for p in props if p['pk']), None)
        def idx_expr(colnames):
            ps = [bylist[c]['prop'] for c in colnames if c in bylist]
            if len(ps) != len(colnames) or not ps:
                return None
            return f'x => x.{ps[0]}' if len(ps) == 1 else 'x => new { ' + ', '.join('x.' + p for p in ps) + ' }'
        if pkp:
            cfg.append(f'        builder.HasKey(x => x.{pkp["prop"]});')
        elif pk and ',' in pk:
            cfg.append(f'        builder.HasKey({idx_expr(pk.split(","))});')
        seen_idx = set()


        for c in sorted(tcons, key=lambda c: c['CONSTRAINT_NAME']):
            if c['CONSTRAINT_TYPE'] == 'U':
                cn = c['COLUMNS'].split(',')
                e = idx_expr(cn)
                if e and tuple(cn) not in seen_idx:
                    seen_idx.add(tuple(cn))
                    cfg.append(f'        builder.HasIndex({e}).IsUnique();')
        for i in sorted(idx[t], key=lambda i: i['INDEX_NAME']):
            cn = i['COLUMNS'].split(',')
            if tuple(cn) in seen_idx or (pkp and cn == [pkp['oracle']]):
                continue
            e = idx_expr(cn)
            if e:
                seen_idx.add(tuple(cn))
                cfg.append(f'        builder.HasIndex({e})' + ('.IsUnique();' if i['UNIQUENESS'] == 'UNIQUE' else ';'))
        cfg.append('')
        for p in props:
            if p['kind'] == 'string' and p['extra']:
                cfg.append(f'        builder.Property(x => x.{p["prop"]}).HasMaxLength({p["extra"]});')
            elif p['kind'] == 'decimal':
                cfg.append(f'        builder.Property(x => x.{p["prop"]}).HasPrecision({p["extra"][0]}, {p["extra"][1]});')
            if upper_snake(p['prop']) != p['col']:
                cfg.append(f'        builder.Property(x => x.{p["prop"]}).HasColumnName("{p["col"]}");')
            if p['pk'] and p['cstype'] in ('long', 'int'):
                cfg.append(f'        builder.Property(x => x.{p["prop"]}).UseIdentityByDefaultColumn();')
        cfg.append('')
        usings = {'Capitra.Domain.Modules.' + ns_module}
        fks = [c for c in tcons if c['CONSTRAINT_TYPE'] == 'R']
        for c in sorted(fks, key=lambda c: c['CONSTRAINT_NAME']):
            rt = c['R_TABLE_NAME']
            col = c['COLUMNS']
            if col not in bylist or rt not in all_tables:
                cfg.append(f'        // {c["CONSTRAINT_NAME"]}: {col} -> {rt} (table not exported)')
                continue
            rmod = module_of.get(table_name(rt).split('_')[0], None)
            if rt not in tables and rmod is None:
                cfg.append(f'        // {col} -> {rt}: added when the {table_name(rt).split("_")[0]} tables are ported.')
                continue
            if rt not in tables:
                usings.add('Capitra.Domain.Modules.' + rmod)
            rcls = class_name(rt)
            ref = cons_by_name.get(c['R_CONSTRAINT_NAME'])
            pkref = ''
            if ref and ref['CONSTRAINT_TYPE'] == 'U':
                rprop = pascal(ref['COLUMNS'])
                pkref = f'.HasPrincipalKey(x => x.{rprop})'
            delete = 'Cascade' if c['DELETE_RULE'] == 'CASCADE' else ('SetNull' if c['DELETE_RULE'] == 'SET NULL' else 'Restrict')
            cfg.append(f'        builder.HasOne<{rcls}>().WithMany(){pkref}.HasForeignKey(x => x.{bylist[col]["prop"]}).OnDelete(DeleteBehavior.{delete});')
        cfg.append('        builder.ConfigureAudit();')
        cfg.append('    }')
        head = ['using ' + u + ';' for u in sorted(usings)] + ['using Microsoft.EntityFrameworkCore;',
                'using Microsoft.EntityFrameworkCore.Metadata.Builders;', '',
                f'namespace Capitra.Infrastructure.Persistence.Configurations.{ns_module};', '',
                f'public sealed class {cls}Configuration : IEntityTypeConfiguration<{cls}>', '{']
        body = '\n'.join(head + cfg + ['}']) + '\n'
        body = re.sub(r'\n\n+(\s+builder\.ConfigureAudit)', r'\n\n\1', body)
        body = re.sub(r'\n\n\n+', '\n\n', body)
        open(os.path.join(cfg_dir, cls + 'Configuration.cs'), 'w').write(body)

    out = os.path.join(os.path.dirname(__file__), f'map-{module.lower()}.json')
    json.dump(mapping, open(out, 'w'), indent=1)
    print(f'{len(tables)} tables -> {dom_dir}, {cfg_dir}, {out}')


if __name__ == '__main__':
    main()
