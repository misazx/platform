import json

with open('Client/GameModes/light_shadow_traveler/Config/Data/levels.json') as f:
    data = json.load(f)

LIGHT_SPEED = 280
LIGHT_JUMP = 550
SHADOW_SPEED = 350
SHADOW_JUMP = 620
GRAVITY = 980
DASH_EXTRA = 90

LIGHT_MAX_H = LIGHT_JUMP**2 / (2*GRAVITY)
LIGHT_AIR_TIME = 2*LIGHT_JUMP/GRAVITY
LIGHT_MAX_DIST = LIGHT_SPEED * LIGHT_AIR_TIME + DASH_EXTRA

SHADOW_MAX_H = SHADOW_JUMP**2 / (2*GRAVITY)
SHADOW_AIR_TIME = 2*SHADOW_JUMP/GRAVITY
SHADOW_MAX_DIST = SHADOW_SPEED * SHADOW_AIR_TIME

print(f'=== Player Physics ===')
print(f'Light: max_h={LIGHT_MAX_H:.0f}px, max_dist={LIGHT_MAX_DIST:.0f}px')
print(f'Shadow: max_h={SHADOW_MAX_H:.0f}px, max_dist={SHADOW_MAX_DIST:.0f}px')
print()

issues = []

for chapter in data['chapters']:
    for level in chapter['levels']:
        lid = level['id']
        platforms = level['platforms']
        start = level.get('startPos', {})
        
        if platforms and start:
            p = platforms[0]
            sx = start.get('x', 0)
            sy = start.get('y', 0)
            if not (p['x'] <= sx <= p['x'] + p['w'] and p['y'] - 80 <= sy <= p['y']):
                issues.append(f'{lid}: startPos({sx},{sy}) NOT on first platform({p["x"]},{p["y"]},{p["w"]})')
        
        for i in range(len(platforms) - 1):
            p1 = platforms[i]
            p2 = platforms[i + 1]
            
            p1_right = p1['x'] + p1['w']
            p2_left = p2['x']
            h_dist = p2_left - p1_right
            
            p1_top = p1['y']
            p2_top = p2['y']
            v_diff = p1_top - p2_top
            
            ptype = p2.get('type', 'normal')
            
            if ptype == 'shadow_wall':
                continue
            
            if ptype in ['light', 'normal']:
                max_h = LIGHT_MAX_H
                max_d = LIGHT_MAX_DIST
            else:
                max_h = SHADOW_MAX_H
                max_d = SHADOW_MAX_DIST
            
            if v_diff > max_h:
                issues.append(f'{lid}: P{i}->P{i+1} TOO HIGH! v_diff={v_diff:.0f}px > max_h={max_h:.0f}px (type={ptype})')
            elif h_dist > max_d:
                issues.append(f'{lid}: P{i}->P{i+1} TOO FAR! h_dist={h_dist:.0f}px > max_d={max_d:.0f}px (type={ptype})')
            elif h_dist > max_d * 0.85 and v_diff > max_h * 0.5:
                issues.append(f'{lid}: P{i}->P{i+1} VERY HARD! h={h_dist:.0f}px v={v_diff:.0f}px (type={ptype})')

print(f'=== Issues Found: {len(issues)} ===')
for issue in issues:
    print(issue)
