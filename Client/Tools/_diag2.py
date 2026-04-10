#!/usr/bin/env python3
"""精准诊断 - 找出哪个属性导致 Label 不可见"""
from tkinter import *

root = Tk()
root.title("Diag v2")
root.geometry("700x600")

# Test A: 最简 Label (无任何自定义)
Label(root, text="A: Plain Label no params").pack(pady=5)

# Test B: 只设 font
Label(root, text="B: font only", font=('Helvetica', 14)).pack(pady=5)

# Test C: 只设 bg+fg
Label(root, text="C: bg+fg only", bg='red', fg='white').pack(pady=5)

# Test D: font + bg + fg (组合)
Label(root, text="D: font+bg+fg", font=('Helvetica', 14), bg='blue', fg='white').pack(pady=5)

# Test E: 用系统默认字体名
Label(root, text="E: Helvetica font", font=('Helvetica', 16, 'bold'), bg='#333', fg='#0f0').pack(pady=5)

# Test F: Entry 最简
Entry(root).pack(fill=X, padx=10, pady=5)

# Test G: Text 最简
Text(root, height=3).pack(fill=X, padx=10, pady=5)

# Test H: Label with height
Label(root, text="H: Label with height=3", font=('Helvetica', 12), bg='purple', fg='white',
      height=3, relief=SOLID, bd=1).pack(fill=X, padx=10, pady=5)

# Info bar
import sys
Label(root, text=f"Python {sys.version.split()[0]} | Tk ver test",
      font=('Helvetica', 10)).pack(side=BOTTOM, fill=X, pady=10)

root.mainloop()
