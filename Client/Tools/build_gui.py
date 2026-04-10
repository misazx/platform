#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
v7.3 - UI优化+日志增强版
修复UI重复问题 + 改进Godot查找 + 完整错误诊断
"""

import os
import sys
import subprocess
import threading
import shutil
from datetime import datetime
from pathlib import Path

from tkinter import *
from tkinter import ttk, messagebox, filedialog


class BuildTool:
    def __init__(self):
        self.root = Tk()
        self.root.title("Godot Build Tool v7.3")
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
                         font=('Helvetica', 11),
                         height=1, cursor='hand2',
                         command=lambda p=pid: self._toggle(p))
            btn.pack(fill=X, pady=2)
            self.platform_buttons[pid] = {'btn': btn, 'var': var, 'name': name}

        fb = Frame(left)
        fb.pack(fill=X, pady=(8, 0))
        Button(fb, text="Select All", command=lambda: self._set_all(1),
               font=('', 9)).pack(side=LEFT, fill=X, expand=True, padx=2)
        Button(fb, text="Clear All", command=lambda: self._set_all(0),
               font=('', 9)).pack(side=LEFT, fill=X, expand=True, padx=2)

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
               font=('', 9), bg='#FF9800', width=10).pack(side=LEFT, padx=2)
        Button(bf, text="Browse .app", command=self._browse_godot,
               font=('', 9), bg='#4A9EFF', width=10).pack(side=LEFT, padx=2)

        # 当前路径显示
        self.btn_path_display = Button(path_frame, text="(click Auto-Find or Browse)",
                                        font=('Courier', 9), height=2,
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

    def _start_build(self):
        sel = [p for p, v in self.platform_vars.items() if v.get()]
        if not sel:
            messagebox.showwarning("Warning", "Select at least 1 platform!")
            return
        if not self.godot_path or not os.path.exists(self.godot_path):
            messagebox.showerror("Error", "Set Godot path first!\n\nClick Auto-Find or Browse .app")
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

            r = subprocess.run(cmd, cwd=str(self.project_root),
                             capture_output=True, text=True, timeout=600,
                             env={**os.environ, 'GODOT_DISABLE_CONSOLE': '1'})

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
