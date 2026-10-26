#!/usr/bin/env python3
"""
Splits an APEX application export (fNNN.sql) into readable files per page:
  pNNNN/page.js, page.css, regions/<n>-<name>.<sql|html>, items.txt, buttons.txt,
  processes/<n>-<name>.sql, dynamic-actions.txt, plus app-level application processes.

usage: apex_pages.py <fNNN.sql> <out dir>
"""
import os
import re
import sys

STR = r"'((?:[^']|'')*)'"
# One piece of a string: 'text' or unistr('text with \00b7 escapes').
PIECE = r"(unistr\()?" + STR + r"(\))?"


def piece_text(m):
    t = unq(m.group(2))
    if m.group(1):
        t = re.sub(r"\\([0-9A-Fa-f]{4})", lambda e: chr(int(e.group(1), 16)), t)
    return t


def unq(s):
    return s.replace("''", "'")


def value(block, key):
    """Value of ,p_key=> in a create_* call: a quoted string or a wwv_flow_string.join(...) list."""
    m = re.search(r"[,(]\s*%s=>" % re.escape(key), block)
    if not m:
        return None
    rest = block[m.end():]
    if rest.startswith('wwv_flow_string.join(wwv_flow_t_varchar2('):
        parts = []
        i = len('wwv_flow_string.join(wwv_flow_t_varchar2(')
        while True:
            pm = re.match(r"\s*" + PIECE, rest[i:])
            if not pm:
                break
            piece = piece_text(pm)
            i += pm.end()
            # Long lines are split as  '...'\n||'...'
            while True:
                cm = re.match(r"\s*\|\|\s*" + PIECE, rest[i:])
                if not cm:
                    break
                piece += piece_text(cm)
                i += cm.end()
            parts.append(piece)
            cm = re.match(r"\s*,", rest[i:])
            if not cm:
                break
            i += cm.end()
        return '\n'.join(parts)
    um = re.match(PIECE + r"((?:\s*\|\|\s*" + PIECE + r")*)", rest)
    if um and um.group(1):
        # unistr('...') possibly joined with || to more pieces
        return ''.join(piece_text(pm) for pm in re.finditer(PIECE, um.group(0)))
    sm = re.match(STR + r"((?:\s*\|\|\s*" + STR + r")*)", rest)
    if sm:
        return unq(sm.group(1)) + ''.join(unq(x) for x in re.findall(r"\|\|\s*" + STR, sm.group(2)))
    nm = re.match(r"([^\n,)]+)", rest)
    return nm.group(1).strip() if nm else None


def one_line(text):
    """A value kept on one line of a .txt list (an inline LOV query spans lines)."""
    return re.sub(r'\s*\n\s*', ' ', text) if text else text


def safe(name):
    return re.sub(r'[^A-Za-z0-9]+', '-', name or 'unnamed').strip('-')[:60]


def main():
    src, out = sys.argv[1], sys.argv[2]
    text = open(src, encoding='utf-8', errors='replace').read()
    calls = list(re.finditer(r"wwv_flow_imp(?:_page|_shared)?\.(create_\w+)\(", text))
    page = None
    last_region = None
    counters = {}
    for idx, m in enumerate(calls):
        end = calls[idx + 1].start() if idx + 1 < len(calls) else len(text)
        block = "(" + text[m.end():end]
        kind = m.group(1)
        if kind == 'create_page':
            pid = int(value(block, 'p_id'))
            page = os.path.join(out, 'p%04d' % pid)
            os.makedirs(page, exist_ok=True)
            counters = {}
            with open(os.path.join(page, 'README.txt'), 'w') as f:
                f.write(f"Page {pid}: {value(block, 'p_name')}\nalias: {value(block, 'p_alias')}\n"
                        f"mode: {value(block, 'p_page_mode')}\n")
            for key, fn in (('p_javascript_code', 'page.js'), ('p_javascript_code_onload', 'page-onload.js'),
                            ('p_inline_css', 'page.css'), ('p_javascript_file_urls', 'js-files.txt'),
                            ('p_css_file_urls', 'css-files.txt')):
                v = value(block, key)
                if v:
                    open(os.path.join(page, fn), 'w').write(v)
            continue
        if kind == 'create_flow_process' or kind == 'create_application_process':
            d = os.path.join(out, 'app-processes')
            os.makedirs(d, exist_ok=True)
            open(os.path.join(d, safe(value(block, 'p_process_name')) + '.sql'), 'w').write(
                f"-- point: {value(block, 'p_process_point')}\n" + (value(block, 'p_process_sql_clob') or ''))
            continue
        if page is None:
            continue
        n = counters[kind] = counters.get(kind, 0) + 1
        if kind == 'create_page_plug':
            d = os.path.join(page, 'regions')
            os.makedirs(d, exist_ok=True)
            name = value(block, 'p_plug_name')
            body = value(block, 'p_plug_source') or ''
            qry = value(block, 'p_query_table')
            head = (f"-- region: {name}\n-- type: {value(block, 'p_plug_source_type')}\n"
                    f"-- template: {value(block, 'p_plug_template')} opts: {value(block, 'p_region_template_options')}\n"
                    f"-- position: {value(block, 'p_plug_display_point')} seq: {value(block, 'p_plug_display_sequence')}\n"
                    f"-- parent: {value(block, 'p_parent_plug_id')} id: {value(block, 'p_id')}\n"
                    f"-- query_type: {value(block, 'p_query_type')} table: {qry} where: {value(block, 'p_query_where')}\n"
                    f"-- condition: {value(block, 'p_plug_display_condition_type')} {value(block, 'p_plug_display_when_condition')}\n")
            last_region = os.path.join(d, '%02d-%s.txt' % (n, safe(name)))
            open(last_region, 'w').write(head + '\n' + body)
        elif kind == 'create_worksheet' and last_region:
            # the interactive report's row link (the edit icon), e.g. f?p=&APP_ID.:12:...:P12_STATUS_CODE:\#STATUS_CODE#\
            head, _, body = open(last_region).read().partition('\n\n')
            link = (value(block, 'p_detail_link') or '').replace('\\', '')
            open(last_region, 'w').write(head + f"\n-- detail_link: {link or None}\n\n" + body)
        elif kind == 'create_worksheet_rpt' and last_region and value(block, 'p_is_default') == 'Y' and value(block, 'p_report_columns'):
            # the columns the default report shows, in order (the rest are hidden in the Actions menu)
            head, _, body = open(last_region).read().partition('\n\n')
            if '-- report_columns:' not in head:
                open(last_region, 'w').write(head + f"\n-- report_columns: {value(block, 'p_report_columns')}\n\n" + body)
        elif kind == 'create_page_item':
            with open(os.path.join(page, 'items.txt'), 'a') as f:
                f.write(f"{value(block, 'p_name')} | {value(block, 'p_display_as')} | prompt={value(block, 'p_prompt')} | "
                        f"source={value(block, 'p_source')} | lov={one_line(value(block, 'p_lov') or value(block, 'p_named_lov'))} | "
                        f"region={value(block, 'p_item_plug_id')} | seq={value(block, 'p_item_sequence')} | "
                        f"default={value(block, 'p_item_default')} | required={value(block, 'p_is_required')} | "
                        f"on_select={'SUBMIT' if re.search(r"'page_action_on_selection', 'SUBMIT'", block) else None} | "
                        f"new_line={value(block, 'p_begin_on_new_line')} | null={value(block, 'p_lov_display_null')}\n")
        elif kind == 'create_page_button':
            with open(os.path.join(page, 'buttons.txt'), 'a') as f:
                f.write(f"{value(block, 'p_button_name')} | label={value(block, 'p_button_image_alt')} | "
                        f"action={value(block, 'p_button_action')} | region={value(block, 'p_button_plug_id')} | "
                        f"position={value(block, 'p_button_position')} | url={value(block, 'p_button_redirect_url')}\n")
        elif kind == 'create_page_process':
            d = os.path.join(page, 'processes')
            os.makedirs(d, exist_ok=True)
            name = value(block, 'p_process_name')
            body = value(block, 'p_process_sql_clob') or ''
            attrs = {k: value(block, k) for k in ('p_attribute_01', 'p_attribute_02', 'p_attribute_04')}
            open(os.path.join(d, '%02d-%s.sql' % (n, safe(name))), 'w').write(
                f"-- process: {name}\n-- type: {value(block, 'p_process_type')} point: {value(block, 'p_process_point')}\n"
                f"-- when button: {value(block, 'p_process_when_button_id')} condition: {value(block, 'p_process_when')}\n"
                f"-- region: {value(block, 'p_process_region_id')} attrs: {attrs}\n\n" + body)
        elif kind in ('create_page_da_event', 'create_page_da_action'):
            with open(os.path.join(page, 'dynamic-actions.txt'), 'a') as f:
                f.write(f"{kind[15:]} | {value(block, 'p_name')} | {value(block, 'p_action') or value(block, 'p_bind_event_type')} | "
                        f"{(value(block, 'p_attribute_01') or '')[:4000]}\n")
        elif kind == 'create_report_region' or kind == 'create_report_columns' or kind == 'create_region_column':
            with open(os.path.join(page, 'columns.txt'), 'a') as f:
                f.write(f"{value(block, 'p_name') or value(block, 'p_column_alias')} | heading={value(block, 'p_heading') or value(block, 'p_column_heading')} | "
                        f"type={value(block, 'p_item_type') or value(block, 'p_column_type')}\n")
        elif kind == 'create_worksheet_column':
            with open(os.path.join(page, 'columns.txt'), 'a') as f:
                f.write(f"{value(block, 'p_db_column_name')} | heading={value(block, 'p_column_label')} | "
                        f"type={value(block, 'p_column_type')} | display={value(block, 'p_display_text_as')} | "
                        f"link={(value(block, 'p_column_link') or '').replace(chr(92), '') or None} | "
                        f"format={value(block, 'p_format_mask')} | order={value(block, 'p_display_order')}\n")
    print('done', out)


if __name__ == '__main__':
    main()
