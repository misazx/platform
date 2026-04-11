import urllib.request, re

h = urllib.request.urlopen('http://localhost:18765/').read().decode()
i2 = h.find('<script>', h.find('<script>')+1)
e = h.find('</script>', i2)
js = h[i2+8:e]

# Find all lines that contain backtick (template literal) 
# and check for issues
lines = js.split('\n')
for i, line in enumerate(lines):
    if '`' in line:
        # Check for unbalanced backticks
        bt_count = line.count('`')
        if bt_count % 2 != 0:
            print(f'LINE {i+1}: UNBALANCED backticks ({bt_count}): {line[:120]}')
        # Check for ${} inside
        if '${' in line:
            # Make sure it's properly closed
            depth = 0
            in_expr = False
            for ch in line:
                if ch == '$' and not in_expr: continue
                if ch == '{' and not in_expr: in_expr = True; depth += 1
                elif ch == '{' and in_expr: depth += 1
                elif ch == '}' and in_expr: depth -= 1; if depth == 0: in_expr = False
            if in_expr or depth != 0:
                print(f'LINE {i+1}: UNCLOSED ${{}}: {line[:120]}')

print(f'Total lines: {len(lines)}, checked {sum(1 for l in lines if "`" in l)} with backticks')
