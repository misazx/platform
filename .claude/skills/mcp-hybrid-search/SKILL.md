---
name: mcp:hybrid-search
description: 混合检索 - 结合向量语义与图谱结构，查找关联度最高的代码符号。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

混合检索 - 结合向量语义与图谱结构，查找关联度最高的代码符号。

**Input**: 搜索关键词或自然语言描述

**Steps**

1. 获取搜索查询

   从用户输入中提取搜索关键词。

2. 调用 hybrid_search 工具

   使用 mcp__tranycode-core__hybrid_search 工具进行搜索。

3. 展示搜索结果

   展示混合检索的结果。
