---
name: mcp:deep-search
description: 深度搜索 - DeepSearcher + Graphify 融合深度搜索，多轮迭代推理。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

深度搜索 - DeepSearcher + Graphify 融合深度搜索，多轮迭代推理。

**Input**: 搜索查询（自然语言描述）

**Steps**

1. 获取搜索查询

   从用户输入中提取搜索查询。

2. 调用 deep_search 工具

   使用 mcp__tranycode-core__deep_search 工具进行深度搜索。

3. 展示搜索结果

   展示深度搜索的综合结果。
