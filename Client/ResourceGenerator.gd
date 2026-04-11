extends Node

var rng = RandomNumberGenerator.new()

func _ready():
    rng.randomize()
    # ⚠️ 已禁用 - 使用 Kenney 风格预生成资源替代程序化生成
    # generate_all_resources()
    print("[ResourceGenerator] ✅ 使用外部 Kenney 风格资源，跳过程序化生成")

func generate_all_resources():
    print("========================================")
    print("   图像资源生成器 - 杀戮尖塔2")
    print("========================================")
    
    generate_character_portraits()
    generate_backgrounds()
    generate_ui_icons()
    generate_enemy_images()
    generate_relic_images()
    generate_potion_images()
    generate_event_images()
    generate_card_icons()
    
    print("========================================")
    print("   所有图像资源生成完成！")
    print("========================================")

func generate_character_portraits():
    print("--- 生成角色肖像 ---")
    
    var characters = {
        "Ironclad": Color("#FF4444"),
        "Silent": Color("#44FF44"),
        "Defect": Color("#4444FF"),
        "Watcher": Color("#AA44AA"),
        "Necromancer": Color("#444444"),
        "Heir": Color("#FFAA44")
    }
    
    for char_name in characters:
        var img = create_character_portrait(characters[char_name])
        save_image(img, "res://Images/Characters/%s.png" % char_name)

func create_character_portrait(color: Color) -> Image:
    var img = Image.create(200, 280, false, Image.FORMAT_RGBA8)
    img.fill(color.darkened(0.6))
    
    # 绘制背景纹理
    for y in range(280):
        for x in range(200):
            var noise = sin(x * 0.1) * cos(y * 0.1) * 0.1
            var c = color.darkened(0.5 + noise)
            img.set_pixel(x, y, c)
    
    # 绘制角色轮廓
    var dark = color.darkened(0.4)
    var light = color.lightened(0.3)
    
    # 头部
    draw_circle_filled(img, 100, 120, 30, dark)
    # 身体
    draw_ellipse_filled(img, 100, 160, 25, 40, dark)
    # 腿部
    draw_rect_filled(img, 75, 195, 20, 35, dark)
    draw_rect_filled(img, 105, 195, 20, 35, dark)
    # 手臂
    draw_line_thick(img, 85, 170, 60, 210, light, 5)
    draw_line_thick(img, 115, 170, 140, 210, light, 5)
    
    # 边框
    draw_frame(img, 200, 280, color.lightened(0.4))
    
    return img

func generate_backgrounds():
    print("--- 生成背景图像 ---")
    
    var backgrounds = {
        "glory": Color(0.6, 0.5, 0.3),
        "hive": Color(0.5, 0.4, 0.2),
        "overgrowth": Color(0.3, 0.5, 0.3),
        "underdocks": Color(0.2, 0.3, 0.4)
    }
    
    for bg_name in backgrounds:
        var img = create_background(backgrounds[bg_name])
        save_image(img, "res://Images/Backgrounds/%s.png" % bg_name)

func create_background(base_color: Color) -> Image:
    var img = Image.create(1280, 720, false, Image.FORMAT_RGBA8)
    
    for y in range(720):
        for x in range(1280):
            var noise = sin(x * 0.02 + y * 0.015) * 0.05 + \
                       sin(x * 0.035 - y * 0.025) * 0.03 + \
                       sin(y * 0.03) * 0.02
            var c = base_color.darkened(y / 720.0 * 0.3)
            c = c.lightened(noise)
            img.set_pixel(x, y, c)
    
    # 添加地面
    var floor_y = int(720 * 0.75)
    var floor_color = base_color.darkened(0.3)
    floor_color.a = 0.5
    
    for y in range(floor_y, 720):
        for x in range(1280):
            var existing = img.get_pixel(x, y)
            var blend = float(y - floor_y) / (720 - floor_y)
            img.set_pixel(x, y, existing.lerp(floor_color, blend * 0.5))
    
    return img

func generate_ui_icons():
    print("--- 生成UI图标 ---")
    generate_rest_icons()
    generate_achievement_icons()
    generate_skill_icons()
    generate_item_icons()
    generate_service_icons()

func generate_rest_icons():
    var icons = {
        "heal": Color(0.3, 1.0, 0.4),
        "upgrade": Color(1.0, 0.85, 0.3),
        "recall": Color(0.5, 0.5, 1.0),
        "smith": Color(0.8, 0.5, 0.3),
        "default": Color(0.5, 0.5, 0.5)
    }
    
    for icon_name in icons:
        var img = create_rest_icon(icon_name, icons[icon_name])
        save_image(img, "res://Icons/Rest/%s.png" % icon_name)

func create_rest_icon(type: String, color: Color) -> Image:
    var img = Image.create(48, 48, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    match type:
        "heal":
            draw_heart_icon(img, color)
        "upgrade":
            draw_arrow_up_icon(img, color)
        "recall":
            draw_recall_icon(img, color)
        "smith":
            draw_hammer_icon(img, color)
        _:
            draw_circle_filled(img, 24, 24, 16, color)
    
    return img

func generate_achievement_icons():
    var icons = {
        "FirstVictory": Color(1.0, 0.85, 0.3),
        "Kill100": Color(0.8, 0.2, 0.2),
        "AllRelics": Color(0.6, 0.3, 0.8),
        "NoDamage": Color(0.3, 0.8, 0.3)
    }
    
    for icon_name in icons:
        var img = create_achievement_icon(icon_name, icons[icon_name])
        save_image(img, "res://Icons/Achievements/%s.png" % icon_name)

func create_achievement_icon(type: String, color: Color) -> Image:
    var img = Image.create(64, 64, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    match type:
        "FirstVictory":
            draw_trophy_icon(img, color)
        "Kill100":
            draw_skull_icon(img, color)
        "AllRelics":
            draw_diamond_shape(img, 32, 32, 20, color)
        "NoDamage":
            draw_shield_icon(img, color)
        _:
            draw_circle_filled(img, 32, 32, 21, color)
    
    return img

func generate_skill_icons():
    var icons = {
        "fireball": Color(1.0, 0.4, 0.1),
        "heal": Color(0.3, 1.0, 0.4),
        "dash": Color(0.3, 0.6, 1.0),
        "iron_skin": Color(0.6, 0.6, 0.6)
    }
    
    for icon_name in icons:
        var img = create_skill_icon(icon_name, icons[icon_name])
        save_image(img, "res://Icons/Skills/%s.png" % icon_name)

func create_skill_icon(type: String, color: Color) -> Image:
    var img = Image.create(48, 48, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    match type:
        "fireball":
            draw_circle_filled(img, 24, 24, 15, color)
            draw_circle_filled(img, 19, 19, 6, color.lightened(0.4))
        "heal":
            draw_heart_icon(img, color)
        "dash":
            draw_arrow_right_icon(img, color)
        "iron_skin":
            draw_shield_icon(img, color)
        _:
            draw_circle_filled(img, 24, 24, 16, color)
    
    return img

func generate_item_icons():
    var icons = {
        "health_potion_small": Color(0.9, 0.2, 0.3),
        "health_potion_large": Color(0.9, 0.2, 0.3),
        "iron_sword": Color(0.7, 0.7, 0.75),
        "steel_armor": Color(0.5, 0.5, 0.6)
    }
    
    for icon_name in icons:
        var img = create_item_icon(icon_name, icons[icon_name])
        save_image(img, "res://Icons/Items/%s.png" % icon_name)

func create_item_icon(type: String, color: Color) -> Image:
    var img = Image.create(48, 48, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    match type:
        "health_potion_small":
            draw_bottle_shape(img, 24, 26, color, 8)
        "health_potion_large":
            draw_bottle_shape(img, 24, 26, color, 12)
        "iron_sword":
            draw_sword_icon(img, color)
        "steel_armor":
            draw_armor_icon(img, color)
        _:
            draw_circle_filled(img, 24, 24, 16, color)
    
    return img

func generate_service_icons():
    var img = Image.create(48, 48, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    draw_circle_filled(img, 24, 24, 16, Color(0.5, 0.5, 0.5))
    save_image(img, "res://Icons/Services/default.png")

func generate_enemy_images():
    print("--- 生成敌人图像 ---")
    
    var enemies = {
        "Cultist": {"color": Color(0.6, 0.3, 0.6), "type": "humanoid"},
        "JawWorm": {"color": Color(0.5, 0.4, 0.3), "type": "beast"},
        "Lagavulin": {"color": Color(0.4, 0.5, 0.6), "type": "construct"},
        "TheGuardian": {"color": Color(0.7, 0.3, 0.3), "type": "boss"}
    }
    
    for enemy_name in enemies:
        var data = enemies[enemy_name]
        var img = create_enemy_image(data["color"], data["type"])
        save_image(img, "res://Images/Enemies/%s.png" % enemy_name)
        
        var icon = create_enemy_icon(data["color"])
        save_image(icon, "res://Icons/Enemies/%s.png" % enemy_name.to_lower())

func create_enemy_image(color: Color, type: String) -> Image:
    var img = Image.create(150, 200, false, Image.FORMAT_RGBA8)
    img.fill(color.darkened(0.5))
    
    # 绘制背景纹理
    for y in range(200):
        for x in range(150):
            var noise = sin(x * 0.1) * cos(y * 0.1) * 0.05
            var c = color.darkened(0.4 + noise)
            img.set_pixel(x, y, c)
    
    var dark = color.darkened(0.3)
    var light = color.lightened(0.2)
    
    match type:
        "humanoid":
            draw_circle_filled(img, 75, 70, 25, dark)
            draw_ellipse_filled(img, 75, 120, 30, 45, dark)
            draw_line_thick(img, 50, 110, 30, 150, light, 5)
            draw_line_thick(img, 100, 110, 120, 150, light, 5)
        "beast":
            draw_ellipse_filled(img, 75, 100, 50, 35, dark)
            draw_circle_filled(img, 40, 90, 12, dark)
            draw_circle_filled(img, 110, 90, 12, dark)
            draw_circle_filled(img, 40, 88, 5, Color(1, 0.3, 0.3))
            draw_circle_filled(img, 110, 88, 5, Color(1, 0.3, 0.3))
        "construct":
            draw_rect_filled(img, 40, 60, 70, 80, dark)
            draw_rect_filled(img, 50, 70, 20, 20, Color(0.2, 0.8, 1))
            draw_rect_filled(img, 80, 70, 20, 20, Color(0.2, 0.8, 1))
            draw_rect_filled(img, 60, 110, 30, 20, light)
        "boss":
            draw_circle_filled(img, 75, 80, 40, dark)
            draw_ellipse_filled(img, 75, 140, 45, 50, dark)
            draw_circle_filled(img, 57, 75, 8, Color(1, 0.5, 0))
            draw_circle_filled(img, 93, 75, 8, Color(1, 0.5, 0))
            draw_line_thick(img, 40, 50, 30, 30, light, 6)
            draw_line_thick(img, 110, 50, 120, 30, light, 6)
    
    draw_frame(img, 150, 200, color.lightened(0.3))
    
    return img

func create_enemy_icon(color: Color) -> Image:
    var img = Image.create(64, 64, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    draw_circle_filled(img, 32, 32, 28, color.darkened(0.3))
    draw_circle_filled(img, 32, 32, 24, color)
    draw_circle_filled(img, 26, 26, 4, color.lightened(0.4))
    
    return img

func generate_relic_images():
    print("--- 生成遗物图像 ---")
    
    for i in range(20):
        var color = Color(rng.randf(), rng.randf(), rng.randf())
        var shape = rng.randi_range(0, 5)
        
        var img = create_relic_image(color, shape)
        save_image(img, "res://Images/Relics/Relic_%d.png" % i)
        
        var icon = create_relic_icon(color)
        save_image(icon, "res://Icons/Relics/relic_%d.png" % i)

func create_relic_image(color: Color, shape_type: int) -> Image:
    var img = Image.create(80, 80, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    draw_circle_filled(img, 40, 40, 37, color.darkened(0.3))
    draw_circle_filled(img, 40, 40, 34, color)
    
    match shape_type:
        0:
            draw_diamond_shape(img, 40, 40, 22, Color.WHITE)
        1:
            draw_small_star(img, 40, 40, 20, Color.WHITE)
        2:
            draw_eye_shape(img, 40, 40, 18, Color.WHITE)
        3:
            draw_cross_shape(img, 40, 40, 20, Color.WHITE)
        4:
            draw_moon_shape(img, 40, 40, 20, Color.WHITE)
        _:
            draw_ring_shape(img, 40, 40, 16, Color.WHITE)
    
    return img

func create_relic_icon(color: Color) -> Image:
    var img = Image.create(48, 48, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    draw_circle_filled(img, 24, 24, 21, color.darkened(0.3))
    draw_circle_filled(img, 24, 24, 18, color)
    
    return img

func generate_potion_images():
    print("--- 生成药水图像 ---")
    
    for i in range(15):
        var color = Color(rng.randf(), rng.randf(), rng.randf())
        var img = create_potion_image(color)
        save_image(img, "res://Images/Potions/Potion_%d.png" % i)

func create_potion_image(liquid_color: Color) -> Image:
    var img = Image.create(56, 56, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    draw_bottle_shape(img, 28, 30, liquid_color)
    
    return img

func generate_event_images():
    print("--- 生成事件图像 ---")
    
    for i in range(10):
        var color = Color(rng.randf() * 0.5 + 0.25, rng.randf() * 0.5 + 0.25, rng.randf() * 0.5 + 0.25)
        var img = create_event_image(color)
        save_image(img, "res://Images/Events/Event_%d.png" % i)

func create_event_image(base_color: Color) -> Image:
    var img = Image.create(300, 200, false, Image.FORMAT_RGBA8)
    
    for y in range(200):
        for x in range(300):
            var noise = sin(x * 0.067) * cos(y * 0.067) * 0.1
            var c = base_color.lightened(noise)
            img.set_pixel(x, y, c)
    
    # 添加装饰性元素
    for i in range(30):
        var x = rng.randi_range(20, 280)
        var y = rng.randi_range(20, 180)
        var size = rng.randi_range(5, 15)
        var c = base_color.lightened(rng.randf_range(0.1, 0.3))
        c.a = 0.5
        draw_circle_filled(img, x, y, size, c)
    
    return img

func generate_card_icons():
    print("--- 生成卡牌图标 ---")
    
    for i in range(30):
        var color = Color(rng.randf(), rng.randf(), rng.randf())
        var card_type = rng.randi_range(0, 2)
        var img = create_card_icon(color, card_type)
        save_image(img, "res://Icons/Cards/card_%d.png" % i)

func create_card_icon(color: Color, type: int) -> Image:
    var img = Image.create(64, 64, false, Image.FORMAT_RGBA8)
    img.fill(Color(0, 0, 0, 0))
    
    draw_rounded_rect(img, 4, 4, 56, 56, 6, color.darkened(0.3))
    draw_rounded_rect(img, 6, 6, 52, 52, 5, color)
    
    match type:
        0:
            draw_sword_icon(img, Color.WHITE)
        1:
            draw_shield_icon(img, Color.WHITE)
        2:
            draw_small_star(img, 32, 32, 15, Color.WHITE)
    
    return img

# ==================== 绘图函数 ====================

func save_image(img: Image, path: String):
    var absolute_path = path.replace("res://", "")
    var error = img.save_png(absolute_path)
    if error == OK:
        print("已保存: %s" % path)
    else:
        print("保存失败: %s, 错误: %d" % [path, error])

func draw_circle_filled(img: Image, cx: int, cy: int, radius: int, color: Color):
    for dy in range(-radius, radius + 1):
        for dx in range(-radius, radius + 1):
            if dx * dx + dy * dy <= radius * radius:
                var px = cx + dx
                var py = cy + dy
                if px >= 0 and px < img.get_width() and py >= 0 and py < img.get_height():
                    img.set_pixel(px, py, color)

func draw_ellipse_filled(img: Image, cx: int, cy: int, rx: int, ry: int, color: Color):
    for dy in range(-ry, ry + 1):
        for dx in range(-rx, rx + 1):
            if float(dx * dx) / (rx * rx) + float(dy * dy) / (ry * ry) <= 1.0:
                var px = cx + dx
                var py = cy + dy
                if px >= 0 and px < img.get_width() and py >= 0 and py < img.get_height():
                    img.set_pixel(px, py, color)

func draw_rect_filled(img: Image, x: int, y: int, w: int, h: int, color: Color):
    for dy in range(h):
        for dx in range(w):
            var px = x + dx
            var py = y + dy
            if px >= 0 and px < img.get_width() and py >= 0 and py < img.get_height():
                img.set_pixel(px, py, color)

func draw_line_thick(img: Image, x0: int, y0: int, x1: int, y1: int, color: Color, thickness: int = 2):
    var steps = max(abs(x1 - x0), abs(y1 - y0)) * 2
    for i in range(steps + 1):
        var t = float(i) / steps
        var x = int(lerf(float(x0), float(x1), t))
        var y = int(lerpf(float(y0), float(y1), t))
        draw_dot(img, x, y, thickness, color)

func draw_dot(img: Image, x: int, y: int, size: int, color: Color):
    for dy in range(-size, size + 1):
        for dx in range(-size, size + 1):
            if dx * dx + dy * dy <= size * size:
                var px = x + dx
                var py = y + dy
                if px >= 0 and px < img.get_width() and py >= 0 and py < img.get_height():
                    img.set_pixel(px, py, color)

func draw_frame(img: Image, w: int, h: int, color: Color):
    color.a = 0.6
    for x in range(w):
        img.set_pixel(x, 0, color)
        img.set_pixel(x, 1, color)
        img.set_pixel(x, h - 1, color)
        img.set_pixel(x, h - 2, color)
    for y in range(h):
        img.set_pixel(0, y, color)
        img.set_pixel(1, y, color)
        img.set_pixel(w - 1, y, color)
        img.set_pixel(w - 2, y, color)

func draw_rounded_rect(img: Image, x: int, y: int, w: int, h: int, radius: int, color: Color):
    for ry in range(y, y + h):
        for rx in range(x, x + w):
            if rx >= 0 and rx < img.get_width() and ry >= 0 and ry < img.get_height():
                var lx = rx - x
                var ly = ry - y
                var in_corner = (lx < radius and ly < radius) or \
                                (lx >= w - radius and ly < radius) or \
                                (lx < radius and ly >= h - radius) or \
                                (lx >= w - radius and ly >= h - radius)
                
                if in_corner:
                    var cx = radius if lx < radius else w - radius - 1
                    var cy = radius if ly < radius else h - radius - 1
                    var dist = sqrt((lx - cx) * (lx - cx) + (ly - cy) * (ly - cy))
                    if dist <= radius:
                        img.set_pixel(rx, ry, color)
                else:
                    img.set_pixel(rx, ry, color)

func draw_triangle_filled(img: Image, x0: int, y0: int, x1: int, y1: int, x2: int, y2: int, color: Color):
    var min_x = min(x0, min(x1, x2))
    var max_x = max(x0, max(x1, x2))
    var min_y = min(y0, min(y1, y2))
    var max_y = max(y0, max(y1, y2))
    
    for y in range(min_y, max_y + 1):
        for x in range(min_x, max_x + 1):
            var d1 = (x1 - x0) * (y - y0) - (x - x0) * (y1 - y0)
            var d2 = (x2 - x1) * (y - y1) - (x - x1) * (y2 - y1)
            var d3 = (x0 - x2) * (y - y2) - (x - x2) * (y0 - y2)
            
            var has_neg = (d1 < 0) or (d2 < 0) or (d3 < 0)
            var has_pos = (d1 > 0) or (d2 > 0) or (d3 > 0)
            
            if not (has_neg and has_pos):
                if x >= 0 and x < img.get_width() and y >= 0 and y < img.get_height():
                    img.set_pixel(x, y, color)

# ==================== 图标绘制函数 ====================

func draw_heart_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    draw_circle_filled(img, c - 6, c - 3, 7, color)
    draw_circle_filled(img, c + 6, c - 3, 7, color)
    draw_triangle_filled(img, c - 12, c + 1, c + 12, c + 1, c, c + 16, color)

func draw_arrow_up_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    var size = img.get_width()
    draw_line_thick(img, c, size - 8, c, 8, color, 4)
    draw_line_thick(img, c, 8, c - 8, 18, color, 4)
    draw_line_thick(img, c, 8, c + 8, 18, color, 4)

func draw_recall_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    draw_arc(img, c, c, 12, 0, PI * 1.5, color, 3)
    draw_line_thick(img, c + 12, c, c + 12, c - 8, color, 3)
    draw_line_thick(img, c + 12, c, c + 4, c, color, 3)

func draw_hammer_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    draw_rect_filled(img, c - 10, c - 15, 20, 12, color)
    draw_rect_filled(img, c - 3, c - 3, 6, 20, color.darkened(0.2))

func draw_trophy_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    draw_rect_filled(img, c - 12, c - 15, 24, 20, color)
    draw_rect_filled(img, c - 5, c + 5, 10, 8, color.darkened(0.2))
    draw_rect_filled(img, c - 10, c + 13, 20, 4, color.darkened(0.3))
    draw_rect_filled(img, c - 18, c - 12, 6, 12, color.lightened(0.2))
    draw_rect_filled(img, c + 12, c - 12, 6, 12, color.lightened(0.2))

func draw_skull_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    draw_ellipse_filled(img, c, c - 5, 15, 18, color)
    draw_rect_filled(img, c - 10, c + 8, 20, 10, color)
    draw_circle_filled(img, c - 6, c - 8, 4, Color(0.1, 0.1, 0.1))
    draw_circle_filled(img, c + 6, c - 8, 4, Color(0.1, 0.1, 0.1))

func draw_shield_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    draw_line_thick(img, c - 15, c - 15, c + 15, c - 15, color, 4)
    draw_line_thick(img, c - 15, c - 15, c - 15, c, color, 3)
    draw_line_thick(img, c + 15, c - 15, c + 15, c, color, 3)
    draw_line_thick(img, c - 15, c, c, c + 18, color, 4)
    draw_line_thick(img, c + 15, c, c, c + 18, color, 4)

func draw_arrow_right_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    var size = img.get_width()
    draw_line_thick(img, 8, c, size - 8, c, color, 4)
    draw_line_thick(img, size - 12, c, size - 20, c - 8, color, 3)
    draw_line_thick(img, size - 12, c, size - 20, c + 8, color, 3)

func draw_bottle_shape(img: Image, cx: int, cy: int, liquid_color: Color, size: int = 10):
    draw_rect_filled(img, cx - 3, cy - size - 4, 6, 5, Color(0.7, 0.7, 0.75))
    draw_ellipse_filled(img, cx, cy, size, size + 4, Color(0.7, 0.7, 0.75))
    draw_ellipse_filled(img, cx, cy + 3, size - 3, size, liquid_color)
    draw_circle_filled(img, cx, cy - size, 3, Color(0.8, 0.85, 0.9))

func draw_sword_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    draw_line_thick(img, c - 8, c + 14, c + 3, c - 12, color, 3)
    draw_line_thick(img, c + 3, c - 12, c + 10, c - 6, color, 3)
    draw_line_thick(img, c - 6, c + 14, c + 6, c + 14, color, 4)
    draw_line_thick(img, c - 8, c + 18, c - 4, c + 22, color, 2)
    draw_line_thick(img, c + 4, c + 18, c + 8, c + 22, color, 2)

func draw_armor_icon(img: Image, color: Color):
    var c = img.get_width() / 2
    draw_ellipse_filled(img, c, c, 14, 18, color)
    draw_ellipse_filled(img, c, c, 10, 14, color.lightened(0.2))
    draw_rect_filled(img, c - 3, c - 8, 6, 16, color.darkened(0.2))

func draw_small_star(img: Image, cx: int, cy: int, outer_radius: int, color: Color):
    var inner_radius = outer_radius * 2 / 5
    for i in range(10):
        var angle1 = (i * PI / 5.0) - PI / 2.0
        var angle2 = ((i + 0.5) * PI / 5.0) - PI / 2.0
        var r1 = outer_radius if i % 2 == 0 else inner_radius
        var r2 = inner_radius if i % 2 == 0 else outer_radius
        var x1 = cx + int(cos(angle1) * r1)
        var y1 = cy + int(sin(angle1) * r1)
        var x2 = cx + int(cos(angle2) * r2)
        var y2 = cy + int(sin(angle2) * r2)
        draw_line_thick(img, x1, y1, x2, y2, color, 2)

func draw_diamond_shape(img: Image, cx: int, cy: int, size: int, color: Color):
    draw_triangle_filled(img, cx, cy - size, cx - size, cy, cx, cy + size, color)
    draw_triangle_filled(img, cx, cy - size, cx + size, cy, cx, cy + size, color)

func draw_eye_shape(img: Image, cx: int, cy: int, size: int, color: Color):
    draw_ellipse_filled(img, cx, cy, size, size / 2, color)
    draw_circle_filled(img, cx, cy, size / 3, Color(0.1, 0.1, 0.15))
    draw_circle_filled(img, cx - size / 6, cy - size / 6, size / 6, Color.WHITE)

func draw_cross_shape(img: Image, cx: int, cy: int, size: int, color: Color):
    draw_line_thick(img, cx, cy - size, cx, cy + size, color, 4)
    draw_line_thick(img, cx - size, cy, cx + size, cy, color, 4)

func draw_moon_shape(img: Image, cx: int, cy: int, size: int, color: Color):
    draw_circle_filled(img, cx, cy, size, color)
    draw_circle_filled(img, cx + size / 2, cy - size / 3, size * 3 / 4, Color(0.08, 0.06, 0.12))

func draw_ring_shape(img: Image, cx: int, cy: int, radius: int, color: Color):
    draw_circle_filled(img, cx, cy, radius, color)
    draw_circle_filled(img, cx, cy, radius * 2 / 3, Color(0.1, 0.08, 0.12))

func draw_arc(img: Image, cx: int, cy: int, radius: int, start_angle: float, end_angle: float, color: Color, thickness: int = 1):
    var a = start_angle
    while a <= end_angle:
        var x = cx + int(cos(a) * radius)
        var y = cy + int(sin(a) * radius)
        draw_dot(img, x, y, thickness, color)
        a += 0.02
