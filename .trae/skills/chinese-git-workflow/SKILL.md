---
name: chinese-git-workflow
description: 适配国内 Git 平台和团队习惯的工作流规范——Gitee、Coding、极狐 GitLab 全覆盖
when_to_use: 当需要配置国内 Git 平台工作流，或适配国内团队 Git 协作习惯时
---

# 国内 Git 工作流规范

## 核心原则

工作流服务于团队效率，不是为了流程而流程。选适合团队规模的，别硬套大厂方案。

## 国内 Git 平台适配

| 特性 | Gitee | Coding.net | 极狐 GitLab | GitHub |
|------|-------|------------|-------------|--------|
| 国内访问 | 快 | 快 | 快 | 不稳定 |
| CI/CD | Gitee Go | Coding CI | 内置 GitLab CI | GitHub Actions |
| 适合场景 | 开源/小团队 | 中大型团队 | 企业私有化 | 国际项目 |

## 工作流选择

### 方案一：主干开发（2-8人小团队）
- 主干始终保持可发布状态
- 功能分支生命周期不超过 2 天
- 每天至少合并一次到主干

### 方案二：Git Flow（中大团队）
- main + develop + release/* + feat/* + hotfix/*
- 适合版本发布节奏固定的团队

### 方案三：国内团队常用简化流程
- main（受保护）+ dev（测试环境）+ feat/*（功能分支）
- 功能分支从 dev 拉出，合回 dev
- dev 测试通过后，合并到 main 发布

## 分支命名规范

```bash
feat/user-login              # 新功能
fix/payment-callback         # Bug 修复
feat/TAPD-1234-order-refund  # 关联任务编号
release/v2.1.0               # 版本发布
hotfix/v2.0.1                # 线上紧急修复
```

## 常用 Git 配置

```bash
# 解决中文文件名显示为转义字符的问题
git config --global core.quotepath false
# 设置默认分支名
git config --global init.defaultBranch main
```
