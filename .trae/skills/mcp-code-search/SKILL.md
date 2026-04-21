---
name: mcp:code-search
description: 代码搜索 - 上下文代码搜索，不仅定位代码位置，还提供智能代码切片与关联调用链。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

代码搜索 - 上下文代码搜索。

使用方式：直接在对话中描述你想搜索的内容，或者提供搜索关键词或自然语言描述。

**Input**: 搜索关键词或自然语言描述

**Steps**

1. 获取搜索查询

   从用户输入中提取搜索关键词。

2. 调用 code_search 工具

   使用 mcp__tranycode-core__code_search 工具进行搜索。

3. 展示搜索结果

   以清晰的格式展示搜索结果。
