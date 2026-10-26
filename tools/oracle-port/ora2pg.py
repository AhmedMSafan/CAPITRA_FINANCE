#!/usr/bin/env python3
"""
Translates the SQL of a copied APEX page (report queries, lists of values) from Oracle to PostgreSQL on the
Capitra tables:

  - Oracle tables and columns become the Capitra ones (map-*.json), quoted in UPPERCASE; Y/N flags become booleans
    (FLAG = 'Y' -> "IS_FLAG", nvl(FLAG,'N') = 'Y' -> coalesce("IS_FLAG", false)).
  - Select-list columns keep their Oracle names (AS "VERIFIED_FLAG"), because the page's report columns use them.
  - nvl, nvl2, decode, sysdate, trunc, listagg, instr, add_months, from dual and || (which ignores nulls in Oracle)
    become their PostgreSQL forms; a / b divides as numeric, as Oracle does.
  - :ITEM and v('ITEM') binds become @ITEM parameters.
  - FNV_V_* views (sql-<app>/views/<VIEW>.sql, written in PostgreSQL) are added as WITH queries, as the database
    holds no views.

What it cannot translate is reported (rownum, (+) joins, months_between, connect by); such a query gets a
hand-written PostgreSQL version in sql-<app>/overrides/<key>.sql, which is used instead.

Used by apex_sql.py; `ora2pg.py "<sql>"` prints one translation.
"""
import json
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))

KEYWORDS = set('''
select from where and or not in is null like between exists case when then else end as join left right inner outer
full cross on group by order having union all distinct asc desc nulls first last with over partition rows range
within fetch next only offset limit interval true false any some lateral minus intersect escape date timestamp
current_date current_timestamp localtimestamp unbounded preceding following current row
'''.split())

TOKEN = re.compile(r"""
    (?P<ws>\s+)
  | (?P<comment>--[^\n]*|/\*.*?\*/)
  | (?P<str>'(?:[^']|'')*')
  | (?P<qid>"[^"]+")
  | (?P<bind>:[A-Za-z][A-Za-z0-9_$#]*)
  | (?P<subst>&[A-Za-z][A-Za-z0-9_]*\.)
  | (?P<num>\d+(?:\.\d+)?)
  | (?P<id>[A-Za-z_][A-Za-z0-9_$#]*)
  | (?P<op>::|\|\||<=|>=|<>|!=|=>|\(\+\)|[=<>.,()*+\-/;%])
""", re.S | re.X)


class Tok:
    __slots__ = ('kind', 'text')

    def __init__(self, kind, text):
        self.kind, self.text = kind, text

    def __repr__(self):
        return f'{self.kind}:{self.text}'

    @property
    def low(self):
        return self.text.lower()


def tokenize(sql):
    out, pos = [], 0
    while pos < len(sql):
        m = TOKEN.match(sql, pos)
        if not m:
            raise ValueError(f'cannot read SQL at: {sql[pos:pos + 30]!r}')
        pos = m.end()
        kind = m.lastgroup
        if kind in ('ws', 'comment'):
            continue
        out.append(Tok(kind, m.group()))
    return out


def load_maps(paths):
    tables = {}
    for p in paths:
        for oracle, t in json.load(open(os.path.join(HERE, p), encoding='utf-8')).items():
            tables[oracle] = t
    return tables


class Translator:
    def __init__(self, maps, views_dir=None, view_body=False):
        self.tables = load_maps(maps)
        # translating a view: its Y/N flag columns stay 'Y'/'N' text (the pages compare them with 'Y'),
        # and the views it uses are added by the query that uses it
        self.view_body = view_body
        self.ctes = set()
        self.views_dir = views_dir
        self.warnings = []

    # ------------------------------------------------------------------------------------------- helpers
    @staticmethod
    def match_paren(toks, i):
        """Index of the ) that closes the ( at i."""
        depth = 0
        for j in range(i, len(toks)):
            if toks[j].text == '(':
                depth += 1
            elif toks[j].text == ')':
                depth -= 1
                if depth == 0:
                    return j
        raise ValueError('unbalanced parentheses')

    @staticmethod
    def split_args(toks):
        """The comma-separated arguments of a call's inside."""
        args, cur, depth = [], [], 0
        for t in toks:
            if t.text == '(':
                depth += 1
            elif t.text == ')':
                depth -= 1
            if t.text == ',' and depth == 0:
                args.append(cur)
                cur = []
            else:
                cur.append(t)
        args.append(cur)
        return args

    def primary_end(self, toks, i):
        """End (exclusive) of the primary expression starting at i: literal, column, call, (...) or CASE ... END."""
        t = toks[i]
        if t.text == '(':
            return self.match_paren(toks, i) + 1
        if t.low == 'case':
            depth = 0
            for j in range(i, len(toks)):
                if toks[j].low == 'case':
                    depth += 1
                elif toks[j].low == 'end':
                    depth -= 1
                    if depth == 0:
                        return j + 1
            raise ValueError('CASE without END')
        if t.text in ('-', '+') and i + 1 < len(toks):
            return self.primary_end(toks, i + 1)
        j = i + 1
        if t.kind in ('id', 'qid'):
            while j + 1 < len(toks) and toks[j].text == '.' and toks[j + 1].kind in ('id', 'qid', 'op') and toks[j + 1].text != '(':
                j += 2
            if j < len(toks) and toks[j].text == '(':
                j = self.match_paren(toks, j) + 1
                # listagg(...) within group (order by ...), f(...) over (...)
                if j + 2 < len(toks) and toks[j].low == 'within' and toks[j + 1].low == 'group':
                    j = self.match_paren(toks, j + 2) + 1
                if j < len(toks) and toks[j].low == 'over':
                    j = self.match_paren(toks, j + 1) + 1
        if j < len(toks) and toks[j].text == '::':
            j += 2
        return j

    # ------------------------------------------------------------------------------------------- name resolution
    def scan_aliases(self, toks):
        """alias -> [oracle tables] for every FROM/JOIN item, and the views the query uses."""
        aliases, views = {}, []
        # WITH queries: name AS ( ... ) after WITH or a comma that follows a WITH query's closing paren
        self.ctes = set()
        for i, t in enumerate(toks):
            if t.kind == 'id' and i + 2 < len(toks) and toks[i + 1].low == 'as' and toks[i + 2].text == '(' and i > 0 and (
                    toks[i - 1].low == 'with' or (toks[i - 1].text == ',' and i > 1 and toks[i - 2].text == ')')):
                self.ctes.add(t.text.upper())
                # a WITH query that selects x.* passes the columns of x's table on under their Capitra names
                close = self.match_paren(toks, i + 2)
                body = toks[i + 3:close]
                star = [body[k - 2].text.upper() for k in range(2, len(body)) if body[k].text == '*' and body[k - 1].text == '.']
                inner, _ = self.scan_aliases_plain(body)
                aliases.setdefault(t.text.upper(), []).extend(x for a in star for x in inner.get(a, []))
        prev = None
        for i, t in enumerate(toks):
            if t.kind == 'id' and prev is not None and (prev.low in ('from', 'join') or (prev.text == ',' and self.in_from(toks, i))):
                name = t.text.upper()
                if name in self.tables or name.startswith('FNV_V_') or self.view_sql(name):
                    if name.startswith('FNV_V_') or (name not in self.tables and self.view_sql(name)):
                        views.append(name)
                    alias = name
                    n = toks[i + 1] if i + 1 < len(toks) else None
                    if n is not None and n.kind == 'id' and n.low not in KEYWORDS and n.low not in ('where', 'join'):
                        alias = n.text.upper()
                    elif n is not None and n.low == 'as' and i + 2 < len(toks):
                        alias = toks[i + 2].text.upper()
                    aliases.setdefault(alias, []).append(name)
                    aliases.setdefault(name, []).append(name)
            prev = t
        return aliases, views

    def scan_aliases_plain(self, toks):
        """alias -> [oracle tables] of a part of a query, without WITH handling."""
        aliases = {}
        for i in range(1, len(toks)):
            t, prev = toks[i], toks[i - 1]
            if t.kind == 'id' and (prev.low in ('from', 'join') or (prev.text == ',' and self.in_from(toks, i))) and t.text.upper() in self.tables:
                n = toks[i + 1] if i + 1 < len(toks) else None
                alias = n.text.upper() if n is not None and n.kind == 'id' and n.low not in KEYWORDS else t.text.upper()
                aliases.setdefault(alias, []).append(t.text.upper())
        return aliases, []

    @staticmethod
    def in_from(toks, i):
        """True when token i sits in a FROM list (after FROM, before WHERE/GROUP/ORDER/...) at its paren depth."""
        depth = 0
        for j in range(i - 1, -1, -1):
            t = toks[j]
            if t.text == ')':
                depth += 1
            elif t.text == '(':
                if depth == 0:
                    return False
                depth -= 1
            elif depth == 0:
                if t.low == 'from':
                    return True
                if t.low in ('select', 'where', 'group', 'order', 'having', 'union', 'set', 'values', 'by'):
                    return False
        return False

    def column(self, oracle_tables, col):
        """(capitra column, kind) of an Oracle column in one of the tables, or None."""
        for t in oracle_tables:
            m = self.tables.get(t)
            if m and col in m['columns']:
                c = m['columns'][col]
                return c['column'], c['kind']
        return None

    def view_sql(self, name):
        if not self.views_dir:
            return None
        p = os.path.join(self.views_dir, name + '.sql')
        return open(p, encoding='utf-8').read().strip().rstrip(';') if os.path.exists(p) else None

    # ------------------------------------------------------------------------------------------- translation
    def translate(self, sql):
        self.warnings = []
        sql = re.sub(r"\bv\(\s*'([A-Z0-9_]+)'\s*\)", r':\1', sql, flags=re.I)
        toks = tokenize(sql.strip().rstrip(';'))
        for bad, why in (('rownum', 'rownum'), ('(+)', 'Oracle (+) outer join'), ('connect', 'connect by'),
                         ('months_between', 'months_between'), ('regexp_substr', 'regexp_substr'), ('keep', 'keep (dense_rank)')):
            if any(t.low == bad for t in toks):
                self.warnings.append('needs a hand translation: ' + why)
        aliases, views = self.scan_aliases(toks)
        all_tables = sorted({t for ts in aliases.values() for t in ts})
        toks = self.rewrite_functions(toks)
        toks = self.rewrite_flags(toks, aliases, all_tables)
        out = self.emit(toks, aliases, all_tables)
        if views and not self.view_body:
            out = self.with_views(out, views)
        return out

    def rewrite_functions(self, toks):
        out = []
        i = 0
        while i < len(toks):
            t = toks[i]
            low = t.low
            nxt = toks[i + 1] if i + 1 < len(toks) else None
            call = t.kind == 'id' and nxt is not None and nxt.text == '('
            if call and low in ('to_clob', 'to_nclob'):
                close = self.match_paren(toks, i + 1)
                out += [Tok('op', '(')] + self.rewrite_functions(toks[i + 2:close]) + [Tok('op', ')')]
                i = close + 1
                continue
            if call and low in ('nvl2', 'decode', 'trunc', 'listagg', 'add_months', 'instr'):
                close = self.match_paren(toks, i + 1)
                args = [self.rewrite_functions(a) for a in self.split_args(toks[i + 2:close])]
                end = close + 1
                if low == 'nvl2' and len(args) == 3:
                    rep = tokenize('(case when') + args[0] + tokenize('is not null then') + args[1] + [Tok('id', 'else')] + args[2] + tokenize('end)')
                elif low == 'decode':
                    rep = tokenize('(case')
                    for k in range(1, len(args) - 1, 2):
                        null = len(args[k]) == 1 and args[k][0].low == 'null'
                        rep += [Tok('id', 'when')] + args[0] + (tokenize('is null') if null else [Tok('op', '=')] + args[k]) + [Tok('id', 'then')] + args[k + 1]
                    if len(args) % 2 == 0:
                        rep += [Tok('id', 'else')] + args[-1]
                    rep += tokenize('end)')
                elif low == 'trunc' and len(args) == 1:
                    a = args[0]
                    if len(a) == 1 and a[0].low in ('sysdate', 'systimestamp', 'current_date', 'localtimestamp', 'now()'):
                        rep = [Tok('id', 'current_date')]
                    elif self.datey(a):
                        rep = [Tok('op', '(')] + a + tokenize(')::date')
                    else:
                        rep = toks[i:end]
                        rep = [Tok('id', 'trunc'), Tok('op', '(')] + a + [Tok('op', ')')]
                elif low == 'trunc' and len(args) == 2 and args[1] and args[1][0].kind == 'str':
                    unit = {"'MM'": 'month', "'MONTH'": 'month', "'YYYY'": 'year', "'YEAR'": 'year', "'Q'": 'quarter', "'DD'": 'day'}.get(args[1][0].text.upper(), 'day')
                    rep = tokenize(f"date_trunc('{unit}',") + args[0] + tokenize(')::date')
                elif low == 'add_months' and len(args) == 2:
                    rep = tokenize('((') + args[0] + tokenize(') + make_interval(months =>') + args[1] + tokenize('))::date')
                elif low == 'instr' and len(args) == 2:
                    rep = tokenize('strpos(') + args[0] + [Tok('op', ',')] + args[1] + [Tok('op', ')')]
                elif low == 'listagg':
                    sep = args[1] if len(args) > 1 else tokenize("','")
                    rep = tokenize('string_agg((') + args[0] + tokenize(')::text,') + sep
                    if end + 1 < len(toks) and toks[end].low == 'within' and toks[end + 1].low == 'group':
                        gclose = self.match_paren(toks, end + 2)
                        rep += self.rewrite_functions(toks[end + 3:gclose])
                        end = gclose + 1
                    rep += [Tok('op', ')')]
                else:
                    rep = [t, Tok('op', '(')] + [x for k, a in enumerate(args) for x in ([Tok('op', ',')] if k else []) + a] + [Tok('op', ')')]
                out += rep
                i = end
                continue
            if low == 'dbms_lob' and nxt is not None and nxt.text == '.' and i + 3 < len(toks) and toks[i + 3].text == '(':
                fn = toks[i + 2].low
                close = self.match_paren(toks, i + 3)
                args = [self.rewrite_functions(a) for a in self.split_args(toks[i + 4:close])]
                if fn == 'substr':
                    # dbms_lob.substr(lob, amount, offset) -> substr(lob, offset, amount)
                    offset = args[2] if len(args) > 2 else tokenize('1')
                    amount = args[1] if len(args) > 1 else tokenize('32767')
                    out += tokenize('substr(') + args[0] + [Tok('op', ',')] + offset + [Tok('op', ',')] + amount + [Tok('op', ')')]
                elif fn == 'getlength':
                    out += tokenize('length(') + args[0] + [Tok('op', ')')]
                else:
                    self.warnings.append('needs a hand translation: dbms_lob.' + fn)
                    out += toks[i:close + 1]
                i = close + 1
                continue
            if call and low == 'nvl':
                out.append(Tok('id', 'coalesce'))
            elif low == 'sysdate' and t.kind == 'id':
                out.append(Tok('id', 'localtimestamp'))
            elif low == 'systimestamp' and t.kind == 'id':
                out.append(Tok('id', 'now()'))
            elif low == 'from' and nxt is not None and nxt.low == 'dual':
                i += 2
                continue
            else:
                out.append(t)
            i += 1
        return out

    @staticmethod
    def datey(toks):
        text = ' '.join(t.text.upper() for t in toks)
        return bool(re.search(r'DATE|_ON\b|_AT\b|SYSDATE|LOCALTIMESTAMP|EXPIRY|ISSUE|MATURITY|DUE', text))

    def flag_ref(self, toks, i, aliases, all_tables):
        """(end, translated tokens) if toks[i:] is a reference to a Capitra boolean column."""
        if toks[i].kind != 'id':
            return None
        if i + 2 < len(toks) and toks[i + 1].text == '.' and toks[i + 2].kind == 'id':
            alias, col, end = toks[i].text.upper(), toks[i + 2].text.upper(), i + 3
            c = self.column(aliases.get(alias, []), col)
        else:
            if i > 0 and toks[i - 1].text == '.':
                return None
            col, end = toks[i].text.upper(), i + 1
            c = self.column(all_tables, col)
        if c and c[1] == 'flag':
            return end
        return None

    def rewrite_flags(self, toks, aliases, all_tables):
        """FLAG = 'Y' -> FLAG; FLAG = 'N' / FLAG <> 'Y' -> not FLAG; coalesce(FLAG,'N') = 'Y' -> coalesce(FLAG,false)."""
        out = []
        i = 0
        while i < len(toks):
            # coalesce(FLAG, 'N'|'Y') [= 'Y'|'N']
            if toks[i].low == 'coalesce' and i + 1 < len(toks) and toks[i + 1].text == '(':
                close = self.match_paren(toks, i + 1)
                inner = toks[i + 2:close]
                args = self.split_args(inner)
                if len(args) == 2 and args[1] and len(args[1]) == 1 and args[1][0].text in ("'Y'", "'N'"):
                    end = self.flag_ref(args[0], 0, aliases, all_tables)
                    if end == len(args[0]):
                        default = 'true' if args[1][0].text == "'Y'" else 'false'
                        expr = [Tok('id', 'coalesce'), Tok('op', '(')] + args[0] + tokenize(f', {default})')
                        rest = close + 1
                        if rest + 1 < len(toks) and toks[rest].text in ('=', '<>', '!=') and toks[rest + 1].text in ("'Y'", "'N'"):
                            neg = (toks[rest].text == '=') != (toks[rest + 1].text == "'Y'")
                            out += ([Tok('id', 'not')] if neg else []) + expr
                            i = rest + 2
                        else:
                            out += expr
                            i = rest
                        continue
            end = self.flag_ref(toks, i, aliases, all_tables)
            if end is not None and end + 1 < len(toks) and toks[end].text in ('=', '<>', '!=') and toks[end + 1].kind in ('bind', 'subst'):
                # FLAG = :P1_FLAG (a Y/N item) -> FLAG = (:P1_FLAG = 'Y')
                out += toks[i:end] + [toks[end], Tok('op', '('), toks[end + 1], Tok('op', '='), Tok('str', "'Y'"), Tok('op', ')')]
                i = end + 2
                continue
            if end is not None and end + 1 < len(toks) and toks[end].text in ('=', '<>', '!=') and toks[end + 1].text in ("'Y'", "'N'"):
                neg = (toks[end].text == '=') != (toks[end + 1].text == "'Y'")
                out += ([Tok('id', 'not')] if neg else []) + toks[i:end]
                i = end + 2
                continue
            if end is not None and end + 2 < len(toks) and toks[end].low == 'in' and toks[end + 1].text == '(' and toks[end + 2].text == "'Y'" and toks[end + 3].text == ')':
                out += toks[i:end]
                i = end + 4
                continue
            out.append(toks[i])
            i += 1
        return out

    def emit(self, toks, aliases, all_tables):
        """Quote names, rename tables and columns, turn || chains into concat() and / into numeric division."""
        types = self.bind_types(toks, aliases, all_tables)
        toks = self.numeric_division(self.concat_chains(toks))
        res = []
        select_depth = None
        depth = 0
        item_start = True
        for i, t in enumerate(toks):
            prev = toks[i - 1] if i else None
            nxt = toks[i + 1] if i + 1 < len(toks) else None
            if t.text == '(':
                depth += 1
            elif t.text == ')':
                depth -= 1
            if t.low == 'select' and select_depth is None:
                select_depth = depth
            if t.kind in ('bind', 'subst'):
                name = (t.text[1:] if t.kind == 'bind' else t.text[1:-1]).upper()
                res.append('@%s::%s' % (name, types.get(name, 'text')))
                continue
            if t.kind != 'id':
                res.append(t.text)
                continue
            low = t.low
            if prev is not None and prev.text == '::' or nxt is not None and nxt.text == '=>':
                res.append(low)                                    # a type name or a named argument
                continue
            if nxt is not None and nxt.text == '(':
                res.append(low)                                    # a function
                continue
            if low in KEYWORDS or low in ('localtimestamp', 'now()', 'current_date', 'coalesce', 'concat', 'not'):
                # a new line before each clause, so the generated query reads like the original
                brk = low in ('from', 'where', 'group', 'order', 'having', 'union') or (
                    low in ('join', 'left', 'inner', 'full', 'cross') and (prev is None or prev.low not in ('left', 'inner', 'full', 'cross', 'outer')))
                res.append(('\n' if brk else '') + low)
                continue
            if prev is not None and prev.text == '.':
                # alias.COLUMN
                alias = toks[i - 2].text.upper()
                col = t.text.upper()
                c = self.column(aliases.get(alias, []), col)
                if c and c[1] == 'flag' and self.view_body and self.bare_select_item(toks, i - 2, i):
                    res[-2:] = []                                   # drop "alias ."
                    res.append("case when %s.\"%s\" then 'Y' when not %s.\"%s\" then 'N' end AS \"%s\"" % (alias.lower(), c[0], alias.lower(), c[0], col))
                    continue
                res.append('"%s"' % (c[0] if c else col))
                if c and c[0] != col and self.bare_select_item(toks, i - 2, i):
                    res.append('AS "%s"' % col)
                continue
            if nxt is not None and nxt.text == '.':
                res.append(t.text.lower() if t.text.upper() in aliases and t.text.upper() not in self.tables else '"%s"' % self.table_name(t.text))
                continue
            name = t.text.upper()
            if name in self.ctes and (nxt is not None and nxt.low == 'as' or prev is not None and (prev.low in ('from', 'join') or prev.text == ',')):
                res.append(t.text.lower())                          # a WITH query
                continue
            if prev is not None and (prev.low in ('from', 'join') or (prev.text == ',' and self.in_from(toks, i))):
                res.append('"%s"' % self.table_name(name))
                continue
            if i >= 2 and prev.kind == 'id' and prev.low not in KEYWORDS and (
                    toks[i - 2].low in ('from', 'join') or (toks[i - 2].text == ',' and self.in_from(toks, i - 1))):
                res.append(t.text.lower())                          # a table alias
                continue
            if prev is not None and (prev.kind in ('str', 'num', 'qid') or prev.text == ')' or (prev.kind == 'id' and prev.low not in KEYWORDS)) or (prev is not None and prev.low == 'as'):
                res.append('"%s"' % name)                           # an output alias
                continue
            c = self.column(all_tables, name)
            res.append('"%s"' % (c[0] if c else name))
            if c and c[0] != name and self.bare_select_item(toks, i, i):
                res.append('AS "%s"' % name)
        text = ' '.join(res)
        text = text.replace(' :: ', '::')
        text = re.sub(r' ?\n', '\n', text)
        text = re.sub(r'\(\s*\n', '(', text)
        text = re.sub(r'\s+([,)])', r'\1', text)
        text = re.sub(r'\(\s+', '(', text)
        text = re.sub(r'\s*\.\s*', '.', text)
        text = re.sub(r'(\w|\))\s+\(', lambda m: m.group(1) + ('(' if m.group(1) not in ('in', 'as', 'on', 'and', 'or', 'not', 'exists', 'then', 'else', 'when') else ' ('), text)
        text = re.sub(r'\b(in|as|on|and|or|not|exists|then|else|when|over|from|join|select|where)\(', r'\1 (', text)
        text = text.replace('@ ', '@')
        return text

    PG_TYPE = {'string': 'text', 'text': 'text', 'int': 'bigint', 'decimal': 'numeric', 'date': 'date',
               'datetime': 'timestamptz', 'flag': 'boolean', 'audit': 'text'}

    def bind_types(self, toks, aliases, all_tables):
        """The PostgreSQL type of each bind, from the column it is compared with (else text): every use of the
        parameter is cast to it, as PostgreSQL cannot type a parameter used only in "is null"."""
        types = {}

        def kind_at(i):
            """Kind of the column reference ending at token i (alias.COL or COL), or of a numeric function call."""
            if i >= 0 and toks[i].text == ')':
                depth = 0
                for j in range(i, -1, -1):
                    depth += toks[j].text == ')'
                    depth -= toks[j].text == '('
                    if depth == 0:
                        fn = toks[j - 1].low if j > 0 else ''
                        return 'decimal' if fn in ('sum', 'count', 'avg', 'round', 'abs', 'greatest', 'least', 'trunc', 'floor', 'ceil', 'nvl', 'coalesce') else None
                return None
            if i < 0 or toks[i].kind != 'id':
                return None
            if i >= 2 and toks[i - 1].text == '.':
                c = self.column(aliases.get(toks[i - 2].text.upper(), []), toks[i].text.upper())
            else:
                c = self.column(all_tables, toks[i].text.upper())
            if c:
                return c[1]
            # a column of a WITH query or view: its type from its name
            name = toks[i].text.upper()
            if re.search(r'(_PCT|_AMOUNT|_VALUE|_COUNT|_DAYS|_RATE|_ID|_QTY|_MONTHS|_RATIO|_TOTAL|_BALANCE)$', name):
                return 'decimal'
            if name.endswith('_DATE'):
                return 'date'
            return None

        def kind_from(i):
            """Kind of the column reference starting at token i."""
            if i >= len(toks) or toks[i].kind != 'id':
                return None
            if i + 2 < len(toks) and toks[i + 1].text == '.':
                return kind_at(i + 2)
            return kind_at(i)

        for i, t in enumerate(toks):
            if t.kind not in ('bind', 'subst'):
                continue
            name = (t.text[1:] if t.kind == 'bind' else t.text[1:-1]).upper()
            kind = None
            if i >= 2 and toks[i - 1].text in ('=', '<>', '!=', '<', '>', '<=', '>=', 'between', 'and') or (i >= 2 and toks[i - 1].low in ('between', 'and')):
                kind = kind_at(i - 2)
            if kind is None and i + 2 < len(toks) and toks[i + 1].text in ('=', '<>', '!=', '<', '>', '<=', '>='):
                kind = kind_from(i + 2)
            if kind is None and i >= 3 and toks[i - 1].low == 'and' and toks[i - 3].low == 'between':
                kind = kind_at(i - 4)
            if kind and kind != 'flag' and name not in types:
                types[name] = self.PG_TYPE.get(kind, 'text')
        return types

    def table_name(self, oracle):
        if oracle in self.tables:
            return self.tables[oracle]['table']
        return oracle

    @staticmethod
    def bare_select_item(toks, start, end):
        """True when toks[start..end] is a whole select-list item with no alias of its own."""
        before = toks[start - 1] if start > 0 else None
        after = toks[end + 1] if end + 1 < len(toks) else None
        if before is None or not (before.low in ('select', 'distinct') or before.text == ','):
            return False
        if after is not None and not (after.text == ',' or after.low == 'from'):
            return False
        # must be in a select list: walk back to the clause keyword at this depth
        depth = 0
        for j in range(start - 1, -1, -1):
            t = toks[j]
            if t.text == ')':
                depth += 1
            elif t.text == '(':
                if depth == 0:
                    return False
                depth -= 1
            elif depth == 0 and t.kind == 'id' and t.low in ('select', 'from', 'where', 'group', 'order', 'having', 'by', 'on'):
                return t.low == 'select'
        return False

    def numeric_division(self, toks):
        """a / b -> a / (b)::numeric: Oracle divides exactly, PostgreSQL truncates integer division."""
        out = []
        i = 0
        while i < len(toks):
            out.append(toks[i])
            if toks[i].text == '/' and i + 1 < len(toks):
                end = self.primary_end(toks, i + 1)
                operand = self.numeric_division(toks[i + 1:end])
                out += [Tok('op', '(')] + operand + [Tok('op', ')'), Tok('op', '::'), Tok('id', 'numeric')]
                i = end
                continue
            i += 1
        return out

    def concat_chains(self, toks):
        """a || b || c -> concat(a, b, c): Oracle treats null as '' in ||, PostgreSQL makes the result null."""
        out = []
        i = 0
        while i < len(toks):
            end = self.primary_end(toks, i) if toks[i].kind != 'op' or toks[i].text in ('(', '-') else i + 1
            if end < len(toks) and toks[end].text == '||' and (toks[i].kind != 'op' or toks[i].text == '('):
                parts = [self.concat_chains(toks[i:end])]
                j = end
                while j < len(toks) and toks[j].text == '||':
                    e = self.primary_end(toks, j + 1)
                    parts.append(self.concat_chains(toks[j + 1:e]))
                    j = e
                out += [Tok('id', 'concat'), Tok('op', '(')]
                for k, p in enumerate(parts):
                    if k:
                        out.append(Tok('op', ','))
                    out += p
                out.append(Tok('op', ')'))
                i = j
                continue
            if toks[i].text == '(' and end > i + 1:
                out.append(toks[i])
                out += self.concat_chains(toks[i + 1:end - 1])
                out.append(toks[end - 1])
                i = end
                continue
            out.append(toks[i])
            i += 1
        return out

    def with_views(self, sql, views):
        """Adds the views the query uses (and the views they use) as WITH queries."""
        order, seen = [], set()

        def visit(name):
            if name in seen:
                return
            seen.add(name)
            body = self.view_sql(name)
            if body is None:
                self.warnings.append(f'view {name} has no PostgreSQL version in the views folder')
                return
            for dep in sorted(set(re.findall(r'"(FNV_V_[A-Z0-9_]+)"', body))):
                visit(dep)
            order.append((name, body))

        for v in views:
            visit(v)
        ctes = ',\n'.join(f'"{n}" AS (\n{b}\n)' for n, b in order)
        if re.match(r'\s*with\b', sql, re.I):
            return re.sub(r'^\s*with\b', 'with ' + ctes.replace('\\', '\\\\') + ',\n', sql, count=1, flags=re.I)
        return f'with {ctes}\n{sql}'


if __name__ == '__main__':
    tr = Translator(['map-mtg.json', 'map-adm.json'], os.path.join(HERE, 'sql-105', 'views'))
    print(tr.translate(sys.argv[1]))
    for w in tr.warnings:
        print('--', w)
