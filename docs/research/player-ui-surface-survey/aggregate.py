"""Aggregate extract.py output into the survey tables.

Usage: python3 -I aggregate.py <all.jsonl> <quest_repo> <quest5_repo> <out.json>
"""
import collections
import glob
import json
import os
import re
import functools
import sys
import xml.etree.ElementTree as ET

src, quest, quest5, out = sys.argv[1:5]

# --- reference sets ---------------------------------------------------------
core_files = glob.glob(os.path.join(quest, 'src/Engine/Core/**/*.aslx'), recursive=True) \
    + glob.glob(os.path.join(quest5, 'WorldModel/WorldModel/Core/**/*.aslx'), recursive=True)
core_names = set()
core_blob = []
for f in core_files:
    t = open(f, encoding='utf-8-sig', errors='replace').read()
    core_blob.append(re.sub(r'\s+', '', t))
    try:
        for c in ET.fromstring(t):
            core_names.add((c.tag, c.get('name') or ''))
    except ET.ParseError:
        pass
core_blob = '\n'.join(core_blob)
# XML-escaped forms appear in the library source; the games' itertext is unescaped.
core_blob = core_blob.replace('&lt;', '<').replace('&gt;', '>').replace('&amp;', '&').replace('&quot;', '"')

ui = {
    'current': [os.path.join(quest, 'src/WasmPlayer/player-ui', f) for f in ('playercore.htm', 'playercore.js', 'player.js', 'grid.js', 'playercore.css')]
               + [os.path.join(quest, 'src/WasmPlayer/wasm-player.js')],
    'q5': [os.path.join(quest5, 'PlayerController', f) for f in ('playercore.htm', 'playercore.js', 'grid.js', 'playercore.css')]
          + [os.path.join(quest5, 'Player/desktopplayer.js')],
}
ids, fns, classes = {}, {}, {}
for k, files in ui.items():
    t = '\n'.join(open(f, encoding='utf-8', errors='replace').read() for f in files)
    ids[k] = set(re.findall(r'id\s*=\s*\\?["\']([\w-]+)', t)) | set(re.findall(r'#([A-Za-z_][\w-]*)', t)) \
        | set(re.findall(r'getElementById\(\s*["\']([\w-]+)', t))
    fns[k] = set(re.findall(r'function\s+([A-Za-z_]\w*)\s*\(', t)) | set(re.findall(r'window\.([A-Za-z_]\w*)\s*=', t)) \
        | set(re.findall(r'^\s+([A-Za-z_]\w*)\s*(?::\s*(?:async\s*)?(?:function|\()|\(.*\)\s*\{)', t, re.M))
    classes[k] = set(re.findall(r'\.([A-Za-z_][\w-]*)', t)) | {c for cl in re.findall(r'class\s*=\s*\\?["\']([^"\']+)', t) for c in cl.split()}

@functools.lru_cache(maxsize=None)
def exists(kind, key):
    table = {'id': ids, 'fn': fns, 'js': fns, 'class': classes}.get(kind)
    if table is None:
        return 'n/a'
    if key in table['current']:
        return 'current'
    if key in table['q5']:
        return 'q5-only'
    return 'unknown'

# --- pass 1: how many games carry each (element, call) ----------------------
freq = collections.Counter()
for line in open(src):
    r = json.loads(line)
    seen = {(o[0], o[1], o[4]) for o in r['occ'] if o[2] in ('js', 'request')}
    freq.update(seen)

@functools.lru_cache(maxsize=None)
def in_core_text(norm):
    return bool(norm) and norm in core_blob


@functools.lru_cache(maxsize=None)
def classify(tag, name, norm):
    in_name = (tag, name) in core_names
    if norm == '':
        # inline <script>/<style>: no call text to compare, so go by the element
        return 'core' if in_name else 'author'
    if in_core_text(norm):
        return 'core'
    common = freq[(tag, name, norm)] >= 20
    if in_name:
        # Old Core versions baked each language library's interface strings into
        # InitInterface as literals, so rarer translations fall under the
        # frequency bar while still being stock.
        if name == 'InitInterface' and ('setInterfaceString' in norm or 'SetInterfaceString' in norm):
            return 'core'
        return 'core' if freq[(tag, name, norm)] >= 5 else 'override'
    return 'library' if common else 'author'

# --- pass 2 -----------------------------------------------------------------
N = 0
parse_fail = 0
saved = collections.Counter()
by_key = collections.defaultdict(lambda: collections.Counter())  # (kind,key) -> class -> games
games_with = collections.Counter()
games_with_by_ver = collections.defaultdict(collections.Counter)
jsfiles = collections.Counter()
flags = collections.Counter()
flag_values = collections.defaultdict(collections.Counter)
author_eval = collections.Counter()
author_js_unknown = collections.Counter()
author_jq = collections.Counter()
author_fn = collections.Counter()
override_names = collections.Counter()
games_any_author_dom = 0

for line in open(src):
    r = json.loads(line)
    N += 1
    if not r['parse']:
        parse_fail += 1
    ver = '.'.join((r['saved'] or 'unknown').split('.')[:2])
    saved[ver] += 1
    if r['jsfiles']:
        games_with['javascript file'] += 1
        games_with_by_ver[ver]['javascript file'] += 1
        for j in set(r['jsfiles']):
            jsfiles[j] += 1
    for k, v in r['flags'].items():
        if v is True:
            flags[k] += 1
        elif isinstance(v, str):
            flag_values[k][v] += 1

    per_game = set()
    cats = set()
    for tag, name, kind, key, norm in r['occ']:
        cls = classify(tag, name, norm)
        if kind in ('id', 'class') and exists(kind, key) == 'unknown':
            cls_key = (kind + ':author-own', key)
        elif kind == 'fn':
            if exists('fn', key) == 'unknown':
                continue
            cls_key = (kind, key)
        else:
            cls_key = (kind, key)
        per_game.add((cls_key, cls))
        if cls in ('author', 'override'):
            if kind == 'js':
                cats.add('author JS.* call')
                if key == 'eval':
                    author_eval[norm[:160]] += 1
                    cats.add('author JS.eval')
                if exists('js', key) == 'unknown':
                    author_js_unknown[key] += 1
            elif kind == 'request':
                cats.add('author request()')
            elif kind == 'id' and exists('id', key) != 'unknown':
                cats.add('author touches a player-UI id')
            elif kind == 'class' and exists('class', key) != 'unknown':
                cats.add('author touches a player-UI class')
            elif kind == 'jq':
                cats.add('author jQuery method')
            elif kind == 'fn':
                cats.add('author calls a player JS global from eval')
            elif kind == 'inline':
                cats.add('author inline ' + key)
        if cls == 'override':
            override_names[(tag, name)] += 1
    for (ck, cls) in per_game:
        by_key[ck][cls] += 1
    # dedupe per game for the "author" lists
    for c in cats:
        games_with[c] += 1
        games_with_by_ver[ver][c] += 1
    if cats & {'author touches a player-UI id', 'author touches a player-UI class', 'author calls a player JS global from eval', 'author jQuery method'}:
        games_any_author_dom += 1
        games_with_by_ver[ver]['any author DOM/jQuery'] += 1

for k in ('author_eval', 'author_jq', 'author_fn'):
    pass

# per-game author eval/jq/fn counts were per occurrence; recount per game for jq/fn
author_jq_games = collections.Counter()
author_fn_games = collections.Counter()
author_eval_games = collections.Counter()
for line in open(src):
    r = json.loads(line)
    jq, fn, ev = set(), set(), set()
    for tag, name, kind, key, norm in r['occ']:
        if classify(tag, name, norm) not in ('author', 'override'):
            continue
        if kind == 'jq':
            jq.add(key)
        elif kind == 'fn' and exists('fn', key) != 'unknown':
            fn.add(key)
        elif kind == 'js' and key == 'eval':
            ev.add(norm[:160])
    author_jq_games.update(jq)
    author_fn_games.update(fn)
    author_eval_games.update(ev)

def table(kind, top=80):
    rows = []
    for (k, key), cnt in by_key.items():
        if k != kind:
            continue
        total = sum(cnt.values())
        rows.append({'key': key, 'exists': exists(kind.split(':')[0], key) if ':' not in kind else 'author-own',
                     'core': cnt['core'], 'library': cnt['library'], 'override': cnt['override'], 'author': cnt['author'], 'rows': total})
    rows.sort(key=lambda r: -(r['author'] + r['override']) * 1000 - r['core'])
    return rows[:top]

result = {
    'games': N,
    'parse_fail': parse_fail,
    'saved': saved.most_common(),
    'games_with': games_with.most_common(),
    'games_any_author_dom': games_any_author_dom,
    'by_version': {v: dict(c) for v, c in games_with_by_ver.items()},
    'ids': table('id', 200),
    'author_own_ids': table('id:author-own', 30),
    'classes': table('class', 60),
    'js': table('js', 200),
    'requests': table('request', 80),
    'fns': table('fn', 80),
    'jq': table('jq', 40),
    'inline': table('inline', 5),
    'jsfiles': jsfiles.most_common(40),
    'jsfile_games': games_with['javascript file'],
    'flags': flags.most_common(60),
    'flag_values': {k: v.most_common(8) for k, v in flag_values.items()},
    'author_eval_top': author_eval_games.most_common(60),
    'author_eval_distinct': len(author_eval_games),
    'author_jq_games': author_jq_games.most_common(30),
    'author_fn_games': author_fn_games.most_common(40),
    'author_js_unknown': author_js_unknown.most_common(40),
    'override_names': [[t, n, c] for (t, n), c in override_names.most_common(40)],
    'core_names': len(core_names),
}
json.dump(result, open(out, 'w'), indent=1)
print('games', N, 'parse_fail', parse_fail, 'any author DOM', games_any_author_dom)
for k, v in games_with.most_common():
    print(f'{v:6d}  {v / N:6.1%}  {k}')
