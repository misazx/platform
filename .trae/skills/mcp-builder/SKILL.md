---
name: mcp-builder
description: MCP 服务器构建方法论 — 系统化构建生产级 MCP 工具，让 AI 助手连接外部能力
when_to_use: 当需要构建 MCP 服务器，为 AI 助手提供工具、资源或提示词模板时
---

# MCP 服务器构建

## 协议核心概念

MCP 定义三种原语：

- **Tools（工具）**：AI 助手主动调用的函数，有副作用
- **Resources（资源）**：AI 助手只读访问的数据源，用 URI 标识
- **Prompts（提示词模板）**：预定义交互模板

**选择原则：** 执行操作 → Tool | 读取数据 → Resource | 引导交互 → Prompt

## Tool 设计原则

### 命名
- `snake_case` 格式，动词开头：`search_users`、`create_issue`
- 名称自解释，AI 助手靠名称选工具

### 参数
- 每个参数有类型约束和描述
- 可选参数给默认值，减少 AI 决策负担
- 用枚举代替布尔开关

### 描述
说明**用途 + 返回内容 + 限制**，这是 AI 选择工具的关键依据

### 输出
- 结构化数据 → JSON，人类可读内容 → Markdown
- 始终用 `content: [{ type: "text", text: "..." }]` 格式返回

## 错误处理四原则

1. 永远不让服务器崩溃 — try/catch 包裹所有外部调用
2. 返回可操作的错误信息 — 告诉 AI 问题是什么、能做什么
3. 使用 `isError: true` — 让 AI 知道调用失败
4. 区分错误类型 — 参数错误、权限不足、资源不存在、服务不可用

## 安全考虑

- 最小权限原则，读写 Tool 分离
- SQL 注入 → 参数化查询
- 路径遍历 → 校验路径，禁止 `../`
- 命令注入 → 用 `execFile` 而非 `exec`
- 密钥通过环境变量传入，不硬编码

## 调试技巧

**关键：MCP 用 stdio 通信，不能用 `console.log`，会破坏协议流。**

```typescript
// 错误
console.log("debug");
// 正确
console.error("[DEBUG]", info);
// 更好
server.sendLoggingMessage({ level: "info", data: "处理中" });
```
