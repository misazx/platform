---
name: mcp:init-task
description: 初始化任务 - 初始化一个新开发任务并制定步骤计划。
license: MIT
metadata:
  author: trae-game
  version: "1.0"
---

初始化任务 - 初始化一个新开发任务并制定步骤计划。

**Input**: 任务目标和计划步骤

**Steps**

1. 如果没有提供目标和计划，询问用户

   使用 AskUserQuestion 询问任务目标和计划。

2. 调用 init_task 工具

   使用 mcp__tranycode-core__init_task 工具初始化任务。

3. 展示任务状态

   展示已创建的任务信息。
