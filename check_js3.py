import re

h = open('.trany/mcps/tranycode-core/reasoning_viz.html').read()
i2 = h.find('<script>', h.find('<script>')+1)
e = h.find('</script>', i2)
js = h[i2+8:e]

open('viz_check.js', 'w').write(js)

stack = []
in_str = None
escaped = False
for i, ch in enumerate(js):
    if escaped: escaped = False; continue
    if ch == '\\': escaped = True; continue
    if ch in '"\'':
        if in_str is None: in_str = ch
        elif in_str == ch: in_str = None
        continue
    if in_str: continue
    
    if ch in '({[':
        stack.append((ch, i))
    elif ch in ')}]':
        if stack:
            oc, oi = stack.pop()
            exp = {'(': ')', '[': ']', '{': '}'}
            if exp.get(oc) != ch:
                ln = js[:i].count('\n') + 1
                print(f'MISMATCH line {ln}: got {ch} expected {exp.get(oc)} (opened line {js[:oi].count(chr(10))+1})')
        else:
            ln = js[:i].count('\n') + 1
            print(f'UNEXPECTED {ch} at line {ln}')

if stack:
    for ch, i in stack:
        print(f'UNCLOSED {ch} at line {js[:i].count(chr(10))+1}')
else:
    print('All brackets balanced - JS syntax OK')
