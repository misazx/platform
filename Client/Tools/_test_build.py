#!/usr/bin/env python3
"""快速测试构建功能 - 模拟一次失败的构建来验证日志显示"""
import sys
sys.path.insert(0, '/Users/zhuyong/trae-game/Client/Tools')

from build_gui import BuildTool
import time

print("Starting GUI test...")
tool = BuildTool()

# 模拟用户操作
print("\n=== Test: Simulating a failed build ===")
tool.godot_path = "/Users/zhuyong/Downloads/Godot_mono.app/Contents/MacOS/Godot"
tool.platform_vars['mac'].set(1)

# 调用构建
print("Calling _start_build()...")
tool._start_build()

# 等待几秒看输出
print("Waiting 5 seconds for logs...")
time.sleep(5)

print(f"\nLog lines captured: {len(tool.log_lines)}")
if tool.log_lines:
    print("\n=== Log Output ===")
    for line in tool.log_lines[-20:]:  # 显示最后20行
        print(line)
else:
    print("WARNING: No log lines captured!")

print("\nTest completed. GUI window should still be open.")
tool.root.mainloop()
