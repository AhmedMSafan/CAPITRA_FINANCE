#!/usr/bin/env python3
"""
Generates the wizard-built pages of an APEX app (interactive reports and forms on one table) from the
split export (apex_pages.py) and the Oracle -> Capitra column map (map-gl.json):

  - C#: the region catalogue the server uses to read and change those tables (GlApexRegions.cs)
  - TS: the page definitions the web app renders (wizardPages.ts)

usage: apex_wizard.py <export app dir> <map json[,map json...]> <out .cs> <out .ts> <page> [<page> ...]
       apex_wizard.py <export app dir> <config .json>      (another app: see wizard-105.json)
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
            'link': link_target(head.get('detail_link')),
            'reportColumns': head['report_columns'].split(':') if head.get('report_columns', 'None') != 'None' and 'f?p=' not in head['report_columns'] else None,
            'template': head.get('template', ''),
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
                    'required': kv.get('required') == 'true', 'default': kv.get('default') if kv.get('default') not in (None, 'None') else None,
                    'submit': kv.get('on_select') == 'SUBMIT', 'sameLine': kv.get('new_line') == 'N'})
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
        target = link_target(url)
        out.append({'name': parts[0], 'label': kv.get('label'), 'action': kv.get('action'),
                    'region': re.sub(r'\D', '', kv.get('region', '')), 'position': kv.get('position'),
                    'page': target['page'] if target else None, 'target': target,
                    'cond': {'type': kv['cond_type'], 'item': kv.get('cond')} if kv.get('cond_type', 'None') != 'None' else None})
    return out


def columns(page_dir):
    path = os.path.join(page_dir, 'columns.txt')
    out = {}
    if os.path.exists(path):
        for line in read(path).splitlines():
            parts = [p.strip() for p in line.split(' | ')]
            kv = dict(p.split('=', 1) for p in parts[1:] if '=' in p)
            out[parts[0]] = {'heading': kv.get('heading'), 'type': kv.get('type'), 'display': kv.get('display'),
                             'order': int(kv['order']) if (kv.get('order') or 'None').isdigit() else 0,
                             'link': link_target(kv.get('link')), 'format': kv.get('format') if kv.get('format') != 'None' else None}
    return out


ALIASES = {}   # page alias (f?p=&APP_ID.:DASHBOARD:...) -> page number, from the README.txt of every page


def page_number(ref):
    return int(ref) if ref.isdigit() else ALIASES.get(ref.upper())


def link_target(url):
    """f?p=&APP_ID.:602:&APP_SESSION.::&DEBUG.:RP:P602_MEMO_ID,P602_PORTFOLIO_ID:#MEMO_ID#,&P601_PORTFOLIO_ID.
    as {page, set: {P602_MEMO_ID: '#MEMO_ID#', ...}}: #COL# is the row's column, &ITEM. a page item."""
    if re.fullmatch(r'\d+', url or ''):
        # a page number alone: the browser resolves it against the friendly URL .../r/<app>/<page>, so it opens that page
        return {'page': int(url), 'set': {}}
    m = re.match(r'f\?p=&APP_ID\.:(\w+):', url or '')
    page = page_number(m.group(1)) if m else None
    if page is None:
        return None
    parts = url[len('f?p='):].split(':')
    names = [n for n in (parts[6].split(',') if len(parts) > 6 else []) if n]
    values = parts[7].split(',') if len(parts) > 7 else []
    return {'page': page, 'set': {n: (values[k] if k < len(values) else '') for k, n in enumerate(names)}}


LOV_TYPES = ('NATIVE_SELECT_LIST', 'NATIVE_POPUP_LOV', 'NATIVE_RADIOGROUP', 'NATIVE_CHECKBOX')
# Universal Theme region templates that print the region's content alone
BLANK_TEMPLATES = ('4501440665235496320',)
FORM_POSITIONS = ('CLOSE', 'DELETE', 'NEXT', 'CREATE', 'CHANGE', 'PREVIOUS')


def page_item(x):
    item = {'name': x['name'], 'type': x['type'], 'label': x['label'], 'required': x['required']}
    if x['lov'] and x['type'] in LOV_TYPES:
        item['lov'] = True
    for k in ('default', 'submit', 'sameLine'):
        if x.get(k):
            item[k] = x[k]
    return item


def column(name, c):
    out = {'name': name, 'heading': c['heading'], 'type': c['type']}
    if c.get('link'):
        out['link'] = c['link']
    if c.get('format'):
        out['format'] = c['format']
    return out


def html_links(html, cfg):
    """href="f?p=&APP_ID.:711:&APP_SESSION.::&DEBUG.:RP:ITEMS:VALUES" in static HTML as the Capitra route of that page."""
    base = cfg.get('route', '/apex/%d/p' % cfg['app'])

    def route(m):
        t = link_target(m.group(1).replace('&amp;', '&'))
        if not t:
            return m.group(0)
        path = (cfg.get('home', base + '1') if t['page'] == 1 else base + str(t['page']))
        query = '&amp;'.join('%s=%s' % (k.lower(), v) for k, v in t['set'].items() if v)
        return 'href="%s"' % (path + ('?' + query if query else ''))
    return re.sub(r'href="(f\?p=[^"]*)"', route, html)


def template_classes(r):
    """The region's template options as classes (t-Region--noPadding, t-Region--scrollBody, margin-*)."""
    opts = r['template'].partition('opts: ')[2]
    return ' '.join(o for part in opts.split(':') for o in part.split() if o != '#DEFAULT#' and not o.startswith('js-'))


def region_buttons(i, r):
    return [{'name': b['name'], 'label': b['label'], 'action': b['action'], 'position': b['position'], 'page': b['page'],
             **({'set': b['target']['set']} if b['target'] and b['target']['set'] else {}),
             **({'cond': b['cond']} if b['cond'] else {})}
            for b in i['buttons'] if b['region'] == r['id']]


def report_links(region, r, i, form_page):
    """The row link (detail link, else the form page of the table) and the buttons right of the search bar."""
    if 'hideHeader' in r['template'] or 'removeHeader' in r['template']:
        region['hideHeader'] = True
    if r['link']:
        region['link'] = r['link']
    elif form_page:
        region['link'] = {'page': form_page, 'item': 'P%d_%s' % (form_page, region['pk'])}
    bar = [b for b in i['buttons'] if b['region'] == r['id'] and b['position'] == 'RIGHT_OF_IR_SEARCH_BAR']
    create = [b for b in bar if b['name'] == 'CREATE']
    if create:
        region['create'] = {'label': create[0]['label'], 'page': create[0]['page']}
        if create[0]['target'] and create[0]['target']['set']:
            region['create']['set'] = create[0]['target']['set']
    if len(bar) > 1 or (bar and bar[0]['name'] != 'CREATE'):
        region['toolbar'] = [{'name': b['name'], 'label': b['label'], 'page': b['page'],
                              **({'action': 'SUBMIT'} if b['action'] == 'SUBMIT' else {}),
                              **({'set': b['target']['set']} if b['target'] and b['target']['set'] else {})} for b in bar]


def readme(page_dir):
    text = read(os.path.join(page_dir, 'README.txt'))
    title = re.match(r'Page \d+: (.*)', text).group(1).strip()
    mode = re.search(r'^mode: (.*)$', text, re.M).group(1).strip()
    return title, mode == 'MODAL'


def key(title):
    return re.sub(r'[^A-Z0-9]+', '_', title.upper()).strip('_')


# App 120 (general ledger), the first app copied: its settings when no config file is given.
GL = {'app': 120, 'class': 'GlApexRegions', 'namespace': 'Capitra.Application.Modules.Gl.Pages',
      'usings': ['Capitra.Domain.Modules.Fin', 'Capitra.Domain.Modules.Gl'],
      'permissions': [{'from': 0, 'to': 99999, 'view': 'Permissions.Gl.SetupView', 'edit': 'Permissions.Gl.SetupEdit'}]}


def main():
    if sys.argv[2].endswith('.json') and len(sys.argv) == 3:
        here = os.path.dirname(os.path.abspath(__file__))
        cfg = json.load(open(sys.argv[2], encoding='utf-8'))
        app_dir = sys.argv[1]
        map_path = ','.join(os.path.join(here, m) for m in cfg['maps'])
        root = os.path.normpath(os.path.join(here, '..', '..'))
        cs_out, ts_out = os.path.join(root, cfg['cs']), os.path.join(root, cfg['ts'])
        pages = [p for r in cfg['pages'] + cfg.get('reports', []) for p in (range(r[0], r[1] + 1) if isinstance(r, list) else [r])]
        if cfg.get('custom') == 'all':
            # every page but the ones not copied: the custom ones render their PL/SQL regions through REGION_<key>
            pages += [int(d[1:]) for d in os.listdir(app_dir) if re.fullmatch(r'p\d{4}', d) and int(d[1:]) not in cfg.get('skip', [])]
        pages = sorted(set(pages))
        pages = [p for p in pages if os.path.isdir(os.path.join(app_dir, 'p%04d' % p))]
    else:
        app_dir, map_path, cs_out, ts_out = sys.argv[1:5]
        pages = [int(p) for p in sys.argv[5:]]
        cfg = GL
    app = cfg['app']
    for d in os.listdir(app_dir):
        if re.fullmatch(r'p\d{4}', d):
            m = re.search(r'^alias: (\S+)', read(os.path.join(app_dir, d, 'README.txt')), re.M)
            if m and m.group(1) != 'None':
                ALIASES[m.group(1).upper()] = int(d[1:])

    def perms(page, table):
        rule = next(r for r in cfg['permissions'] if r['from'] <= page <= r['to'])
        edit = next((v for k, v in cfg.get('tableEdit', {}).items() if cmap[table]['table'].startswith(k)), rule['edit'])
        return rule['view'], edit
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

    attrs_path = os.path.join(app_dir, 'region-attrs.json')
    attrs = json.load(open(attrs_path, encoding='utf-8')) if os.path.exists(attrs_path) else {}
    # the Breadcrumb's entries (breadcrumb.json, from apex_nav.py)
    crumbs_path = os.path.join(app_dir, 'breadcrumb.json')
    crumbs = json.load(open(crumbs_path, encoding='utf-8')) if os.path.exists(crumbs_path) else {}
    tables = {}
    specs = []
    cs_regions = []
    cs_lovs = []
    for p in pages:
        i = info[p]
        page = {'page': p, 'title': i['title'], 'modal': i['modal'], 'regions': []}
        for f, k in (('page.js', 'js'), ('page-onload.js', 'onload')):
            if os.path.exists(os.path.join(i['dir'], f)):
                page[k] = read(os.path.join(i['dir'], f))
        if os.path.exists(os.path.join(i['dir'], 'page.css')):
            page['css'] = read(os.path.join(i['dir'], 'page.css')).strip()
        # Before Header processes: the PAGE_LOAD process (sql-<app>/loads, or ported by hand) sets the page's items
        procs = os.path.join(i['dir'], 'processes')
        loads = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'sql-%d' % app, 'loads')
        if any(os.path.exists(os.path.join(loads, n % p)) for n in ('P%d.sql', 'P%04d.sql')) or (
                os.path.isdir(procs) and not any(r['type'] == 'NATIVE_FORM' for r in i['regions'])
                and any('point: BEFORE_HEADER' in read(os.path.join(procs, f)) for f in os.listdir(procs))):
            page['load'] = True
        # After Submit processes of a page that is no wizard form: its SUBMIT process runs them (REQUEST is the button)
        if os.path.isdir(procs) and not any(r['type'] == 'NATIVE_FORM' for r in i['regions']) and any(
                'point: AFTER_SUBMIT' in read(os.path.join(procs, f)) and 'Close Dialog' not in f.replace('-', ' ') for f in os.listdir(procs)):
            page['submit'] = True
        marks = []
        for r in sorted(i['regions'], key=lambda r: (r['type'] != 'NATIVE_BREADCRUMB', r['seq'])):  # the Breadcrumb Bar is above the body
            marks.append((len(page['regions']), r))
            if r['type'] == 'NATIVE_BREADCRUMB':
                # the page's entry and its parents, the root first; useBreadcrumbTitle shows the page's entry as the title
                entry = next((c for c in crumbs.values() if c['page'] == p), None)
                chain = []
                while entry:
                    chain.insert(0, {'label': entry['label'], 'page': entry['link']})
                    entry = crumbs.get(entry['parent']) if entry['parent'] else None
                if chain:
                    page['regions'].append({'key': key(r['title']), 'title': r['title'], 'kind': 'breadcrumb', 'table': None, 'pk': None,
                                            'crumbs': chain, 'cls': ' '.join(o for o in r['template'].split(':') if o.startswith('t-BreadcrumbRegion--'))})
                continue
            if r['type'] == 'NATIVE_IR' and r['table'] is None and p in views:
                # a report on a view: its process is written by hand (IR_<key>), the columns come in the view's order
                cols = i['columns']
                order = [c for c in views[p] if c in cols]
                page['regions'].append({'key': key(r['title']), 'title': r['title'], 'kind': 'ir', 'table': None, 'pk': order[0],
                                        'columns': [{'name': c, 'heading': cols[c]['heading'], 'type': cols[c]['type']} for c in order]})
                continue
            if r['type'] == 'NATIVE_IR' and r['table'] is None:
                # a report on a SQL query, translated by apex_sql.py (MtgApexSql: process IR_<key>)
                cols = i['columns']
                # no default report columns (or a link where they belong): every column that is not hidden, in display order
                shown = r['reportColumns'] or sorted((c for c in cols if cols[c]['display'] != 'HIDDEN_ESCAPE_SC'), key=lambda c: cols[c]['order'])
                region = {'key': key(r['title']), 'title': r['title'], 'kind': 'ir', 'table': None, 'pk': None,
                          'columns': [column(c, cols[c]) for c in shown if c in cols]}
                report_links(region, r, i, None)
                page['regions'].append(region)
                continue
            # a region whose buttons sit in form positions belongs to the wizard form, unless the page has no form
            # (a dialog of page items whose processes are hand-written): then it is a region of buttons
            has_form = any(x['type'] == 'NATIVE_FORM' for x in i['regions'])
            if r['type'] in (None, 'None', 'NATIVE_STATIC', 'NATIVE_PLSQL') and (not has_form or not any(b['region'] == r['id'] and b['position'] in FORM_POSITIONS for b in i['buttons'])):
                # a region of page items (the filters of a report) or of HTML; PL/SQL dynamic content runs on the server (REGION_<key>)
                its = sorted((x for x in i['items'] if x['region'] == r['id']), key=lambda x: x['seq'])
                shown_items = [x for x in its if x['type'] != 'NATIVE_HIDDEN']
                region = {'key': key(r['title']), 'title': r['title'], 'kind': 'plsql' if r['type'] == 'NATIVE_PLSQL' else 'items' if shown_items else 'html',
                          'table': None, 'pk': None}
                if region['kind'] == 'items':
                    region['items'] = [page_item(x) for x in its]
                    region['buttons'] = region_buttons(i, r)
                elif region['kind'] == 'html':
                    region['html'] = html_links(read(os.path.join(i['dir'], 'regions', r['file'])).partition('\n\n')[2].strip(), cfg)
                    region['buttons'] = region_buttons(i, r)
                    if not region['buttons']:
                        del region['buttons']
                    if not region['html'] and 'buttons' not in region:
                        continue
                if 'removeHeader' in r['template'] or 'hideHeader' in r['template']:
                    region['hideHeader'] = True
                cls = template_classes(r)
                if r['template'].split()[0] in BLANK_TEMPLATES:      # Blank with Attributes: the content alone
                    region['hideHeader'] = True
                    cls = (cls + ' apx-blank').strip()
                if cls:
                    region['cls'] = cls
                page['regions'].append(region)
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
                m = re.match(r'(\w+)\s*=\s*:?(P\d+_\w+)\s*$', r['where'])
                if m:
                    parent = (m.group(1), m.group(2))
                    region['parentItem'] = parent[1]
                else:
                    print('where not copied:', p, r['title'], r['where'], file=sys.stderr)
            if region['kind'] == 'ir':
                cols = i['columns']
                if r['reportColumns']:
                    region['columns'] = [column(c, cols[c]) for c in r['reportColumns'] if c in cols]
                else:
                    order = [c for c in t['columns'] if c in cols] + [c for c in cols if c not in t['columns']]
                    region['columns'] = [{'name': c, 'heading': cols[c]['heading'], 'type': cols[c]['type']} for c in order]
                report_links(region, r, i, form_page.get(r['table']))
            else:
                its = [x for x in i['items'] if x['region'] == r['id'] and (x['source'] or x['type'] == 'NATIVE_FILE')]
                region['items'] = [{**page_item(x), 'column': x['source'] if x['source'] != 'None' else None,
                                    'kind': KIND.get(t['columns'].get(x['source'], {}).get('kind'), 'Text')}
                                   for x in sorted(its, key=lambda x: x['seq'])]
                # select lists: LOV TABLE.DISPLAY_COLUMN returning the column of the same name, else the table's key;
                # a shared or SQL list of values comes from apex_sql.py (page_item marks it)
                for x in its:
                    if x['type'] not in LOV_TYPES or not x['lov'] or '.' not in x['lov'] or ' ' in x['lov']:
                        continue
                    lt, dc = x['lov'].split('.')
                    if lt not in cmap:
                        print('skipped LOV (no table map):', x['name'], x['lov'], file=sys.stderr)
                        continue
                    tables[lt] = cmap[lt]
                    if dc not in cmap[lt]['columns'] and dc.endswith('_EN') and dc[:-3] in cmap[lt]['columns']:
                        dc = dc[:-3]
                    rc = x['source'] if x['source'] in cmap[lt]['columns'] else cmap[lt]['pk']
                    cs_lovs.append('        new(%d, %d, "%s", %s, "%s", "%s", %s),' % (app, p, x['name'], pascal(lt), dc, rc, perms(p, r['table'])[0]))
                    next(i for i in region['items'] if i['name'] == x['name'])['lov'] = True
                region['pkItem'] = 'P%d_%s' % (p, t['pk'])
                bs = [b for b in i['buttons'] if b['position'] != 'RIGHT_OF_IR_SEARCH_BAR']
                region['buttons'] = [{'name': b['name'], 'label': b['label'], 'action': b['action'],
                                      'position': b['position'], 'page': b['page']} for b in bs]
            page['regions'].append(region)
            edit = region['kind'] == 'form'
            view, edit_perm = perms(p, r['table'])
            cs_regions.append('        new(%d, %d, "%s", %s, %s%s%s),' % (
                app, p, k, pascal(r['table']), view,
                ', ' + edit_perm if edit else ', null',
                (', "%s", "%s"' % parent) if parent else ''))
        seen = {}
        for region in page['regions']:              # two regions with one title: the second key gets _2
            seen[region['key']] = seen.get(region['key'], 0) + 1
            if seen[region['key']] > 1:
                region['key'] += '_%d' % seen[region['key']]
        # the region's Static ID and CSS Classes (region-attrs.json, from apex_region_attrs.py): page CSS and scripts find it by them
        for n, (start, r) in enumerate(marks):
            end = marks[n + 1][0] if n + 1 < len(marks) else len(page['regions'])
            a = attrs.get(r['id'] or '', {})
            for region in page['regions'][start:end]:
                for b in region.get('buttons', []):
                    if attrs.get('%s:%s' % (r['id'], b['name']), {}).get('staticId'):
                        b['staticId'] = attrs['%s:%s' % (r['id'], b['name'])]['staticId']
                if a.get('staticId'):
                    region['staticId'] = a['staticId']
                if a.get('cls'):
                    region['cls'] = (region.get('cls', '') + ' ' + a['cls']).strip()
        specs.append(page)

    # C#: the tables and regions
    cs = ['// <auto-generated> tools/oracle-port/apex_wizard.py from the APEX export of App %d. Do not edit by hand. </auto-generated>' % app,
          'using Capitra.Application.Modules.Apex;',
          'using Capitra.Application.Security;']
    cs += ['using %s;' % u for u in cfg['usings']]
    cs += ['using K = Capitra.Application.Modules.Apex.ApexColumnKind;',
          '',
          'namespace %s;' % cfg['namespace'],
          '',
          '/// <summary>The wizard-built reports and forms of App %d and the tables behind them.</summary>' % app,
          'public static class %s' % cfg['class'],
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

    if cfg.get('index'):
        # every page of the app (but the ones not copied): its title, whether it opens as a dialog, and whether it is a wizard page
        index = []
        for d in sorted(os.listdir(app_dir)):
            if re.fullmatch(r'p\d{4}', d) and int(d[1:]) not in cfg.get('skip', []):
                title, modal = readme(os.path.join(app_dir, d))
                index.append({'page': int(d[1:]), 'title': title, 'modal': modal, 'wizard': int(d[1:]) in pages})
        with open(os.path.join(root, cfg['index']), 'w', encoding='utf-8') as f:
            f.write('// Generated by tools/oracle-port/apex_wizard.py from the APEX export of App %d. Do not edit by hand.\n' % app)
            f.write('export type AppPage = { page: number; title: string; modal: boolean; wizard: boolean }\n\n')
            f.write('export const appPages: AppPage[] = [\n' + ''.join("  { page: %d, title: %s, modal: %s, wizard: %s },\n" % (x['page'], "'" + x['title'].replace("'", "\\'") + "'", str(x['modal']).lower(), str(x['wizard']).lower()) for x in index) + ']\n')

    ts = ['// Generated by tools/oracle-port/apex_wizard.py from the APEX export of App %d. Do not edit by hand.' % app,
          "import type { WizardPage } from '@/modules/apex/wizard'",
          '',
          'export const wizardPages: WizardPage[] = ' + json.dumps(specs, indent=2, ensure_ascii=False).replace('#APP_FILES#', '/apex/%d/' % app)]
    with open(ts_out, 'w', encoding='utf-8') as f:
        f.write('\n'.join(ts) + '\n')


if __name__ == '__main__':
    main()
