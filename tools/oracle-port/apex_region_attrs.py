"""Static ID and CSS Classes of every page region of an APEX export, which the split export leaves out.

usage: python3 apex_region_attrs.py <fNNN.sql> <split dir>   writes <split dir>/region-attrs.json
{"<region id>": {"staticId": "...", "cls": "..."}} keyed by the region id.
"""
import json
import os
import re
import sys

PLUG = re.compile(r"wwv_flow_imp_page\.create_page_plug\((.*?)\n\);", re.S)


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
    with open(os.path.join(sys.argv[2], 'region-attrs.json'), 'w', encoding='utf-8') as f:
        json.dump(out, f, indent=1, sort_keys=True)
    print(len(out), 'regions with a static id or css classes')


if __name__ == '__main__':
    main()
