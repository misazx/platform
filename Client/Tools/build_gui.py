#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
v7.4 - 彻底修复.app沙箱环境问题
在Python进程启动时就强制注入DOTNET_ROOT到os.environ
确保.app和.command行为完全一致
"""

import os
import sys
import subprocess
import threading
import shutil
from datetime import datetime
from pathlib import Path

# ===== 关键: 在导入tkinter之前就设置环境变量 =====
# .app通过Launch Services启动时环境变量可能丢失
# 这里强制注入，确保os.environ中始终有DOTNET_ROOT
_dotnet_paths = [
    '/usr/local/share/dotnet',
    '/opt/homebrew/share/dotnet',
]
for _dp in _dotnet_paths:
    if os.path.exists(_dp):
        os.environ['DOTNET_ROOT'] = _dp
        os.environ['DOTNET_ROOT_ARM64'] = _dp
        # 同时确保PATH包含dotnet
        if _dp not in os.environ.get('PATH', ''):
            os.environ['PATH'] = f"{_dp}:{os.environ.get('PATH', '')}"
        break

from tkinter import *
from tkinter import ttk, messagebox, filedialog


class BuildTool:
    def __init__(self):
        self.root = Tk()
        self.root.title("Godot Build Tool v7.4")
        self.root.geometry("1000x680")

        self.script_dir = Path(__file__).parent.resolve()
        self.project_root = self.script_dir.parent.parent
        self.build_dir = self.project_root / "build"

        self.godot_path = ""
        self.debug_mode = BooleanVar(value=False)
        self.clean_build = BooleanVar(value=False)

        self.platforms = [
            ('windows', 'Windows'),
            ('mac', 'macOS'),
            ('android', 'Android'),
            ('ios', 'iOS'),
            ('web', 'Web'),
            ('wechat', 'WeChat'),
        ]
        self.platform_vars = {p[0]: IntVar(value=0) for p in self.platforms}
        self.platform_buttons = {}

        self.is_building = False
        self.log_lines = []
        self.progress_var = DoubleVar(value=0)

        self._build()
        # 启动后显示环境变量状态
        self._log(f"[BOOT] DOTNET_ROOT={os.environ.get('DOTNET_ROOT', 'NOT SET')}", 'info' if 'DOTNET_ROOT' in os.environ else 'err')
        self._log(f"[BOOT] PATH has dotnet: {('/usr/local/share/dotnet' in os.environ.get('PATH', '')) or ('/opt/homebrew/share/dotnet' in os.environ.get('PATH', ''))}", 'info')
        self.root.protocol("WM_DELETE_WINDOW", self._on_close)

        # 启动时自动尝试查找 Godot
        self.root.after(500, self._auto_find_on_startup)

    def _build(self):
        R = self.root

        # ===== 主区域 =====
        main = Frame(R)
        main.pack(fill=BOTH, expand=True, padx=10, pady=5)

        # === LEFT: 平台按钮 ===
        left = Frame(main)
        left.pack(side=LEFT, fill=Y, padx=(0, 10))

        Label(left, text="Platforms", font=('Helvetica', 12, 'bold')).pack(anchor='w', pady=(0, 5))

        for pid, name in self.platforms:
            var = self.platform_vars[pid]
            btn = Button(left, text=f"  [ ] {name}  ",
                         font=('Helvetica', 11), fg='black',
                         height=1, cursor='hand2',
                         command=lambda p=pid: self._toggle(p))
            btn.pack(fill=X, pady=2)
            self.platform_buttons[pid] = {'btn': btn, 'var': var, 'name': name}

        fb = Frame(left)
        fb.pack(fill=X, pady=(8, 0))
        Button(fb, text="Select All", command=lambda: self._set_all(1),
               font=('', 9), fg='black').pack(side=LEFT, fill=X, expand=True, padx=2)
        Button(fb, text="Clear All", command=lambda: self._set_all(0),
               font=('', 9), fg='black').pack(side=LEFT, fill=X, expand=True, padx=2)

        # 平台流程按钮
        Button(left, text="📋 Platform Workflows",
               command=self._show_workflows,
               font=('Helvetica', 10), bg='#9C27B0', fg='white',
               height=1, cursor='hand2').pack(fill=X, pady=(10, 2))

        # === MIDDLE: 配置 ===
        mid = Frame(main)
        mid.pack(side=LEFT, fill=Y, padx=(0, 10))

        Label(mid, text="Configuration", font=('Helvetica', 12, 'bold')).pack(anchor='w', pady=(0, 5))

        # Godot 路径区域
        path_frame = Frame(mid)
        path_frame.pack(fill=X, pady=5)

        Label(path_frame, text="Godot Path:", font=('Helvetica', 10)).pack(anchor='w')

        bf = Frame(path_frame)
        bf.pack(fill=X, pady=3)
        Button(bf, text="Auto-Find", command=self._find_godot,
               font=('', 9), bg='#FF9800', fg='black', width=10).pack(side=LEFT, padx=2)
        Button(bf, text="Browse .app", command=self._browse_godot,
               font=('', 9), bg='#4A9EFF', fg='black', width=10).pack(side=LEFT, padx=2)

        # 当前路径显示
        self.btn_path_display = Button(path_frame, text="(click Auto-Find or Browse)",
                                        font=('Courier', 9), fg='black', height=2,
                                        relief=SUNKEN, anchor='w')
        self.btn_path_display.pack(fill=X, pady=3)

        # 选项
        opts = Frame(mid)
        opts.pack(fill=X, pady=8)
        Checkbutton(opts, text="Debug Mode", variable=self.debug_mode,
                   font=('', 10)).pack(anchor='w')
        Checkbutton(opts, text="Clean Build", variable=self.clean_build,
                   font=('', 10)).pack(anchor='w')

        # 操作按钮
        Label(mid, text="Actions", font=('Helvetica', 12, 'bold')).pack(anchor='w', pady=(10, 5))

        self.btn_start = Button(mid, text=">>> START BUILD <<<",
                                 command=self._start_build,
                                 font=('Helvetica', 12, 'bold'),
                                 bg='#4CAF50', fg='white', height=2)
        self.btn_start.pack(fill=X, pady=5)

        self.btn_stop = Button(mid, text="[ STOP ]",
                               command=self._stop_build,
                               font=('', 10), bg='#F44336', fg='white',
                               state='disabled', height=1)
        self.btn_stop.pack(fill=X, pady=3)

        fa = Frame(mid)
        fa.pack(fill=X, pady=5)
        Button(fa, text="Clean Dir", command=self._clean_dir,
               font=('', 9), bg='#607D8B', fg='white').pack(side=LEFT, fill=X, expand=True, padx=2)
        Button(fa, text="Open Output", command=self._open_output,
               font=('', 9), bg='#795548', fg='white').pack(side=LEFT, fill=X, expand=True, padx=2)

        # === RIGHT: 日志 ===
        right = Frame(main)
        right.pack(side=LEFT, fill=BOTH, expand=True)

        Label(right, text="Build Log", font=('Helvetica', 12, 'bold')).pack(anchor='w', pady=(0, 5))

        self.log_display = Listbox(right, font=('Courier', 9), height=25,
                                   selectmode=SINGLE, bg='#1a1a1a', fg='#00FF00')
        self.log_display.pack(fill=BOTH, expand=True)

        lc = Frame(right)
        lc.pack(fill=X, pady=5)
        Button(lc, text="Clear Log", command=self._clear_log,
               font=('', 9), width=10).pack(side=LEFT, padx=2)
        Button(lc, text="Save Log", command=self._save_log,
               font=('', 9), width=10).pack(side=LEFT, padx=2)

        pg = Frame(right)
        pg.pack(fill=X, pady=5)
        Label(pg, text="Progress:", font=('', 10)).pack(side=LEFT)
        ttk.Progressbar(pg, variable=self.progress_var, maximum=100,
                        length=180).pack(side=LEFT, padx=8)
        self.lbl_pct = Label(pg, text="0%", font=('', 10, 'bold'), width=4)
        self.lbl_pct.pack(side=RIGHT)

        # === 状态栏 ===
        self.status_btn = Button(R, text="Ready - Set Godot path, select platform, click START",
                                   font=('Helvetica', 10))
        self.status_btn.pack(fill=X, padx=10, pady=5)

    def _toggle(self, pid):
        info = self.platform_buttons[pid]
        var = info['var']
        btn = info['btn']
        val = var.get()
        new_val = 1 if val == 0 else 0
        var.set(new_val)
        if new_val == 1:
            btn.config(text=f"  [✓] {info['name']}  ", bg='#BBDEFB', fg='black')
        else:
            btn.config(text=f"  [ ] {info['name']}  ")
        self._update_status()

    def _set_all(self, val):
        for pid in self.platform_vars:
            if self.platform_vars[pid].get() != val:
                self._toggle(pid)

    def _update_status(self):
        sel = [p for p, v in self.platform_vars.items() if v.get()]
        if sel:
            names = [self.platform_buttons[p]['name'] for p in sel]
            path_info = f" | Path: {self.godot_path}" if self.godot_path else ""
            self.status_btn.config(text=f"Selected: {len(sel)} ({', '.join(names)}){path_info}")
        else:
            self.status_btn.config(text="Ready - Select at least 1 platform")

    def _find_godot(self):
        """手动触发查找 Godot"""
        self._log("[FIND] Starting manual search...", 'info')
        result = self._do_find_godot()
        if result:
            self.godot_path = result
            self.btn_path_display.config(text=f"{result}")
            self._log(f"[FIND] ✓ Found: {result}", 'ok')
            self._update_status()
        else:
            self._log("[WARN] Auto-find failed. Use Browse .app button.", 'warn')

    def _do_find_godot(self):
        """执行实际的 Godot 查找逻辑"""
        import glob, shutil

        home = Path.home()
        candidates = []

        # 搜索路径列表（按优先级排序）
        search_patterns = [
            '/Applications/Godot.app',
            '/Applications/Godot*.app',
            str(home / 'Downloads' / 'Godot*.app'),
            str(home / 'Downloads' / 'Godot_mono.app'),  # 用户的具体路径
            str(home / 'Applications' / 'Godot*.app'),
        ]

        self._log(f"[FIND] Searching in: {len(search_patterns)} locations", 'info')

        for pat in search_patterns:
            matches = glob.glob(pat)
            if matches:
                for m in matches:
                    candidates.append(m)
                    self._log(f"[FIND]   Found: {m}", 'info')

        # 检查 PATH 中的 godot 命令
        cmd = shutil.which("godot")
        if cmd:
            candidates.append(cmd)
            self._log(f"[FIND]   Found in PATH: {cmd}", 'info')

        # 去重并验证存在性
        seen = set()
        valid = []
        for c in candidates:
            if c not in seen and os.path.exists(c):
                seen.add(c)
                valid.append(c)

        self._log(f"[FIND] Valid candidates: {len(valid)}", 'info')

        if valid:
            app = valid[0]
            if app.endswith(".app"):
                exe = os.path.join(app, "Contents", "MacOS", "Godot")
                final = exe if os.path.exists(exe) else app
            else:
                final = app
            return final

        return None

    def _auto_find_on_startup(self):
        """启动时自动查找 Godot"""
        try:
            result = self._do_find_godot()
            if result:
                self.godot_path = result
                self.root.after(0, lambda: self.btn_path_display.config(text=f"{result}"))
                self.root.after(0, lambda: self._log(f"[AUTO] ✓ Found Godot: {result}", 'ok'))
                self.root.after(0, lambda: self._update_status())
            else:
                self.root.after(0, lambda: self._log("[AUTO] No Godot found. Please set path manually.", 'warn'))
        except Exception as e:
            self.root.after(0, lambda: self._log(f"[WARN] Auto-find error: {e}", 'warn'))

    def _browse_godot(self):
        path = filedialog.askopenfilename(
            title="Select Godot .app",
            filetypes=[("App Bundle", "*.app"), ("All Files", "*.*")],
        )
        if not path:
            return
        if path.endswith(".app"):
            exe = os.path.join(path, "Contents", "MacOS", "Godot")
            if os.path.exists(exe):
                path = exe
        self.godot_path = path
        self.btn_path_display.config(text=f"{path}")
        self._log(f"[BROWSE] Selected: {path}", 'ok')
        self._update_status()

    def _log(self, msg, lvl='info'):
        ts = datetime.now().strftime("[%H:%M:%S]")
        line = f"{ts} {msg}"
        self.log_lines.append(line)
        if hasattr(self, 'log_display'):
            self.log_display.insert(END, line)
            self.log_display.see(END)

    def _clear_log(self):
        self.log_lines = []
        if hasattr(self, 'log_display'):
            self.log_display.delete(0, END)

    def _save_log(self):
        p = filedialog.asksaveasfilename(defaultextension=".txt",
                                         initialfile=f"build_{datetime.now():%Y%m%d_%H%M%S}.txt")
        if p:
            with open(p, 'w') as f:
                f.write('\n'.join(self.log_lines))
            self._log(f"[SAVED] -> {p}", 'ok')

    def _check_export_templates(self):
        """检查 Godot 导出模板是否已安装"""
        # 获取 Godot 版本
        try:
            env = self._get_build_env()
            r = subprocess.run([self.godot_path, '--headless', '--version'],
                             capture_output=True, text=True, timeout=10, env=env)
            version = r.stdout.strip() if r.stdout else "unknown"
            self._log(f"[CHECK] Godot version: {version}", 'info')
        except Exception:
            version = "unknown"

        # 检查模板目录
        template_base = Path.home() / "Library" / "Application Support" / "Godot" / "export_templates"
        version_dirs = list(template_base.glob("*")) if template_base.exists() else []

        if not version_dirs:
            self._log("[ERROR] Export templates NOT installed!", 'err')
            self._log("[ERROR] Run: Godot Editor -> Editor -> Manage Export Templates -> Install", 'err')
            messagebox.showerror("Export Templates Missing",
                "Godot export templates are not installed!\n\n"
                "How to install:\n"
                "1. Open Godot Editor\n"
                "2. Editor -> Manage Export Templates\n"
                "3. Click 'Download and Install'\n\n"
                f"Or download manually:\n"
                f"https://github.com/godotengine/godot/releases")
            return False

        # 检查模板文件
        for vd in version_dirs:
            macos_template = vd / "macos.zip"
            if macos_template.exists():
                self._log(f"[CHECK] ✓ Export templates found: {vd.name}", 'ok')
                return True

        self._log("[WARN] Export templates directory exists but macos.zip not found", 'warn')
        self._log(f"[WARN] Checked: {version_dirs}", 'warn')
        return True

    def _start_build(self):
        sel = [p for p, v in self.platform_vars.items() if v.get()]
        if not sel:
            messagebox.showwarning("Warning", "Select at least 1 platform!")
            return
        if not self.godot_path or not os.path.exists(self.godot_path):
            messagebox.showerror("Error", "Set Godot path first!\n\nClick Auto-Find or Browse .app")
            return

        # 检查导出模板
        if not self._check_export_templates():
            return

        self.is_building = True
        self.btn_start.config(state='disabled', text='>>> BUILDING... <<<')
        self.btn_stop.config(state='normal')
        self.progress_var.set(0)
        self._log("=" * 55, 'hdr')
        self._log(f"START | Platforms: {len(sel)} | Godot: {self.godot_path}", 'hdr')
        self._log(f"Project Root: {self.project_root}", 'info')
        self._log(f"Build Script: {self.script_dir / 'build.py'}", 'info')
        self.status_btn.config(text="BUILDING...", bg='#FF9800')

        threading.Thread(target=self._run_build, args=(sel,), daemon=True).start()

    def _run_build(self, platforms):
        total = len(platforms)
        ok_count = 0
        t0 = datetime.now()
        build_script = self.script_dir / "build.py"

        # 验证 build.py 是否存在
        if not build_script.exists():
            self._log_safe(f"[ERROR] build.py not found: {build_script}", 'err')
            self._log_safe(f"[ERROR] Please ensure build.py exists in: {self.script_dir}", 'err')
            return

        if self.clean_build.get():
            self._exec([sys.executable, str(build_script), '--clean'])

        for i, plat in enumerate(platforms, 1):
            if not self.is_building:
                break
            name = dict(self.platforms)[plat]
            pct = (i - 1) / total * 100
            self.root.after(0, lambda p=pct: self.progress_var.set(p))
            self._log_safe(f"\n[{i}/{total}] Building: {name} ...", 'info')

            cmd = [sys.executable, str(build_script), '--platform', plat]
            if self.debug_mode.get():
                cmd.append('--debug')
            cmd += ['--godot-path', self.godot_path]

            if self._exec(cmd):
                ok_count += 1
                self._log_safe(f"   >>> OK! <<<", 'ok')
            else:
                self._log_safe(f"   >>> FAILED! <<<", 'err')

            pct = i / total * 100
            self.root.after(0, lambda p=pct: self.progress_var.set(p))
            self.root.after(0, lambda p=int(pct): self.lbl_pct.config(text=f"{p}%"))

        dt = (datetime.now() - t0).total_seconds()
        m, s = int(dt // 60), int(dt % 60)

        self.is_building = False
        self.btn_start.config(state='normal', text=">>> START BUILD <<<")
        self.btn_stop.config(state='disabled')
        self.progress_var.set(100)
        self.lbl_pct.config(text="100%")
        self._log_safe(f"\n{'='*55}", 'hdr')
        self._log_safe(f"DONE! Success: {ok_count}/{total} | Time: {m}m{s}s", 'hdr')

        if ok_count == total:
            self.status_btn.config(text=f"SUCCESS! ({m}m{s}s)")
            messagebox.showinfo("Done", f"All {total} OK!\nTime: {m}m{s}s")
        else:
            self.status_btn.config(text=f"PARTIAL: {ok_count}/{total}")
            messagebox.showwarning("Done", f"Success: {ok_count}/{total}")

    def _get_build_env(self):
        """构建包含 .NET 环境变量的环境 - 基于已注入的 os.environ"""
        env = dict(os.environ)

        # 二次确认: 如果 os.environ 中还没有（理论上不可能），再次设置
        if 'DOTNET_ROOT' not in env:
            for dp in ['/usr/local/share/dotnet', '/opt/homebrew/share/dotnet']:
                if os.path.exists(dp):
                    env['DOTNET_ROOT'] = dp
                    env['DOTNET_ROOT_ARM64'] = dp
                    break

        env['GODOT_DISABLE_CONSOLE'] = '1'
        return env

    def _exec(self, cmd):
        """执行构建命令（线程安全版本）"""
        try:
            # 显示要执行的命令
            cmd_str = ' '.join(cmd)
            self._log_safe(f"[CMD] {cmd_str}", 'info')

            # 验证可执行文件存在
            exe = cmd[0] if cmd else None
            if exe and not os.path.exists(exe):
                self._log_safe(f"   ERROR: Executable not found: {exe}", 'err')
                return False

            # 获取包含 .NET 环境变量的环境
            build_env = self._get_build_env()

            r = subprocess.run(cmd, cwd=str(self.project_root),
                             capture_output=True, text=True, timeout=600,
                             env=build_env)

            # 显示返回码
            self._log_safe(f"[RET] Return code: {r.returncode}", 'info' if r.returncode == 0 else 'err')

            # 显示标准输出
            if r.stdout:
                self._log_safe(f"[OUT] Output ({len(r.stdout)} bytes):", 'info')
                for ln in r.stdout.strip().split('\n'):
                    if ln.strip():
                        self._log_safe(f"   {ln}", 'info')

            # 显示标准错误（无论是否失败都显示）
            if r.stderr:
                self._log_safe(f"[ERR] Errors ({len(r.stderr)} bytes):", 'err')
                for ln in r.stderr.strip().split('\n'):
                    if ln.strip():
                        self._log_safe(f"   {ln}", 'err')

            # 如果没有输出但失败了，显示提示
            if not r.stdout and not r.stderr and r.returncode != 0:
                self._log_safe(f"   ! Command failed with no output (code={r.returncode})", 'warn')
                self._log_safe(f"   ! Possible causes:", 'warn')
                self._log_safe(f"   !   - .NET SDK not installed (need 8.0+)", 'warn')
                self._log_safe(f"   !   - Godot project has export errors", 'warn')
                self._log_safe(f"   !   - Export preset not configured", 'warn')

            return r.returncode == 0

        except FileNotFoundError as e:
            self._log_safe(f"   ERROR: File not found - {e}", 'err')
            self._log_safe(f"   ! Check: Does the executable exist?", 'warn')
            return False
        except PermissionError as e:
            self._log_safe(f"   ERROR: Permission denied - {e}", 'err')
            self._log_safe(f"   ! Try: chmod +x <executable>", 'warn')
            return False
        except subprocess.TimeoutExpired:
            self._log_safe(f"   ERROR: Timeout (600s exceeded)", 'err')
            return False
        except Exception as e:
            self._log_safe(f"   ERROR: {type(e).__name__} - {e}", 'err')
            import traceback
            self._log_safe(f"   TRACE: {traceback.format_exc()}", 'err')
            return False

    def _log_safe(self, msg, lvl='info'):
        """线程安全的日志方法 - 使用 root.after() 在主线程更新GUI"""
        def _do_log():
            ts = datetime.now().strftime("[%H:%M:%S]")
            line = f"{ts} {msg}"
            self.log_lines.append(line)
            if hasattr(self, 'log_display'):
                self.log_display.insert(END, line)
                self.log_display.see(END)
        self.root.after(0, _do_log)

    def _stop_build(self):
        if messagebox.askyesno("Confirm", "Stop?"):
            self.is_building = False
            self._log("[STOPPED]", 'warn')
            self.status_btn.config(text="STOPPED")

    def _clean_dir(self):
        if messagebox.askyesno("Confirm", f"Delete?\n{self.build_dir}"):
            if self.build_dir.exists():
                shutil.rmtree(self.build_dir)
                self._log("[CLEAN] Deleted", 'ok')
            else:
                self._log("[CLEAN] Not exist", 'info')

    def _open_output(self):
        self.build_dir.mkdir(parents=True, exist_ok=True)
        subprocess.run(['open', str(self.build_dir)])

    # ===== 平台打包流程数据 =====
    WORKFLOWS = {
        'windows': {
            'name': 'Windows 桌面版',
            'icon': '🪟',
            'steps': [
                ('1️⃣ 准备', [
                    '确保安装 Godot (Windows 版本)',
                    '.NET SDK 8.0+ 已安装',
                    '导出模板已下载: Editor → Manage Export Templates',
                ]),
                ('2️⃣ 配置 export_presets.cfg', [
                    'application/identifier: com.roguelikegame.slaythespire2',
                    'binary_format/architecture: x86_64',
                    'codesign/enable: false (桌面版无需签名)',
                    'texture_format/etc2: true',
                ]),
                ('3️⃣ 执行导出命令', [
                    'Godot --headless --path <project> --export-release Windows',
                    '输出: build/windows/杀戮尖塔2.exe',
                ]),
                ('4️⃣ 打包分发', [
                    '可使用 Inno Setup 或 NSIS 创建安装包',
                    '或直接分发 .exe + .pck 文件',
                    '用户需要安装 Visual C++ Redistributable',
                ]),
            ],
            'notes': 'Windows 导出需要 Windows 上的 Godot 编辑器或使用交叉编译'
        },
        'mac': {
            'name': 'macOS 桌面版',
            'icon': '🍎',
            'steps': [
                ('1️⃣ 准备', [
                    'Godot 4.6.1 Mono 已安装',
                    '.NET SDK 8.0+ 已安装 (dotnet --version)',
                    'DOTNET_ROOT 环境变量已设置',
                    '导出模板已安装到 ~/Library/Application Support/Godot/export_templates/',
                ]),
                ('2️⃣ 配置 export_presets.cfg', [
                    'application/bundle_identifier: com.roguelikegame.slaythespire2',
                    'binary_format/architecture: universal (Intel+Apple Silicon)',
                    'codesign/enable: false (开发阶段)',
                    'texture_format/bptc: true (macOS 原生格式)',
                ]),
                ('3️⃣ 执行导出命令', [
                    'DOTNET_ROOT=/usr/local/share/dotnet \\',
                    'Godot --headless --path <project> --export-release macOS',
                    '输出: build/macOS/杀戮尖塔2.zip (.app bundle)',
                ]),
                ('4️⃣ 代码签名 & 公证 (发布时)', [
                    '生成 Developer ID 证书: Apple Developer 账号',
                    'codesign --deep --sign "Developer ID ..." 杀戮尖塔2.app',
                    'xcrun notarytool submit ... (公证)',
                    'stapler staple ... (钉固票)',
                ]),
            ],
            'notes': '当前配置: universal 架构，支持 Intel 和 Apple Silicon Mac'
        },
        'android': {
            'name': 'Android APK',
            'icon': '🤖',
            'steps': [
                ('1️⃣ 准备', [
                    '安装 Android SDK (通过 Android Studio)',
                    '设置 ANDROID_SDK_ROOT 环境变量',
                    '安装 JDK 17 (Android 构建需要)',
                    'USB 调试模式已开启 (真机测试)',
                ]),
                ('2️⃣ 配置 Godot Editor', [
                    'Editor → Project Settings → Android',
                    '设置 SDK Path: /Users/<user>/Library/Android/sdk',
                    '启用 Custom Build (如需)',
                    'Debug Keystore: 自动生成或自定义',
                ]),
                ('3️⃣ 配置 export_presets.cfg', [
                    'apk/signed: false (调试) / true (发布)',
                    'permissions: INTERNET, ACCESS_NETWORK_STATE',
                    'screen/support_small: true',
                    'screen/orientations: portrait, landscape',
                ]),
                ('4️⃣ 执行导出', [
                    'Godot --headless --path <project> --export-release Android',
                    '输出: build/android/杀戮尖塔2.apk',
                ]),
                ('5️⃣ 发布到 Google Play', [
                    '创建 Google Play 开发者账号 ($25一次性)',
                    '上传 .aab (App Bundle) 格式',
                    '填写应用信息、隐私政策、内容分级',
                    '审核通过后上线',
                ]),
            ],
            'notes': 'APK 直接安装测试，AAB 用于商店发布'
        },
        'ios': {
            'name': 'iOS (需要 Mac + Xcode)',
            'icon': '📱',
            'steps': [
                ('1️⃣ 准备 (仅Mac)', [
                    '安装 Xcode (App Store 免费下载)',
                    '安装 Xcode Command Line Tools: xcode-select --install',
                    '注册 Apple Developer 账号 ($99/年)',
                    '创建 Provisioning Profile',
                ]),
                ('2️⃣ 配置 Godot Editor', [
                    'Editor → Export → iOS',
                    '选择 Team / Bundle Identifier',
                    '设置 App Icon (1024x1024 PNG)',
                    '配置 Info.plist (权限声明)',
                ]),
                ('3️⃣ 导出 Xcode 项目', [
                    'Godot --headless --path <project> --export-debug iOS',
                    '输出: build/ios/ 目录 (Xcode 项目)',
                ]),
                ('4️⃣ 在 Xcode 中构建', [
                    '打开 build/ios/*.xcworkspace',
                    '选择目标设备 (模拟器或真机)',
                    'Product → Run (运行) / Archive (归档)',
                    '真机测试需要在设备上信任开发者证书',
                ]),
                ('5️⃣ 提交 App Store', [
                    'Xcode → Product → Archive',
                    'Organizer → Distribute App',
                    '选择 App Store Connect',
                    '填写版本信息、截图、描述',
                    '提交审核',
                ]),
            ],
            'notes': 'iOS 必须在 Mac 上用 Xcode 构建，无法跨平台'
        },
        'web': {
            'name': 'Web (HTML5)',
            'icon': '🌐',
            'steps': [
                ('1️⃣ 准备', [
                    '无需额外依赖，任何平台均可导出',
                    'Web 导出使用 Godot 内置的 Emscripten',
                ]),
                ('2️⃣ 配置 export_presets.cfg', [
                    'html/canvas_resize_policy: 2 (适应窗口)',
                    'html/fullscreen: 可选',
                    'html/custom_html_head: 自定义 meta 标签',
                    'html/export_icon: favicon.ico',
                ]),
                ('3️⃣ 执行导出', [
                    'Godot --headless --path <project> --export-release Web',
                    '输出: build/web/index.html + *.wasm + *.js + *.pck',
                ]),
                ('4️⃣ 部署', [
                    '本地测试: python3 -m http.server 8080',
                    '静态托管: GitHub Pages / Netlify / Vercel',
                    'CDN加速: Cloudflare Pages',
                ]),
            ],
            'notes': 'Web 导出文件较大 (~10-50MB)，首次加载较慢。适合演示和轻量游戏。'
        },
        'wechat': {
            'name': '微信小游戏',
            'icon': '💬',
            'steps': [
                ('1️⃣ 准备', [
                    '先完成 Web 导出（微信基于 Web）',
                    '注册微信小游戏账号 (mp.weixin.qq.com)',
                    '下载微信开发者工具',
                    '获取 AppID (game.json 需要)',
                ]),
                ('2️⃣ Web 导出', [
                    '先执行: Godot --headless --export-release Web',
                    '输出: build/web/ 目录',
                ]),
                ('3️⃣ 转换为微信格式', [
                    '运行 wechat_converter.py 转换脚本',
                    '自动转换: index.html → game.js/game.json',
                    '处理: WASM 加载方式适配微信环境',
                    '处理: 音频格式转换 (MP3/Ogg → 微信兼容)',
                ]),
                ('4️⃣ 配置 game.json', [
                    '{',
                    '  "deviceOrientation": "portrait",',
                    '  "showStatusBar": false,',
                    '  "networkTimeout": { "request": 5000, "connectSocket": 5000 },',
                    '  "subpackages": []',
                    '}',
                ]),
                ('5️⃣ 微信开发者工具预览', [
                    '打开微信开发者工具',
                    '导入项目目录 (build/wechat/)',
                    '点击预览，手机扫码测试',
                    '确认功能正常后上传审核',
                ]),
                ('6️⃣ 提交审核', [
                    '微信开发者工具 → 上传代码',
                    '登录 mp.weixin.qq.com 提交审核',
                    '填写游戏信息、类目、隐私协议',
                    '审核通常 1-3 个工作日',
                ]),
            ],
            'notes': '微信小游戏限制: 包体 ≤ 4MB，首屏加载时间 ≤ 3秒。大资源需分包加载。'
        },
    }

    def _show_workflows(self):
        """显示平台打包流程窗口"""
        wf_win = Toplevel(self.root)
        wf_win.title("Platform Build Workflows")
        wf_win.geometry("850x620")
        wf_win.transient(self.root)
        wf_win.grab_set()

        # 平台选择
        top = Frame(wf_win)
        top.pack(fill=X, padx=10, pady=5)

        Label(top, text="Select Platform:", font=('Helvetica', 11, 'bold')).pack(side=LEFT)
        self.wf_platform = StringVar(value='mac')
        pf = Frame(top)
        pf.pack(side=LEFT, padx=10)

        for pid, name in self.platforms:
            rb = Radiobutton(pf, text=name, variable=self.wf_platform,
                            value=pid, command=self._update_workflow_display,
                            font=('Helvetica', 10))
            rb.pack(side=LEFT, padx=5)

        # 内容区域
        content = Frame(wf_win)
        content.pack(fill=BOTH, expand=True, padx=10, pady=5)

        # 左侧: 步骤列表
        left_p = Frame(content)
        left_p.pack(side=LEFT, fill=BOTH, expand=True, padx=(0, 5))
        Label(left_p, text="Steps", font=('Helvetica', 11, 'bold')).pack(anchor='w')
        self.wf_steps_list = Listbox(left_p, font=('Helvetica', 10), height=25,
                                        selectmode=SINGLE, bg='#ffffff', fg='black',
                                        selectbackground='#4A9EFF', selectforeground='white')
        self.wf_steps_list.pack(fill=BOTH, expand=True)

        # 右侧: 详细说明 (用 Label 替代 Text 避免兼容问题)
        right_p = Frame(content)
        right_p.pack(side=LEFT, fill=BOTH, expand=True, padx=(5, 0))
        Label(right_p, text="Details", font=('Helvetica', 11, 'bold')).pack(anchor='w')

        detail_frame = Frame(right_p)
        detail_frame.pack(fill=BOTH, expand=True)

        self.wf_detail_text = ""
        self.wf_detail_label = Label(detail_frame, text="",
                                       font=('Courier', 9), justify=LEFT,
                                       anchor='nw', bg='#f0f0f0', fg='black',
                                       wraplength=480)
        self.wf_detail_label.pack(fill=BOTH, expand=True, padx=2, pady=2)

        # 底部备注
        self.wf_notes = Label(wf_win, text="", font=('Helvetica', 9),
                              fg='#666', justify='left', anchor='w')
        self.wf_notes.pack(fill=X, padx=10, pady=5)

        # 关闭按钮
        Button(wf_win, text="Close", command=wf_win.destroy,
               font=('', 10), bg='#607D8B', fg='white').pack(pady=5)

        # 延迟初始化显示，确保窗口完全渲染
        wf_win.after(100, self._update_workflow_display)

    def _update_workflow_display(self):
        """更新流程显示"""
        pid = self.wf_platform.get()
        wf = self.WORKFLOWS.get(pid, {})

        # 更新步骤列表
        self.wf_steps_list.delete(0, END)
        if 'steps' in wf:
            for step_title, items in wf['steps']:
                self.wf_steps_list.insert(END, f"  {step_title}")

        # 更新详情 (使用 Label)
        icon = wf.get('icon', '')
        name = wf.get('name', pid)
        notes = wf.get('notes', '')

        lines = [f"{icon}  {name}", "=" * 50, ""]
        if 'steps' in wf:
            for step_title, items in wf['steps']:
                lines.append(f"▶ {step_title}")
                for item in items:
                    lines.append(f"   • {item}")
                lines.append("")

        self.wf_detail_text = "\n".join(lines)
        self.wf_detail_label.config(text=self.wf_detail_text)

        # 更新备注
        self.wf_notes.config(text=f"📝 {notes}" if notes else "")

    def _on_close(self):
        if self.is_building:
            if not messagebox.askyesno("Confirm", "Building, quit?"):
                return
            self.is_building = False
        self.root.destroy()


def main():
    try:
        BuildTool().root.mainloop()
    except Exception as e:
        print(f"FATAL: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
