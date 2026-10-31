#!/usr/bin/env python3
"""
Turns the htp.p('...' || expr || '...') lines of a PL/SQL dynamic-content region into C# statements
that append to a StringBuilder `h`, using $$\"\"\"...\"\"\" raw strings ({{expr}} is interpolation).
PL/SQL expressions are mapped through a JSON file of exact replacements; an unmapped one stops the run.

usage: htp_to_cs.py <region.sql> <map.json>  (prints C# to stdout)
"""
import json
import re
import sys

src = open(sys.argv[1], encoding='utf-8').read()
mapping = json.load(open(sys.argv[2], encoding='utf-8'))


def split_concat(expr):
    """Split a PL/SQL concatenation into literals ('text') and expressions, respecting quotes and parentheses."""
    parts, buf, depth, i = [], '', 0, 0
    while i < len(expr):
        c = expr[i]
        if c == "'":
            j = i + 1
            while True:
                if expr[j] == "'" and j + 1 < len(expr) and expr[j + 1] == "'":
                    j += 2
                    continue
                if expr[j] == "'":
                    break
                j += 1
            buf += expr[i:j + 1]
            i = j + 1
            continue
        if c == '(':
            depth += 1
        elif c == ')':
            depth -= 1
        if depth == 0 and expr.startswith('||', i):
            parts.append(buf.strip())
            buf = ''
            i += 2
            continue
        buf += c
        i += 1
    if buf.strip():
        parts.append(buf.strip())
    return parts


out = []
for m in re.finditer(r"htp\.p\((.*?)\);\s*$", src, re.S | re.M):
    pieces = []
    for part in split_concat(m.group(1)):
        if re.fullmatch(r"'(?:[^']|'')*'", part, re.S):
            pieces.append(part[1:-1].replace("''", "'"))
        else:
            key = re.sub(r'\s+', ' ', part)
            tm = re.fullmatch(r"t\('((?:[^']|'')*)',\s*'((?:[^']|'')*)'\)", key)
            if tm and key not in mapping:
                # t('English', 'Arabic'): the bilingual span helper (ApexHtml.T)
                lit = lambda s: '"' + s.replace("''", "'").replace('\\', '\\\\').replace('"', '\\"') + '"'
                pieces.append('{{T(' + lit(tm.group(1)) + ', ' + lit(tm.group(2)) + ')}}')
                continue
            if key not in mapping:
                sys.exit(f'unmapped expression: {key}')
            pieces.append('{{' + mapping[key] + '}}')
    text = ''.join(pieces)
    out.append('h.AppendLine($$"""\n' + text + '\n""");')
print('\n'.join(out))
