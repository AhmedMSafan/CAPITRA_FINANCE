#!/usr/bin/env python3
"""
Translates the SQL of a copied APEX app's pages to PostgreSQL (ora2pg.py) and writes it as C#:

  - the interactive reports whose source is a SQL query (ApexSqlReport, process IR_<key>)
  - the select lists of the pages (ApexSqlLov, process LOVS): shared lists of values (lovs-<app>.json),
    inline SQL and STATIC: lists
  - the Before Header processes that select into page items (ApexSqlLoad, process PAGE_LOAD), from sql-<app>/loads

Every query is run against the local database (binds as null) and listed as OK or FAILED with the error.
A failing or untranslatable query gets a hand-written PostgreSQL version in sql-<app>/overrides/<name>.sql
(P301_PORTFOLIO_DOCUMENTS.sql for a report, LOV_FNV_DEVELOPER.sql for a list), used instead of the translation.

usage: apex_sql.py <export app dir> <config .json>     (the same config as apex_wizard.py; "reports" lists the pages)
"""
import json
import os
import re
import subprocess
import sys

from apex_wizard import key, items, readme, regions
from ora2pg import Translator

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, '..', '..'))
PG = ['psql', '-h', 'localhost', '-U', 'postgres', '-d', 'CAPITRA_DB', '-v', 'ON_ERROR_STOP=1', '-qAt']


def check(sql):
    """Runs the query with every bind null; returns None or the error."""
    probe = re.sub(r'@[A-Z][A-Z0-9_]*', 'NULL', sql)          # @P1_X::bigint -> NULL::bigint
    r = subprocess.run(PG + ['-c', f'select count(*) from ({probe}) q'], capture_output=True, text=True,
                       env={**os.environ, 'PGPASSWORD': os.environ.get('PGPASSWORD', 'devpass')})
    return None if r.returncode == 0 else (r.stderr.strip().splitlines() or ['failed'])[0]


def static_sql(spec):
    """STATIC:Display;Return,Display;Return as a query."""
    pairs = [p.split(';', 1) if ';' in p else [p, p] for p in spec[len('STATIC:'):].split(',')]
    q = lambda s: "'" + s.replace("'", "''") + "'"
    return ' union all '.join(f'select {q(d)} "D", {q(r)} "R"' for d, r in pairs)


def main():
    app_dir, cfg_path = sys.argv[1:3]
    cfg = json.load(open(cfg_path, encoding='utf-8'))
    app = cfg['app']
    sql_dir = os.path.join(HERE, 'sql-%d' % app)
    tr = Translator(cfg['maps'], os.path.join(sql_dir, 'views'))
    lovs = json.load(open(os.path.join(HERE, 'lovs-%d.json' % app), encoding='utf-8'))

    def pages_of(spec):
        return [p for r in spec for p in (range(r[0], r[1] + 1) if isinstance(r, list) else [r])
                if os.path.isdir(os.path.join(app_dir, 'p%04d' % p))]

    def perm(page):
        return next(r for r in cfg['permissions'] if r['from'] <= page <= r['to'])['view']

    def translate(name, oracle):
        path = os.path.join(sql_dir, 'overrides', name + '.sql')
        if os.path.exists(path):
            return open(path, encoding='utf-8').read().strip().rstrip(';'), ['hand-written']
        return tr.translate(oracle), list(tr.warnings)

    # the FNV_V_* views, from their Oracle source (sql-<app>/views-oracle) unless written by hand in views/
    vtr = Translator(cfg['maps'], None, view_body=True)
    os.makedirs(os.path.join(sql_dir, 'views'), exist_ok=True)
    for f in sorted(os.listdir(os.path.join(sql_dir, 'views-oracle'))):
        out = os.path.join(sql_dir, 'views', f)
        hand = os.path.join(sql_dir, 'overrides', 'VIEW_' + f)
        sql = open(hand, encoding='utf-8').read() if os.path.exists(hand) else vtr.translate(open(os.path.join(sql_dir, 'views-oracle', f), encoding='utf-8').read())
        open(out, 'w', encoding='utf-8').write(sql.strip() + '\n')

    results = []      # (name, sql, error, notes)
    reports = []
    for p in pages_of(cfg.get('reports', [])):
        d = os.path.join(app_dir, 'p%04d' % p)
        seen = {}
        # in the page's order, as apex_wizard.py keys them: two regions with one title, the second key gets _2
        for r in sorted(regions(d), key=lambda r: (r['type'] != 'NATIVE_BREADCRUMB', r['seq'])):
            k = key(r['title'])
            seen[k] = seen.get(k, 0) + 1
            if seen[k] > 1:
                k += '_%d' % seen[k]
            if r['type'] != 'NATIVE_IR' or r['table']:
                continue
            body = open(os.path.join(d, 'regions', r['file']), encoding='utf-8').read().split('\n\n', 1)[1].strip()
            name = 'P%d_%s' % (p, k)
            sql, notes = translate(name, body)
            results.append((name, sql, check(sql), notes))
            reports.append((p, k, name))

    lov_items = []
    used = {}
    # every page that is copied ("custom": "all" copies the pages that are not skipped)
    every = [int(d[1:]) for d in os.listdir(app_dir) if re.match(r'p\d{4}$', d)] if cfg.get('custom') == 'all' else []
    for p in sorted((set(pages_of(cfg['pages'])) | set(pages_of(cfg.get('reports', []))) | set(every)) - set(cfg.get('skip', []))):
        d = os.path.join(app_dir, 'p%04d' % p)
        for it in items(d):
            if not it['lov'] or it['type'] not in ('NATIVE_SELECT_LIST', 'NATIVE_POPUP_LOV', 'NATIVE_RADIOGROUP', 'NATIVE_CHECKBOX'):
                continue
            spec = it['lov']
            if spec.startswith('STATIC'):
                name, sql = 'LOV_%s' % it['name'], static_sql(spec)
                results.append((name, sql, check(sql), ['static']))
            elif re.match(r'\s*select\b', spec, re.I):
                full = open(os.path.join(d, 'items.txt'), encoding='utf-8').read()
                name = 'LOV_%s' % it['name']
                sql, notes = translate(name, spec)
                results.append((name, sql, check(sql), notes + ['inline (first line only in items.txt)'] if '\n' not in spec else notes))
            elif spec in lovs and lovs[spec]['type'] == 'SQL':
                name = 'LOV_%s' % spec
                if name not in used:
                    sql, notes = translate(name, lovs[spec]['query'])
                    used[name] = sql
                    results.append((name, sql, check(sql), notes))
            elif spec in lovs and lovs[spec]['type'] == 'STATIC':
                name = 'LOV_%s' % spec
                if name not in used:
                    sql = static_sql('STATIC:' + ','.join(f'{d};{r}' for d, r in lovs[spec]['values']))
                    used[name] = sql
                    results.append((name, sql, check(sql), ['static']))
            else:
                print('skipped list of values', p, it['name'], spec, file=sys.stderr)
                continue
            lov_items.append((p, it['name'], name))

    # Before Header processes that select into page items (sql-<app>/loads/P<page>.sql, the items as column aliases)
    loads = []
    load_dir = os.path.join(sql_dir, 'loads')
    for f in sorted(os.listdir(load_dir)) if os.path.isdir(load_dir) else []:
        p = int(re.match(r'P(\d+)\.sql$', f).group(1))
        oracle = '\n'.join(l for l in open(os.path.join(load_dir, f), encoding='utf-8').read().splitlines() if not l.lstrip().startswith('--'))
        name = 'LOAD_P%d' % p
        sql, notes = translate(name, oracle)
        results.append((name, sql, check(sql), notes))
        loads.append((p, name))

    # the queries of the ported PL/SQL regions and processes of the custom pages (sql-<app>/pages/P<page>.sql,
    # each query after a line "-- name: KEY"): constants P<page>_<KEY> of <Class>PageSql
    page_sql = []
    pages_dir = os.path.join(sql_dir, 'pages')
    for f in sorted(os.listdir(pages_dir)) if os.path.isdir(pages_dir) else []:
        pm = re.match(r'P(\d+)\.sql$', f)
        prefix = 'P%d' % int(pm.group(1)) if pm else f[:-4].upper()     # P0603.sql: P603_<KEY>; VALIDATION.sql: VALIDATION_<KEY>
        text = open(os.path.join(pages_dir, f), encoding='utf-8').read()
        for m in re.finditer(r'^-- name: (\w+)[^\n]*\n(.*?)(?=^-- name: |\Z)', text, re.S | re.M):
            name = '%s_%s' % (prefix, m.group(1))
            oracle = '\n'.join(l for l in m.group(2).splitlines() if not l.lstrip().startswith('--')).strip().rstrip(';')
            sql, notes = translate(name, oracle)
            results.append((name, sql, check(sql), notes))
            page_sql.append((name, sql))

    by_name = {n: s for n, s, _, _ in results if not any(n == q for q, _ in page_sql)}
    failed = [r for r in results if r[2]]
    with open(os.path.join(sql_dir, 'REPORT.md'), 'w', encoding='utf-8') as f:
        f.write(f'# App {app}: SQL translated to PostgreSQL\n\n{len(results) - len(failed)} of {len(results)} run.\n\n')
        for n, s, err, notes in results:
            f.write(f"- {'OK    ' if not err else 'FAILED'} {n}" + (f" ({'; '.join(notes)})" if notes else '') + (f"\n  `{err}`" if err else '') + '\n')

    def raw(sql):
        body = '\n'.join('        ' + l if l.strip() else '' for l in sql.splitlines())
        return '"""\n' + body + '\n        """'

    cs = ['// <auto-generated> tools/oracle-port/apex_sql.py from the APEX export of App %d. Do not edit by hand. </auto-generated>' % app,
          'using Capitra.Application.Modules.Apex;',
          'using Capitra.Application.Security;',
          '',
          'namespace %s;' % cfg['namespace'],
          '',
          '/// <summary>The SQL reports and select lists of App %d, translated from Oracle to PostgreSQL.</summary>' % app,
          'public static class %s' % cfg['class'].replace('Regions', 'Sql'),
          '{',
          '    public static readonly IReadOnlyList<ApexSqlReport> Reports =',
          '    [']
    for p, k, n in reports:
        cs.append('        new(%d, %d, "%s", %s, Q.%s),' % (app, p, k, perm(p), n))
    cs += ['    ];', '', '    public static readonly IReadOnlyList<ApexSqlLov> Lovs =', '    [']
    for p, item, n in lov_items:
        cs.append('        new(%d, %d, "%s", %s, Q.%s),' % (app, p, item, perm(p), n))
    cs += ['    ];', '', '    public static readonly IReadOnlyList<ApexSqlLoad> Loads =', '    [']
    for p, n in loads:
        cs.append('        new(%d, %d, %s, Q.%s),' % (app, p, perm(p), n))
    cs += ['    ];', '', '    private static class Q', '    {']
    for n in sorted(by_name):
        cs.append('        public const string %s = %s;' % (n, raw(by_name[n])))
        cs.append('')
    cs[-1:] = ['    }', '}']
    out = os.path.join(ROOT, cfg['cs'].replace('Regions.cs', 'Sql.cs'))
    with open(out, 'w', encoding='utf-8') as f:
        f.write('\n'.join(cs) + '\n')
    ps = ['// <auto-generated> tools/oracle-port/apex_sql.py from the APEX export of App %d. Do not edit by hand. </auto-generated>' % app,
          'namespace %s;' % cfg['namespace'], '',
          '/// <summary>The queries of the ported custom pages of App %d (sql-%d/pages), translated from Oracle to PostgreSQL.</summary>' % (app, app),
          'public static class %s' % cfg['class'].replace('Regions', 'PageSql'), '{']
    for n, q in page_sql:
        ps.append('    public const string %s = %s;' % (n, raw(q)))
        ps.append('')
    ps[-1:] = ['}'] if page_sql else [ps[-1], '}']
    with open(os.path.join(ROOT, cfg['cs'].replace('Regions.cs', 'PageSql.cs')), 'w', encoding='utf-8') as f:
        f.write('\n'.join(ps) + '\n')
    print(f'{len(results) - len(failed)} of {len(results)} queries run; {len(failed)} failed (see {os.path.relpath(sql_dir, ROOT)}/REPORT.md)')


if __name__ == '__main__':
    main()
