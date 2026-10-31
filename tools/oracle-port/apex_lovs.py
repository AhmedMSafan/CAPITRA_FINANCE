#!/usr/bin/env python3
"""Extracts the shared lists of values (create_list_of_values) of an APEX export file into lovs-<app>.json.

usage: apex_lovs.py <export .sql> <out .json>
"""
import json
import re
import sys


def literal(text):
    """An APEX export value: 'text', wwv_flow_string.join(wwv_flow_t_varchar2('a','b')) or a number."""
    text = re.sub(r"'\s*\|\|\s*'", '', text)      # 'abc'||'def': one string the export split at a length limit
    parts = re.findall(r"'((?:[^']|'')*)'", text)
    return '\n'.join(p.replace("''", "'") for p in parts)


def blocks(src):
    for m in re.finditer(r'wwv_flow_imp_shared\.create_list_of_values\((.*?)\n\);\n(.*?)end;', src, re.S):
        yield m.group(1), m.group(2)


def main():
    src = open(sys.argv[1], encoding='utf-8', errors='replace').read()
    out = {}
    for head, rest in blocks(src):
        attrs = {}
        for k, v in re.findall(r'^,?p_(\w+)=>(.*?)(?=\n,p_|\Z)', head, re.S | re.M):
            attrs[k] = v
        name = literal(attrs['lov_name'])
        lov = {'type': literal(attrs.get('source_type', "'STATIC'")) or 'STATIC'}
        if 'lov_query' in attrs and lov['type'] == 'SQL':
            lov['query'] = literal(attrs['lov_query'])
        if 'query_table' in attrs:
            lov['table'] = literal(attrs['query_table'])
            lov['where'] = literal(attrs['query_where']) if 'query_where' in attrs else None
        for k in ('return_column_name', 'display_column_name', 'default_sort_column_name', 'default_sort_direction'):
            if k in attrs:
                lov[k.replace('_column_name', '').replace('_name', '')] = literal(attrs[k])
        if literal(attrs.get('location', "''")) == 'STATIC':
            lov['type'] = 'STATIC'
            values = []
            for d in re.finditer(r'create_static_lov_data\((.*?)\n\);', rest, re.S):
                seq = re.search(r'p_lov_disp_sequence=>(\d+)', d.group(1))
                disp = re.search(r"p_lov_disp_value=>('(?:[^']|'')*')", d.group(1))
                ret = re.search(r"p_lov_return_value=>('(?:[^']|'')*')", d.group(1))
                values.append((int(seq.group(1)) if seq else 0, literal(disp.group(1)), literal(ret.group(1))))
            lov['values'] = [[d, r] for _, d, r in sorted(values)]
        out[name] = lov
    with open(sys.argv[2], 'w', encoding='utf-8') as f:
        json.dump(out, f, indent=2, ensure_ascii=False)
    print(len(out), 'lists of values')


if __name__ == '__main__':
    main()
