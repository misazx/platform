---
name: mcp:get-file-omniscient-report
description: 文件全景报告 - 获取文件的全景上帝视角报告，集成符号定义、引用关系等。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

文件全景报告 - 获取文件的全景上帝视角报告，集成符号定义、引用关系等。

**Input**: 文件路径

**Steps**

1. 获取文件路径

   从用户输入中提取文件路径。

2. 调用 get_file_omniscient_report 工具

   使用 mcp__tranycode-core__get_file_omniscient_report 工具获取报告。

3. 展示全景报告

   展示文件的全景分析报告。
