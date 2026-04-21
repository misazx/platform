---
name: mcp
description: 通过斜杠命令调用 MCP 工具。使用 /mcp <tool-name> [args] 来调用 tranycode-core MCP 服务器的工具。
license: MIT
compatibility: Requires tranycode-core MCP server.
metadata:
  author: trae-game
  version: "1.0"
---

通过斜杠命令调用 MCP 工具。

可用命令：
- `/mcp list` - 列出所有可用的 MCP 工具
- `/mcp <tool-name>` - 调用指定的 MCP 工具
- `/mcp <tool-name> --arg1 value1 --arg2 value2` - 带参数调用工具

示例：
- `/mcp code_search --query "Player"`
- `/mcp get_live_scene_tree`
- `/mcp init_task --objective "修复bug" --plan "[\"第一步\", \"第二步\"]"`

**Input**: 斜杠命令后的参数，包括工具名和参数。

**Steps**

1. **解析命令参数**

   解析用户输入，提取：
   - 工具名称（第一个参数）
   - 参数键值对（--key value 格式）

2. **如果是 `list` 命令**

   显示所有可用的 MCP 工具列表，包括工具名称和简短描述。

3. **如果是工具调用**

   验证工具是否存在，然后使用对应的 MCP 工具调用。
   对于数组类型的参数，尝试解析 JSON 字符串。

4. **显示结果**

   展示工具调用的结果。

**可用工具**

tranycode-core 提供的工具：
- set_performance_mode - 设置系统运行模式
- init_task - 初始化开发任务
- update_task - 更新任务状态
- get_task_state - 查看任务进度
- check_backtrack - 检查回溯记忆
- record_summary - 记录项目摘要
- clear_session - 清理会话缓存
- manage_experiment - 实验管理
- workflow_analyze - 全能代码分析
- diagnose_code - 深度代码诊断
- explore_architecture - 架构探索
- search_implementation - 实现搜索
- code_search - 代码搜索
- get_runtime_diagnostics - 获取运行时错误
- trace_chain - 追踪调用链
- hybrid_search - 混合检索
- get_file_omniscient_report - 文件全景报告
- expand_panorama - 展开全景报告
- auto_sync_index - 同步向量索引
- get_live_scene_tree - 获取场景节点树
- run_acceptance_tests - 运行验收测试
- precheck_code_integrity - 代码完整性检查
- start_reasoning_viz - 启动推理可视化
- stop_reasoning_viz - 停止推理可视化
- get_reasoning_viz_status - 获取可视化状态
- emit_thinking_step - 发射思考步骤
- deep_search - 深度搜索
- graph_enhanced_query - 图谱增强查询
- explore_community - 社区探索
