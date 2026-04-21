---
name: mcp:diagnose-code
description: 代码诊断 - 深度代码诊断工具，强制触发调试流。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

代码诊断 - 深度代码诊断工具，强制触发调试流。

**Input**: 报错信息或问题现象描述

**Steps**

1. 获取问题描述

   从用户输入中提取问题描述。

2. 调用 diagnose_code 工具

   使用 mcp__tranycode-core__diagnose_code 工具进行诊断。

3. 展示诊断结果

   展示诊断结果和建议。
