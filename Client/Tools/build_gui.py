#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
v7.1 - 增强稳定版
修复语法错误 + 启动时自动检测 + 改进的错误处理
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
        self.root.title("Godot Build Tool v7.1")
        self.root.geometry("950x700")

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

        # ===== 标题 (Label 可能不可见，用 Message 或跳过) =====
        title_btn = Button(R, text="=== Godot Build Tool ===\nClick platforms below, then START",
                             font=('Helvetica', 14), height=2,
                             relief=GROOVE, bd=2)
        title_btn.pack(fill=X, padx=10, pady=8)

        # ===== 主区域 =====
        main = Frame(R)
        main.pack(fill=BOTH, expand=True, padx=10)

        # === LEFT: 平台按钮 ===
        left = Frame(main)
        left.pack(side=LEFT, fill=Y, padx=(0, 10))

        for pid, name in self.platforms:
            var = self.platform_vars[pid]
            btn = Button(left, text=f"  [ ] {name}  ",
                         font=('Helvetica', 12),
                         height=2, cursor='hand2',
                         command=lambda p=pid: self._toggle(p))
            btn.pack(fill=X, pady=3)
            self.platform_buttons[pid] = {'btn': btn, 'var': var, 'name': name}

        fb = Frame(left)
        fb.pack(fill=X, pady=6)
        Button(fb, text="Select All", command=lambda: self._set_all(1),
               font=('', 10)).pack(side=LEFT, fill=X, expand=True, padx=2)
        Button(fb, text="Clear All", command=lambda: self._set_all(0),
               font=('', 10)).pack(side=LEFT, fill=X, expand=True, padx=2)

        # === MIDDLE: 配置 ===
        mid = Frame(main)
        mid.pack(side=LEFT, fill=Y, padx=(0, 10))

        # Godot 路径 - 用 Button 显示和操作
        Button(mid, text="[Godot Path - Click to Set]",
               font=('Helvetica', 11), height=2,
               relief=GROOVE, bd=2, anchor='w',
               command=self._browse_godot).pack(fill=X, pady=4)

        bf = Frame(mid)
        bf.pack(fill=X, pady=4)
        Button(bf, text="Auto-Find", command=self._find_godot,
               font=('', 10), bg='#FF9800').pack(side=LEFT, fill=X, expand=True, padx=2)
        Button(bf, text="Browse .app", command=self._browse_godot,
               font=('', 10), bg='#4A9EFF').pack(side=LEFT, fill=X, expand=True, padx=2)

        # 当前路径显示按钮
        self.btn_path_display = Button(mid, text="(no path set)",
                                        font=('Courier', 10), height=3,
                                        relief=SUNKEN, anchor='w')
        self.btn_path_display.pack(fill=X, pady=4)

        Checkbutton(mid, text="Debug Mode", variable=self.debug_mode,
                   font=('', 11)).pack(anchor='w', pady=4)
        Checkbutton(mid, text="Clean Build", variable=self.clean_build,
                   font=('', 11)).pack(anchor='w', pady=4)

        # 操作按钮
        self.btn_start = Button(mid, text=">>> START BUILD <<<",
                                 command=self._start_build,
                                 font=('Helvetica', 14, 'bold'),
                                 bg='#4CAF50', fg='white', height=2)
        self.btn_start.pack(fill=X, pady=8)

        self.btn_stop = Button(mid, text="[ STOP ]",
                               command=self._stop_build,
                               font=('', 11), bg='#F44336', fg='white',
                               state='disabled')
        self.btn_stop.pack(fill=X, pady=4)

        fa = Frame(mid)
        fa.pack(fill=X, pady=8)
        Button(fa, text="Clean Dir", command=self._clean_dir,
               font=('', 10), bg='#607D8B', fg='white').pack(side=LEFT, fill=X, expand=True, padx=2)
        Button(fa, text="Open Output", command=self._open_output,
               font=('', 10), bg='#795548', fg='white').pack(side=LEFT, fill=X, expand=True, padx=2)

        # === RIGHT: 日志 (用 Button 模拟文本区) ===
        right = Frame(main)
        right.pack(side=LEFT, fill=BOTH, expand=True)

        Button(right, text="=== Build Log (see terminal for details) ===",
               font=('Helvetica', 11), anchor='w').pack(fill=X)

        self.log_display = Listbox(right, font=('Courier', 10), height=20,
                                   selectmode=SINGLE, bg='#111', fg='#DDD')
        self.log_display.pack(fill=BOTH, expand=True)

        lc = Frame(right)
        lc.pack(fill=X, pady=4)
        Button(lc, text="Clear Log", command=self._clear_log,
               font=('', 9)).pack(side=LEFT, padx=2)
        Button(lc, text="Save Log", command=self._save_log,
               font=('', 9)).pack(side=LEFT, padx=2)

        pg = Frame(right)
        pg.pack(fill=X, pady=4)
        Label(pg, text="Progress:", font=('', 11)).pack(side=LEFT)
        ttk.Progressbar(pg, variable=self.progress_var, maximum=100,
                        length=200).pack(side=LEFT, padx=8)
        self.lbl_pct = Label(pg, text="0%", font=('', 11, 'bold'), width=5)
        self.lbl_pct.pack(side=RIGHT)

        # === 状态栏 ===
        self.status_btn = Button(R, text="Ready - Set Godot path, select platform, click START",
                                   font=('Helvetica', 11))
        self.status_btn.pack(fill=X, padx=10, pady=8)

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
        import glob, shutil
        home = Path.home()
        candidates = []
        for pat in ['/Applications/Godot*', '/Applications/*Godot*',
                    str(home/'Applications'/'Godot*'), str(home/'Downloads'/'Godot*')]:
            candidates.extend(glob.glob(pat))
        cmd = shutil.which("godot")
        if cmd:
            candidates.append(cmd)
        seen = set()
        valid = []
        for c in candidates:
            if c not in seen and os.path.exists(c):
                seen.add(c)
                valid.append(c)
        if valid:
            app = valid[0]
            if app.endswith(".app"):
                exe = os.path.join(app, "Contents", "MacOS", "Godot")
                final = exe if os.path.exists(exe) else app
            else:
                final = app
            self.godot_path = final
            self.btn_path_display.config(text=f"{final}")
            self._log(f"[FIND] Found: {final}", 'ok')
            self._update_status()
        else:
            self._log("[WARN] Not found. Use Browse.", 'warn')
            messagebox.showinfo("Not Found",
                "Could not auto-find.\n\n"
                "Use 'Browse' and pick:\n"
                "/Users/zhuyong/Downloads/Godot_mono.app")

    def _auto_find_on_startup(self):
        """启动时自动查找 Godot，不弹出提示"""
        import glob, shutil
        try:
            home = Path.home()
            candidates = []
            # 常见安装位置
            search_paths = [
                '/Applications/Godot*',
                '/Applications/*Godot*',
                str(home/'Applications'/'Godot*'),
                str(home/'Downloads'/'Godot*'),
                '/usr/local/bin/godot*',
            ]
            for pat in search_paths:
                candidates.extend(glob.glob(pat))

            # 检查 PATH 中的 godot 命令
            cmd = shutil.which("godot")
            if cmd:
                candidates.append(cmd)

            # 去重并验证
            seen = set()
            valid = []
            for c in candidates:
                if c not in seen and os.path.exists(c):
                    seen.add(c)
                    valid.append(c)

            if valid:
                app = valid[0]
                if app.endswith(".app"):
                    exe = os.path.join(app, "Contents", "MacOS", "Godot")
                    final = exe if os.path.exists(exe) else app
                else:
                    final = app

                self.godot_path = final
                # 更新显示（使用 after 确保线程安全）
                self.root.after(0, lambda: self.btn_path_display.config(text=f"{final}"))
                self.root.after(0, lambda: self._log(f"[AUTO] Found Godot: {final}", 'ok'))
                self.root.after(0, lambda: self._update_status())
            else:
                self.root.after(0, lambda: self._log("[INFO] Auto-find: No Godot found. Please set manually.", 'info'))
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
        self.status_btn.config(text="BUILDING...", bg='#FF9800')

        threading.Thread(target=self._run_build, args=(sel,), daemon=True).start()

    def _run_build(self, platforms):
        total = len(platforms)
        ok_count = 0
        t0 = datetime.now()
        build_script = self.script_dir / "build.py"

        if self.clean_build.get():
            self._exec([sys.executable, str(build_script), '--clean'])

        for i, plat in enumerate(platforms, 1):
            if not self.is_building:
                break
            name = dict(self.platforms)[plat]
            pct = (i - 1) / total * 100
            self.root.after(0, lambda p=pct: self.progress_var.set(p))
            self._log(f"\n[{i}/{total}] Building: {name} ...", 'info')

            cmd = [sys.executable, str(build_script), '--platform', plat]
            if self.debug_mode.get():
                cmd.append('--debug')
            cmd += ['--godot-path', self.godot_path]

            if self._exec(cmd):
                ok_count += 1
                self._log(f"   >>> OK! <<<", 'ok')
            else:
                self._log(f"   >>> FAILED! <<<", 'err')

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
        self._log(f"\n{'='*55}", 'hdr')
        self._log(f"DONE! Success: {ok_count}/{total} | Time: {m}m{s}s", 'hdr')

        if ok_count == total:
            self.status_btn.config(text=f"SUCCESS! ({m}m{s}s)")
            messagebox.showinfo("Done", f"All {total} OK!\nTime: {m}m{s}s")
        else:
            self.status_btn.config(text=f"PARTIAL: {ok_count}/{total}")
            messagebox.showwarning("Done", f"Success: {ok_count}/{total}")

    def _exec(self, cmd):
        try:
            r = subprocess.run(cmd, cwd=str(self.project_root),
                             capture_output=True, text=True, timeout=600)
            if r.stdout:
                for ln in r.stdout.strip().split('\n'):
                    if ln.strip():
                        self._log(f"   {ln}", 'info')
            if r.stderr and r.returncode != 0:
                for ln in r.stderr.strip().split('\n'):
                    if ln.strip():
                        self._log(f"   ! {ln}", 'err')
            return r.returncode == 0
        except Exception as e:
            self._log(f"   ERROR: {e}", 'err')
            return False

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
