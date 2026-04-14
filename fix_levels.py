import json

with open('Client/GameModes/light_shadow_traveler/Config/Data/levels.json') as f:
    data = json.load(f)

fixed_count = 0
for chapter in data['chapters']:
    for level in chapter['levels']:
        platforms = level.get('platforms', [])
        start = level.get('startPos', {})
        if platforms and start:
            p = platforms[0]
            sx = start.get('x', 0)
            sy = start.get('y', 0)
            new_x = sx
            if not (p['x'] <= sx <= p['x'] + p['w']):
                new_x = p['x'] + min(80, p['w'] // 2)
            new_y = p['y'] - 40
            if new_x != sx or new_y != sy:
                level['startPos']['x'] = new_x
                level['startPos']['y'] = new_y
                fixed_count += 1
                print(f'Fixed {level["id"]}: startPos ({sx},{sy}) -> ({new_x},{new_y})')

        if level['id'] == 'sl_04':
            for p in level['platforms']:
                if p['x'] == 500 and p['y'] == 300 and p['type'] == 'shadow':
                    p['y'] = 310
                    print(f'Fixed sl_04: shadow platform y 300 -> 310')

with open('Client/GameModes/light_shadow_traveler/Config/Data/levels.json', 'w') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)

print(f'\nTotal startPos fixes: {fixed_count}')
