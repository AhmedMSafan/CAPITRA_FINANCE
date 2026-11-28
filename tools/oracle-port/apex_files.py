#!/usr/bin/env python3
"""
Writes the Static Application Files of an APEX export (Shared Components > Static Application Files) to
web/public/apex/<app>/, where a copied page finds them as #APP_FILES# (the app's CSS file URLs and images).

usage: apex_files.py <export .sql> <app> [file ...]     (no files: all of them)
"""
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, '..', '..'))


def main():
    export, app, wanted = sys.argv[1], int(sys.argv[2]), set(sys.argv[3:])
    out = os.path.join(ROOT, 'web', 'public', 'apex', str(app))
    text = open(export, encoding='utf-8', errors='replace').read()
    for block in text.split('wwv_flow_imp.g_varchar2_table := wwv_flow_imp.empty_varchar2_table;')[1:]:
        m = re.search(r"create_app_static_file\(.*?p_file_name=>'([^']+)'", block, re.S)
        if not m or (wanted and m.group(1) not in wanted):
            continue
        data = bytes.fromhex(''.join(re.findall(r"g_varchar2_table\(\d+\) := '([0-9A-F]*)'", block)))
        path = os.path.join(out, m.group(1))
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, 'wb') as f:
            f.write(data)
        print(os.path.relpath(path, ROOT), len(data))


if __name__ == '__main__':
    main()
