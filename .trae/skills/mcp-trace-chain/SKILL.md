---
name: mcp:trace-chain
description: 调用链追踪 - 追踪实体的链式关系（调用链、继承链、依赖链）。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

调用链追踪 - 追踪实体的链式关系（调用链、继承链、依赖链）。

**Input**: 目标符号名称

**Steps**

1. 获取目标符号

   从用户输入中提取要追踪的符号名称。

2. 调用 trace_chain 工具

   使用 mcp__tranycode-core__trace_chain 工具进行追踪。

3. 展示调用链

   以清晰的格式展示调用链关系。
