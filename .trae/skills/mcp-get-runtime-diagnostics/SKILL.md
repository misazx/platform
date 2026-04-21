---
name: mcp:get-runtime-diagnostics
description: 运行时诊断 - 获取并分析 Godot 控制台当前的运行时错误。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

运行时诊断 - 获取并分析 Godot 控制台当前的运行时错误。

**Input**: 无参数，或可选的 --clear 参数

**Steps**

1. 调用 get_runtime_diagnostics 工具

   使用 mcp__tranycode-core__get_runtime_diagnostics 工具获取诊断信息。

2. 展示诊断结果

   展示运行时错误和分析结果。
