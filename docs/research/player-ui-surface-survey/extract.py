"""Extract the player-UI surface each published game touches.

Usage: python3 -I extract.py <corpus_dir> <out.jsonl>

One JSON line per game: engine version, UI option flags on <game>, <javascript>
references, and every UI-facing occurrence (JS.* call, request(), selectors and
jQuery UI methods inside JS strings), tagged with the top-level element it sits
in so the aggregator can tell stock library code from author code.
"""
import json
import os
import re
import sys
import xml.etree.ElementTree as ET
from multiprocessing import Pool

JS_CALL = re.compile(r'\bJS\.([A-Za-z_]\w*)\s*\(')
REQUEST = re.compile(r'\brequest\s*\(\s*([A-Za-z]+)\s*,\s*("([^"]*)"|[^)\n]*)\)')
STRING = re.compile(r'"((?:[^"\\]|\\.)*)"|\'((?:[^\'\\]|\\.)*)\'')
ID_SEL = re.compile(r'#([A-Za-z_][\w-]*)')
CLASS_SEL = re.compile(r'(?:^|[\s,>(\'"])\.([A-Za-z_][\w-]*)')
GET_BY_ID = re.compile(r'getElementById\s*\(\s*\\?[\'"]([\w-]+)')
CALL_IN_STR = re.compile(r'(?<![\w.$])([A-Za-z_]\w*)\s*\(')
JQUI = re.compile(r'\.(button|buttonset|dialog|accordion|multiOpenAccordion|selectable|draggable|droppable|resizable|sortable|tabs|slider|tooltip|autocomplete|menu|progressbar|spinner|datepicker|jjmenu|effect|toggle|hide|show|insertBefore|insertAfter|appendTo|prependTo|after|before|append|prepend|remove|css|html|attr|addClass|removeClass)\s*\(')
SAVED_BY = re.compile(r'Saved by Quest ([0-9.]+)')
ASL_VER = re.compile(r'<asl version="(\d+)"')
HEX = re.compile(r'^[0-9a-fA-F]{3}([0-9a-fA-F]{1}|[0-9a-fA-F]{3}|[0-9a-fA-F]{5})?$')


def call_text(text, start):
    """The call from `start` to its matching close paren (or end of line)."""
    depth = 0
    i = start
    in_str = None
    while i < len(text) and i - start < 2000:
        c = text[i]
        if in_str:
            if c == '\\':
                i += 1
            elif c == in_str:
                in_str = None
        elif c in '"\'':
            in_str = c
        elif c == '(':
            depth += 1
        elif c == ')':
            depth -= 1
            if depth == 0:
                return text[start:i + 1]
        i += 1
    return text[start:i]


def strings_in(s):
    return [a if a else b for a, b in STRING.findall(s)]


def scan(text, where, occ):
    tag, name = where
    for m in JS_CALL.finditer(text):
        call = call_text(text, m.start())
        norm = re.sub(r'\s+', '', call)
        occ.append([tag, name, 'js', m.group(1), norm])
        for s in strings_in(call[m.end() - m.start():]):
            for i in ID_SEL.findall(s):
                if not HEX.match(i):
                    occ.append([tag, name, 'id', i, norm])
            for i in GET_BY_ID.findall(s):
                occ.append([tag, name, 'id', i, norm])
            for c in CLASS_SEL.findall(s):
                occ.append([tag, name, 'class', c, norm])
            for f in CALL_IN_STR.findall(s):
                occ.append([tag, name, 'fn', f, norm])
            for j in JQUI.findall(s):
                occ.append([tag, name, 'jq', j, norm])
    for m in REQUEST.finditer(text):
        arg = m.group(3) if m.group(3) is not None else m.group(2).strip()
        norm = re.sub(r'\s+', '', m.group(0))
        key = m.group(1) + ('(' + arg + ')' if m.group(1) in ('Show', 'Hide', 'SetInterfaceString') else '')
        occ.append([tag, name, 'request', key, norm])
    if '<script' in text.lower():
        occ.append([tag, name, 'inline', '<script>', ''])
    if '<style' in text.lower():
        occ.append([tag, name, 'inline', '<style>', ''])


def truthy(el):
    if el.get('type') == 'boolean' or not el.get('type'):
        t = (el.text or '').strip().lower()
        if t in ('', 'true'):
            return True if len(el) == 0 else None
        if t == 'false':
            return False
    return None


def process(path):
    gid = os.path.basename(path)[:-5]
    with open(path, 'rb') as f:
        raw = f.read()
    text = raw.decode('utf-8-sig', errors='replace')
    rec = {
        'id': gid,
        'saved': (SAVED_BY.search(text[:300]) or [None, None])[1],
        'asl': (ASL_VER.search(text[:500]) or [None, None])[1],
        'parse': True,
        'flags': {},
        'jsfiles': [],
        'occ': [],
    }
    try:
        root = ET.fromstring(text)
    except ET.ParseError:
        rec['parse'] = False
        scan(text, ('?', '?'), rec['occ'])
        return rec
    for child in root:
        tag = child.tag
        name = child.get('name') or ''
        if tag == 'game':
            rec['name'] = name
            gid_el = child.find('gameid')
            rec['gameid'] = (gid_el.text or '').strip() if gid_el is not None else None
            for g in child:
                v = truthy(g)
                if v is not None:
                    rec['flags'][g.tag] = v
                elif g.tag in ('customwidth', 'custompaddingtop', 'menufontsize', 'commandcursor', 'compassdirections'):
                    rec['flags'][g.tag] = (g.text or '').strip()[:60]
        if tag == 'javascript':
            rec['jsfiles'].append(child.get('src') or '')
        scan('\n'.join(child.itertext()), (tag, name), rec['occ'])
    return rec


def main():
    corpus, out = sys.argv[1], sys.argv[2]
    files = sorted(os.path.join(corpus, f) for f in os.listdir(corpus) if f.endswith('.aslx'))
    with Pool() as pool, open(out, 'w') as o:
        for n, rec in enumerate(pool.imap_unordered(process, files, chunksize=20), 1):
            o.write(json.dumps(rec) + '\n')
            if n % 2000 == 0:
                print(n, file=sys.stderr, flush=True)


if __name__ == '__main__':
    main()
