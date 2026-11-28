"""Static ID and CSS Classes of every page region of an APEX export, which the split export leaves out.

usage: python3 apex_region_attrs.py <fNNN.sql> <split dir>   writes <split dir>/region-attrs.json
{"<region id>": {"staticId": "...", "cls": "..."}} keyed by the region id, and the buttons' Static ID by "<region id>:<NAME>".
"""
import json
import os
import re
import sys

PLUG = re.compile(r"wwv_flow_imp_page\.create_page_plug\((.*?)\n\);", re.S)
BUTTON = re.compile(r"wwv_flow_imp_page\.create_page_button\((.*?)\n\);", re.S)


def main():
    src = open(sys.argv[1], encoding='utf-8', errors='ignore').read()
    out = {}
    for m in PLUG.finditer(src):
        body = m.group(1)
        rid = re.search(r"p_id=>wwv_flow_imp\.id\((\d+)\)", body)
        if not rid:
            continue
        sid = re.search(r"p_static_id=>'([^']*)'", body)
        cls = re.search(r"p_region_css_classes=>'([^']*)'", body)
        if sid or cls:
            out[rid.group(1)] = {**({'staticId': sid.group(1)} if sid else {}), **({'cls': cls.group(1)} if cls else {})}
    # buttons: "<region id>:<BUTTON_NAME>" -> {"staticId": ...}
    for m in BUTTON.finditer(src):
        body = m.group(1)
        rid = re.search(r"p_button_plug_id=>wwv_flow_imp\.id\((\d+)\)", body)
        name = re.search(r"p_button_name=>'([^']*)'", body)
        sid = re.search(r"p_button_static_id=>'([^']*)'", body)
        if rid and name and sid:
            out[rid.group(1) + ':' + name.group(1)] = {'staticId': sid.group(1)}
    with open(os.path.join(sys.argv[2], 'region-attrs.json'), 'w', encoding='utf-8') as f:
        json.dump(out, f, indent=1, sort_keys=True)
    print(len(out), 'regions with a static id or css classes')


if __name__ == '__main__':
    main()
