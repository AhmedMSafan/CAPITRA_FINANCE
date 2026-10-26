#!/usr/bin/env python3
"""Extracts the static application files (JS, CSS, images) from an APEX export.  usage: apex_files.py <fNNN.sql> <out dir>"""
import os
import re
import sys

text = open(sys.argv[1], encoding='utf-8', errors='replace').read()
out = sys.argv[2]
hexbuf = []
pos = 0
for m in re.finditer(r"g_varchar2_table := wwv_flow_imp\.empty_varchar2_table;|g_varchar2_table\(\d+\) := '([0-9A-Fa-f]*)';|create_app_static_file\((.*?)p_file_content", text, re.S):
    if m.group(0).startswith('g_varchar2_table :='):
        hexbuf = []
    elif m.group(1) is not None:
        hexbuf.append(m.group(1))
    else:
        name = re.search(r"p_file_name=>'([^']*)'", m.group(2)).group(1)
        path = os.path.join(out, name)
        os.makedirs(os.path.dirname(path), exist_ok=True)
        open(path, 'wb').write(bytes.fromhex(''.join(hexbuf)))
        print(name, os.path.getsize(path))
