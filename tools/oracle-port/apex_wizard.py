#!/usr/bin/env python3
"""
Generates the wizard-built pages of an APEX app (interactive reports and forms on one table) from the
split export (apex_pages.py) and the Oracle -> Capitra column map (map-gl.json):

  - C#: the region catalogue the server uses to read and change those tables (GlApexRegions.cs)
  - TS: the page definitions the web app renders (wizardPages.ts)

usage: apex_wizard.py <export app dir> <map json[,map json...]> <out .cs> <out .ts> <page> [<page> ...]
"""
import json
import os
import re
import sys

KIND = {'string': 'Text', 'text': 'Text', 'int': 'Number', 'decimal': 'Number', 'flag': 'Flag',
        'date': 'Date', 'datetime': 'DateTime', 'audit': 'Audit'}


def pascal(col):
    return ''.join(p[:1] + p[1:].lower() for p in col.split('_'))


def read(path):
    with open(path, encoding='utf-8') as f:
        return f.read()


def regions(page_dir):
    out = []
    rdir = os.path.join(page_dir, 'regions')
    for name in sorted(os.listdir(rdir)):
        text = read(os.path.join(rdir, name))
        head = dict(re.findall(r'^-- (\w+): (.*)$', text, re.M))
        m = re.search(r'type: (\S+)', text)
        q = re.search(r'query_type: (\S+) table: (\S+) where: (.*)$', text, re.M)
        rid = re.search(r'id: wwv_flow_imp\.id\((\d+)', text)
        seq = re.search(r'seq: (\d+)', text)
        out.append({
            'title': head.get('region', '').strip(),
            'type': m.group(1) if m else None,
            'table': q.group(2) if q and q.group(2) != 'None' else None,
            'where': q.group(3).strip() if q and q.group(3).strip() != 'None' else None,
            'id': rid.group(1) if rid else None,
            'seq': int(seq.group(1)) if seq else 0,
            'file': name,
        })
    return out


def items(page_dir):
    path = os.path.join(page_dir, 'items.txt')
    out = []
    if not os.path.exists(path):
        return out
    for line in read(path).splitlines():
        parts = [p.strip() for p in line.split(' | ')]
        if len(parts) < 3:
            continue
        kv = dict(p.split('=', 1) for p in parts[2:] if '=' in p)
        out.append({'name': parts[0], 'type': parts[1], 'label': kv.get('prompt'), 'source': kv.get('source'),
                    'lov': kv.get('lov') if kv.get('lov') not in (None, 'None') else None,
                    'region': re.sub(r'\D', '', kv.get('region', '')), 'seq': int(kv.get('seq') or 0),
                    'required': kv.get('required') == 'true'})
    return out


def buttons(page_dir):
    path = os.path.join(page_dir, 'buttons.txt')
    out = []
    if not os.path.exists(path):
        return out
    for line in read(path).splitlines():
        parts = [p.strip() for p in line.split(' | ')]
        kv = dict(p.split('=', 1) for p in parts[1:] if '=' in p)
        url = kv.get('url')
        target = re.match(r'f\?p=&APP_ID\.:(\d+):', url or '')
        out.append({'name': parts[0], 'label': kv.get('label'), 'action': kv.get('action'),
                    'region': re.sub(r'\D', '', kv.get('region', '')), 'position': kv.get('position'),
                    'page': int(target.group(1)) if target else None})
    return out


def columns(page_dir):
    path = os.path.join(page_dir, 'columns.txt')
    out = {}
    if os.path.exists(path):
        for line in read(path).splitlines():
            parts = [p.strip() for p in line.split(' | ')]
            kv = dict(p.split('=', 1) for p in parts[1:] if '=' in p)
            out[parts[0]] = {'heading': kv.get('heading'), 'type': kv.get('type')}
    return out


def readme(page_dir):
    text = read(os.path.join(page_dir, 'README.txt'))
    title = re.match(r'Page \d+: (.*)', text).group(1).strip()
    mode = re.search(r'^mode: (.*)$', text, re.M).group(1).strip()
    return title, mode == 'MODAL'


def key(title):
    return re.sub(r'[^A-Z0-9]+', '_', title.upper()).strip('_')


def main():
    app_dir, map_path, cs_out, ts_out = sys.argv[1:5]
    pages = [int(p) for p in sys.argv[5:]]
    cmap = {}
    for path in map_path.split(','):
        cmap.update(json.load(open(path, encoding='utf-8')))

    # reports on views (select * from VIEW): the view's columns in order, from the data export's _COLUMNS.csv
    views = {}
    colcsv = os.path.join(os.path.dirname(os.path.normpath(app_dir)), 'data', '_COLUMNS.csv')
    viewcols = {}
    if os.path.exists(colcsv):
        import csv
        for row in csv.DictReader(open(colcsv, encoding='utf-8')):
            viewcols.setdefault(row['TABLE_NAME'], []).append((int(row['COLUMN_ID']), row['COLUMN_NAME']))
    for p in pages:
        d = os.path.join(app_dir, 'p%04d' % p)
        for name in os.listdir(os.path.join(d, 'regions')):
            m = re.search(r'select \*\s+from (\w+)', read(os.path.join(d, 'regions', name)), re.I)
            if m and m.group(1).upper() in viewcols:
                views[p] = [c for _, c in sorted(viewcols[m.group(1).upper()])]

    # which page holds the form of each table (the IR's edit link and the Create button go there)
    form_page = {}
    info = {}
    for p in pages:
        d = os.path.join(app_dir, 'p%04d' % p)
        info[p] = {'dir': d, 'regions': regions(d), 'items': items(d), 'buttons': buttons(d), 'columns': columns(d)}
        info[p]['title'], info[p]['modal'] = readme(d)
        for r in info[p]['regions']:
            if r['type'] == 'NATIVE_FORM':
                form_page[r['table']] = p

    tables = {}
    specs = []
    cs_regions = []
    cs_lovs = []
    for p in pages:
        i = info[p]
        page = {'page': p, 'title': i['title'], 'modal': i['modal'], 'regions': []}
        for r in sorted(i['regions'], key=lambda r: r['seq']):
            if r['type'] == 'NATIVE_IR' and r['table'] is None and p in views:
                # a report on a view: its process is written by hand (IR_<key>), the columns come in the view's order
                cols = i['columns']
                order = [c for c in views[p] if c in cols]
                page['regions'].append({'key': key(r['title']), 'title': r['title'], 'kind': 'ir', 'table': None, 'pk': order[0],
                                        'columns': [{'name': c, 'heading': cols[c]['heading'], 'type': cols[c]['type']} for c in order]})
                continue
            if r['type'] not in ('NATIVE_IR', 'NATIVE_FORM') or r['table'] not in cmap:
                if r['type'] in ('NATIVE_IR', 'NATIVE_FORM'):
                    print('skipped (no table map):', p, r['title'], r['table'], file=sys.stderr)
                continue
            t = cmap[r['table']]
            tables[r['table']] = t
            k = key(r['title'])
            region = {'key': k, 'title': r['title'], 'kind': 'ir' if r['type'] == 'NATIVE_IR' else 'form',
                      'table': r['table'], 'pk': t['pk']}
            parent = None
            if r['where']:
                m = re.match(r'(\w+)\s*=\s*:?(P\d+_\w+)', r['where'])
                parent = (m.group(1), m.group(2))
                region['parentItem'] = parent[1]
            if region['kind'] == 'ir':
                cols = i['columns']
                order = [c for c in t['columns'] if c in cols] + [c for c in cols if c not in t['columns']]
                region['columns'] = [{'name': c, 'heading': cols[c]['heading'], 'type': cols[c]['type']} for c in order]
                fp = form_page.get(r['table'])
                if fp:
                    region['link'] = {'page': fp, 'item': 'P%d_%s' % (fp, t['pk'])}
                create = [b for b in i['buttons'] if b['region'] == r['id'] and b['position'] == 'RIGHT_OF_IR_SEARCH_BAR']
                if create:
                    region['create'] = {'label': create[0]['label'], 'page': create[0]['page']}
            else:
                its = [x for x in i['items'] if x['region'] == r['id'] and x['source']]
                region['items'] = [{'name': x['name'], 'column': x['source'], 'type': x['type'], 'label': x['label'],
                                    'required': x['required'],
                                    'kind': KIND.get(t['columns'].get(x['source'], {}).get('kind'), 'Text')}
                                   for x in sorted(its, key=lambda x: x['seq'])]
                # select lists: LOV TABLE.DISPLAY_COLUMN returning the column of the same name, else the table's key
                for x in its:
                    if x['type'] != 'NATIVE_SELECT_LIST' or not x['lov']:
                        continue
                    lt, dc = x['lov'].split('.')
                    if lt not in cmap:
                        print('skipped LOV (no table map):', x['name'], x['lov'], file=sys.stderr)
                        continue
                    tables[lt] = cmap[lt]
                    if dc not in cmap[lt]['columns'] and dc.endswith('_EN') and dc[:-3] in cmap[lt]['columns']:
                        dc = dc[:-3]
                    rc = x['source'] if x['source'] in cmap[lt]['columns'] else cmap[lt]['pk']
                    cs_lovs.append('        new(120, %d, "%s", %s, "%s", "%s", Permissions.Gl.SetupView),' % (p, x['name'], pascal(lt), dc, rc))
                    next(i for i in region['items'] if i['name'] == x['name'])['lov'] = True
                region['pkItem'] = 'P%d_%s' % (p, t['pk'])
                bs = [b for b in i['buttons'] if b['position'] != 'RIGHT_OF_IR_SEARCH_BAR']
                region['buttons'] = [{'name': b['name'], 'label': b['label'], 'action': b['action'],
                                      'position': b['position'], 'page': b['page']} for b in bs]
            page['regions'].append(region)
            edit = region['kind'] == 'form'
            cs_regions.append('        new(120, %d, "%s", %s, %s%s%s),' % (
                p, k, pascal(r['table']), 'Permissions.Gl.SetupView',
                ', Permissions.Gl.SetupEdit' if edit else ', null',
                (', "%s", "%s"' % parent) if parent else ''))
        specs.append(page)

    # C#: the tables and regions
    cs = ['// <auto-generated> tools/oracle-port/apex_wizard.py from the APEX export of App 120. Do not edit by hand. </auto-generated>',
          'using Capitra.Application.Modules.Apex;',
          'using Capitra.Application.Security;',
          'using Capitra.Domain.Modules.Fin;',
          'using Capitra.Domain.Modules.Gl;',
          'using K = Capitra.Application.Modules.Apex.ApexColumnKind;',
          '',
          'namespace Capitra.Application.Modules.Gl.Pages;',
          '',
          '/// <summary>The wizard-built reports and forms of App 120 and the tables behind them.</summary>',
          'public static class GlApexRegions',
          '{']
    for name, t in tables.items():
        cs.append('    private static readonly ApexTable %s = new("%s", typeof(%s), "%s",' % (pascal(name), name, t['class'], t['pk']))
        cs.append('    [')
        for oc, c in t['columns'].items():
            if c['kind'] == 'blob':
                continue
            cs.append('        new("%s", "%s", K.%s),' % (oc, pascal(c['column']), KIND[c['kind']]))
        cs.append('    ]);')
        cs.append('')
    cs.append('    public static readonly IReadOnlyList<ApexRegion> All =')
    cs.append('    [')
    cs.extend(cs_regions)
    cs.append('    ];')
    cs.append('')
    cs.append('    public static readonly IReadOnlyList<ApexLov> Lovs =')
    cs.append('    [')
    cs.extend(cs_lovs)
    cs.append('    ];')
    cs.append('}')
    with open(cs_out, 'w', encoding='utf-8') as f:
        f.write('\n'.join(cs) + '\n')

    ts = ['// Generated by tools/oracle-port/apex_wizard.py from the APEX export of App 120. Do not edit by hand.',
          "import type { WizardPage } from '@/modules/apex/wizard'",
          '',
          'export const wizardPages: WizardPage[] = ' + json.dumps(specs, indent=2, ensure_ascii=False)]
    with open(ts_out, 'w', encoding='utf-8') as f:
        f.write('\n'.join(ts) + '\n')


if __name__ == '__main__':
    main()
