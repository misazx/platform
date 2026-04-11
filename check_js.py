import re, sys
sys.path.insert(0, '.')
from modules.reasoning_viz import _generate_html

html = _generate_html()
idx1 = html.find('<script>')
idx2 = html.find('<script>', idx1 + 1)
end = html.find('</script>', idx2)
js = html[idx2 + len('<script>'):end]

open('viz_debug.js', 'w').write(js)
print(f'JS extracted: {len(js)} chars')

if "focusNode('${esc(n)}')" in js:
    print('OK: Template literal fix applied')
else:
    print('WARN: fix not found')

if "\\'" in js or "\\''" in js:
    print('WARN: escaped quote issue remains')
else:
    print('OK: No escaped quote issues')
