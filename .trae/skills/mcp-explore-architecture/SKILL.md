---
name: mcp:explore-architecture
description: 架构探索 - 深度架构探索工具，生成 Mermaid 类图、审计组件复杂度。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

架构探索 - 深度架构探索工具，生成 Mermaid 类图、审计组件复杂度。

**Input**: 模块名、类名或文件名

**Steps**

1. 获取探索主题

   从用户输入中提取要探索的主题。

2. 调用 explore_architecture 工具

   使用 mcp__tranycode-core__explore_architecture 工具进行架构探索。

3. 展示架构分析结果

   展示架构图和分析结果。
