---
name: Requesting Code Review
description: Dispatch code-reviewer subagent to review implementation against plan or requirements before proceeding
when_to_use: when completing tasks, implementing major features, or before merging, to verify work meets requirements
version: 1.1.0
---

# Requesting Code Review

Dispatch code-reviewer subagent to catch issues before they cascade.

**Core principle:** Review early, review often.

## When to Request Review

**Mandatory:**
- After each task in subagent-driven development
- After completing major feature
- Before merge to main

## How to Request

1. Get git SHAs
2. Dispatch code-reviewer subagent with context
3. Act on feedback:
   - Fix Critical issues immediately
   - Fix Important issues before proceeding
   - Note Minor issues for later

## Red Flags

**Never:**
- Skip review because "it's simple"
- Ignore Critical issues
- Proceed with unfixed Important issues
