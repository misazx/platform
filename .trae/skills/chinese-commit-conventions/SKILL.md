---
name: chinese-commit-conventions
description: 中文 Git 提交规范 — 适配国内团队的 commit message 规范和 changelog 自动化
when_to_use: 当需要编写符合国内团队规范的 Git commit message 时
---

# 中文 Git 提交规范

## Conventional Commits 中文适配

### 格式

```
<type>(<scope>): <中文简要描述>

<中文详细说明（可选）>

<关联信息（可选）>
```

### 类型对照表

| 类型 | 含义 | 示例 |
|------|------|------|
| feat | 新功能 | feat(用户): 新增手机号登录功能 |
| fix | 修复 Bug | fix(支付): 修复微信支付回调重复处理的问题 |
| docs | 文档变更 | docs: 更新 API 接口文档 |
| style | 代码格式 | style: 统一缩进为 2 个空格 |
| refactor | 重构 | refactor(订单): 拆分订单服务，提取公共逻辑 |
| perf | 性能优化 | perf(列表): 虚拟滚动优化长列表渲染性能 |
| test | 测试 | test(auth): 补充登录模块单元测试 |
| chore | 构建/工具 | chore: 升级 Node.js 至 v20 |

### 原则

- type 保留英文关键字（工具链兼容性好）
- scope 和 description 使用中文
- subject 不超过 50 个字符，使用动宾短语
- 不加句号结尾

### Breaking Changes 标注

```
feat(接口)!: 重构用户信息返回结构

BREAKING CHANGE: /api/user/info 返回结构变更
- avatar 字段移入 profile 对象
- 移除已废弃的 nickname 字段
```
