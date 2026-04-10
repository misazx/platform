#!/usr/bin/env python3
"""诊断脚本 - 测试 tkinter 各组件是否可见"""
from tkinter import *

root = Tk()
root.title("tkinter Diagnostics")
root.geometry("800x500")
root.configure(bg='white')

# Test 1: Label
Label(root, text="TEST 1: Label - if you see this, Label works",
      font=('', 14), bg='green', fg='white', pady=10).pack(fill=X)

# Test 2: Entry
Label(root, text="TEST 2: Entry below:", font=('', 12)).pack(anchor='w', padx=10)
e = Entry(root, font=('', 14), bg='yellow', fg='black', width=40)
e.pack(fill=X, padx=10, pady=5)
e.insert(0, "If you see this text, Entry works")

# Test 3: Label with textvariable
Label(root, text="TEST 3: Label with textvariable below:", font=('', 12)).pack(anchor='w', padx=10)
sv = StringVar(value="If you see this, Label+textvariable works")
Label(root, textvariable=sv, font=('Courier', 14), bg='cyan', fg='black',
      relief='groove', bd=2, height=2, anchor='w', padx=8).pack(fill=X, padx=10, pady=5)

# Test 4: Text widget
Label(root, text="TEST 4: Text widget below:", font=('', 12)).pack(anchor='w', padx=10)
t = Text(root, font=('Courier', 12), bg='lightyellow', fg='black', height=5)
t.pack(fill=X, padx=10, pady=5)
t.insert(END, "If you see this, Text widget works\nLine 2\nLine 3")

# Test 5: Button
Label(root, text="TEST 5: Button below:", font=('', 12)).pack(anchor='w', padx=10)
Button(root, text="Click me - Button works!", font=('', 14, 'bold'),
       bg='orange', fg='white', command=lambda: sv.set("BUTTON CLICKED!")).pack(fill=X, padx=10, pady=5)

# Test 6: Python version
import sys
Label(root, text=f"Python: {sys.version} | Tk: {root.tk.call('info', 'patchlevel')}",
      font=('', 10), bg='gray', fg='white', pady=5).pack(fill=X, side=BOTTOM)

root.mainloop()
