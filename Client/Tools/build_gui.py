#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
v4.0 - 极简稳定版
原则: 只用 pack, 用 Button 做平台卡片, 系统字体, 确保每个像素都可见
"""

import os
import sys
import subprocess
import threading
import shutil
from datetime import datetime
from pathlib import Path

from tkinter import *
from tkinter import ttk, messagebox, filedialog, scrolledtext


class BuildTool:
    def __init__(self):
        self.root = Tk()
        self.root.title("Build Tool - Godot")
        self.root.geometry("1000x720")

        self.script_dir = Path(__file__).parent.resolve()
        self.project_root = self.script_dir.parent
        self.build_dir = self.project_root / "build"

        self.godot_path_var = StringVar(value="")
        self.debug_mode = BooleanVar(value=False)
        self.clean_build = BooleanVar(value=False)

        self.platforms = [
            ('windows', 'Windows', '#0078D7'),
            ('mac', 'macOS', '#555555'),
            ('android', 'Android', '#3DDC84'),
            ('ios', 'iOS', '#007AFF'),
            ('web', 'Web', '#FF9500'),
            ('wechat', 'WeChat', '#09BB07'),
        ]
        self.platform_vars = {p: IntVar(value=0) for p, _, _ in self.platforms}

        self.is_building = False
        self.log_text = None
        self.progress_var = DoubleVar(value=0)

        self._build()
        self.root.protocol("WM_DELETE_WINDOW", self._on_close)

    def _build(self):
        R = self.root

        # ====== 标题 ======
        ft = Frame(R, bg='#333')
        ft.pack(fill=X, padx=10, pady=(10, 6))
        Label(ft, text="Godot Build Tool - Multi-Platform",
              font=('', 18, 'bold'), bg='#333', fg='#FFF',
              anchor=W).pack(fill=X)
        Label(ft, text=f"Project: {self.project_root.name}",
              font=('', 10), bg='#333', fg='#AAA',
              anchor=W).pack(fill=X, pady=(2, 6))

        # ====== 主区域: 上(平台+配置) 下(日志) ======
        fm = Frame(R, bg='#222')
        fm.pack(fill=BOTH, expand=True, padx=10)

        # ---- 上半: 左平台 | 中配置 | 右日志 ----
        top = Frame(fm, bg='#222')
        top.pack(fill=BOTH, expand=True)

        # === LEFT: 平台 (纯 pack, 用 Button) ===
        left = Frame(top, bg='#222', width=340)
        left.pack(side=LEFT, fill=Y, padx=(0, 10))
        left.pack_propagate(False)

        Label(left, text="=== Target Platforms ===",
              font=('', 12, 'bold'), bg='#222', fg='#4A9EFF',
              anchor=W).pack(fill=X, pady=(0, 8))

        # 每个 platform 一个大 Button (100% 可见)
        for pid, name, color in self.platforms:
            var = self.platform_vars[pid]

            btn = Button(left,
                         text=f"  {name}  ",
                         font=('', 14, 'bold'),
                         bg=color, fg='white',
                         activebackground='#FFF', activeforeground=color,
                         relief='raised', bd=2, cursor='hand2',
                         height=2, anchor='center',
                         command=lambda p=pid, v=var, b=None: self._toggle_btn(p, v, b))
            
            # 存引用以便后续改颜色
            btn._pid = pid
            btn._var = var
            
            btn.pack(fill=X, pady=3)

        # 快捷按钮行
        fb = Frame(left, bg='#222')
        fb.pack(fill=X, pady=(10, 0))
        Button(fb, text="Select All", command=lambda: self._set_all(1),
               font=('', 9), bg='#4CAF50', fg='white',
               relief='flat').pack(side=LEFT, padx=2, expand=True, fill=X)
        Button(fb, text="Clear All", command=lambda: self._set_all(0),
               font=('', 9), bg='#F44336', fg='white',
               relief='flat').pack(side=LEFT, padx=2, expand=True, fill=X)

        # === MIDDLE: 配置 + 按钮 ===
        mid = Frame(top, bg='#222', width=300)
        mid.pack(side=LEFT, fill=Y, padx=(0, 10))
        mid.pack_propagate(False)

        Label(mid, text="=== Config ===",
              font=('', 12, 'bold'), bg='#222', fg='#4A9EFF',
              anchor=W).pack(fill=X, pady=(0, 8))

        # Godot 路径
        fr1 = Frame(mid, bg='#222')
        fr1.pack(fill=X, pady=4)
        Label(fr1, text="Godot:", font=('', 11),
              bg='#222', fg='white', width=7, anchor='w').pack(side=LEFT)
        Entry(fr1, textvariable=self.godot_path_var,
              font=('Courier', 11), bg='#444', fg='white',
              insertbackground='white', width=14).pack(side=LEFT, padx=4)
        Button(fr1, text="Find", command=self._find_godot,
               font=('', 9), bg='#FF9800', fg='white',
               relief='flat').pack(side=LEFT)

        # 选项
        fr2 = Frame(mid, bg='#222')
        fr2.pack(fill=X, pady=12)
        Checkbutton(fr2, text="Debug Mode", variable=self.debug_mode,
                   font=('', 11), bg='#222', fg='white',
                   selectcolor='#444', activebackground='#222').pack(anchor='w')
        Checkbutton(fr2, text="Clean Build", variable=self.clean_build,
                   font=('', 11), bg='#222', fg='white',
                   selectcolor='#444', activebackground='#222').pack(anchor='w', pady=(6, 0))

        # 操作区
        Label(mid, text="=== Actions ===",
              font=('', 12, 'bold'), bg='#222', fg='#4A9EFF',
              anchor=W).pack(fill=X, pady=(20, 8))

        self.btn_start = Button(mid, text=">>> START BUILD <<<",
                                 command=self._start_build,
                                 font=('', 15, 'bold'),
                                 bg='#4CAF50', fg='white',
                                 relief='raised', bd=3, cursor='hand2',
                                 height=2)
        self.btn_start.pack(fill=X, pady=4)

        self.btn_stop = Button(mid, text="[ STOP BUILD ]",
                               command=self._stop_build,
                               font=('', 12),
                               bg='#F44336', fg='white',
                               state='disabled', relief='flat')
        self.btn_stop.pack(fill=X, pady=4)

        fa = Frame(mid, bg='#222')
        fa.pack(fill=X, pady=(12, 0))
        Button(fa, text="Clean Dir", command=self._clean_dir,
               font=('', 10), bg='#607D8B', fg='white',
               relief='flat').pack(side=LEFT, padx=2, expand=True, fill=X)
        Button(fa, text="Open Output", command=self._open_output,
               font=('', 10), bg='#795548', fg='white',
               relief='flat').pack(side=LEFT, padx=2, expand=True, fill=X)

        # === RIGHT: 日志 ===
        right = Frame(top, bg='#222')
        right.pack(side=LEFT, fill=BOTH, expand=True)

        Label(right, text="=== Build Log ===",
              font=('', 12, 'bold'), bg='#222', fg='#4A9EFF',
              anchor=W).pack(fill=X, pady=(0, 5))

        self.log_text = scrolledtext.ScrolledText(
            right, wrap=WORD, font=('Courier', 11),
            bg='#111', fg='#DDD', insertbackground='white',
            height=22, bd=2, relief='sunken'
        )
        self.log_text.pack(fill=BOTH, expand=True)
        self.log_text.tag_configure('info', foreground='#888')
        self.log_text.tag_configure('ok', foreground='#4CAF50')
        self.log_text.tag_configure('err', foreground='#F44336')
        self.log_text.tag_configure('warn', foreground='#FF9800')
        self.log_text.tag_configure('hdr', foreground='#4A9EFF')

        lc = Frame(right, bg='#222')
        lc.pack(fill=X, pady=(5, 0))
        Button(lc, text="Clear Log",
               command=lambda: self.log_text.delete(1.0, END),
               font=('', 9), bg='#555', fg='white',
               relief='flat').pack(side=LEFT)
        Button(lc, text="Save Log", command=self._save_log,
               font=('', 9), bg='#555', fg='white',
               relief='flat').pack(side=LEFT, padx=8)

        pg = Frame(right, bg='#222')
        pg.pack(fill=X, pady=(6, 0))
        Label(pg, text="Progress:", font=('', 11),
              bg='#222', fg='white').pack(side=LEFT)
        ttk.Progressbar(pg, variable=self.progress_var, maximum=100,
                        length=280, mode='determinate').pack(side=LEFT, padx=8)
        self.lbl_pct = Label(pg, text="0%", font=('', 12, 'bold'),
                             bg='#222', fg='#4A9EFF', width=4)
        self.lbl_pct.pack(side=RIGHT)

        # ====== 状态栏 ======
        self.status = Label(R, text="Ready - Select platforms and click START",
                             font=('', 11), bg='#333', fg='#4CAF50',
                             anchor=W, padx=12, pady=8)
        self.status.pack(fill=X, padx=10, pady=(8, 10))

    def _toggle_btn(self, pid, var, btn):
        val = var.get()
        new_val = 1 if val == 0 else 0
        var.set(new_val)
        self._update_status()

    def _set_all(self, val):
        for v in self.platform_vars.values():
            v.set(val)
        self._update_status()

    def _sel_desk(self):
        desk = {'windows', 'mac', 'web'}
        for p, v in self.platform_vars.items():
            v.set(1 if p in desk else 0)
        self._update_status()

    def _sel_mob(self):
        mob = {'android', 'ios', 'wechat'}
        for p, v in self.platform_vars.items():
            v.set(1 if p in mob else 0)
        self._update_status()

    def _update_status(self):
        sel = [p for p, v in self.platform_vars.items() if v.get()]
        if sel:
            names = [n for p, n, _ in self.platforms if p in sel]
            self.status.config(text=f"Selected: {len(sel)} platforms -> {', '.join(names)}")
        else:
            self.status.config(text="Ready - Select at least 1 platform")

    def _find_godot(self):
        import glob
        for pat in ["/Applications/Godot.app/Contents/MacOS/Godot"]:
            found = glob.glob(pat)
            if found:
                self.godot_path_var.set(found[0])
                self._log(f"Found: {found[0]}", 'ok')
                return
        self._log("Not found, browse manually", 'warn')

    def _log(self, msg, lvl='info'):
        ts = datetime.now().strftime("[%H:%M:%S]")
        if self.log_text:
            self.log_text.insert(END, f"{ts} {msg}\n", lvl)
            self.log_text.see(END)

    def _save_log(self):
        p = filedialog.asksaveasfilename(defaultextension=".txt",
                                         initialfile=f"build_{datetime.now():%Y%m%d_%H%M%S}.txt")
        if p and self.log_text:
            with open(p, 'w') as f:
                f.write(self.log_text.get(1.0, END))
            self._log("Log saved", 'ok')

    def _start_build(self):
        sel = [p for p, v in self.platform_vars.items() if v.get()]
        if not sel:
            messagebox.showwarning("Warning", "Select at least 1 platform!")
            return
        godot = self.godot_path_var.get().strip()
        if not godot or not os.path.exists(godot):
            messagebox.showerror("Error", "Set valid Godot path first!")
            return

        self.is_building = True
        self.btn_start.config(state='disabled', text='>>> BUILDING... <<<')
        self.btn_stop.config(state='normal')
        self.progress_var.set(0)
        self._log("=" * 58, 'hdr')
        self._log(f"Building {len(sel)} platforms | Godot: {godot}", 'hdr')
        self.status.config(text="BUILDING...", fg='#FF9800')

        threading.Thread(target=self._run_build, args=(sel, godot), daemon=True).start()

    def _run_build(self, platforms, godot):
        total = len(platforms)
        ok_count = 0
        t0 = datetime.now()

        if self.clean_build.get():
            self._exec(['python3', 'build.py', '--clean'])

        for i, plat in enumerate(platforms, 1):
            if not self.is_building:
                break
            name = dict(self.platforms)[plat]
            pct = (i - 1) / total * 100
            self.root.after(0, lambda p=pct: self.progress_var.set(p))
            self._log(f"\n[{i}/{total}] {name} ...", 'info')

            cmd = ['python3', 'build.py', '--platform', plat]
            if self.debug_mode.get():
                cmd.append('--debug')
            cmd += ['--godot-path', godot]

            if self._exec(cmd):
                ok_count += 1
                self._log("   OK!", 'ok')
            else:
                self._log("   FAILED!", 'err')

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
        self._log(f"\n{'='*58}", 'hdr')
        self._log(f"DONE! {ok_count}/{total} | Time: {m}m{s}s", 'hdr')

        if ok_count == total:
            self.status.config(text=f"SUCCESS! ({m}m{s}s)", fg='#4CAF50')
            messagebox.showinfo("Done", f"All {total} platforms built OK!\nTime: {m}m{s}s")
        else:
            self.status.config(text=f"PARTIAL: {ok_count}/{total}", fg='#FF9800')
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
                        self._log(f"   {ln}", 'err')
            return r.returncode == 0
        except Exception as e:
            self._log(f"   Error: {e}", 'err')
            return False

    def _stop_build(self):
        if messagebox.askyesno("Confirm", "Stop building?"):
            self.is_building = False
            self._log("[STOPPED by user]", 'warn')
            self.status.config(text="STOPPED", fg='#F44336')

    def _clean_dir(self):
        if messagebox.askyesno("Confirm", f"Delete?\n{self.build_dir}"):
            if self.build_dir.exists():
                shutil.rmtree(self.build_dir)
                self._log("Cleaned", 'ok')
            else:
                self._log("Dir not exist", 'info')

    def _open_output(self):
        self.build_dir.mkdir(parents=True, exist_ok=True)
        import platform as pf
        if pf.system() == 'Darwin':
            subprocess.run(['open', str(self.build_dir)])
        elif pf.system() == 'Windows':
            subprocess.run(['explorer', str(self.build_dir)])

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
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
