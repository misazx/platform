---
name: Subagent-Driven Development
description: Execute implementation plan by dispatching fresh subagent for each task, with code review between tasks
when_to_use: when executing implementation plans with independent tasks in the current session, using fresh subagents with review gates
version: 1.1.0
---

# Subagent-Driven Development

Execute plan by dispatching fresh subagent per task, with code review after each.

**Core principle:** Fresh subagent per task + review between tasks = high quality, fast iteration

## The Process

### 1. Load Plan
Read plan file, create TodoWrite with all tasks.

### 2. Execute Task with Subagent
For each task, dispatch fresh subagent with:
- Specific scope: One task
- Clear goal: Implement exactly what the task specifies
- Constraints: Don't change other code

### 3. Review Subagent's Work
Dispatch code-reviewer subagent to check:
- Strengths, Issues (Critical/Important/Minor), Assessment

### 4. Apply Review Feedback
- Fix Critical issues immediately
- Fix Important issues before next task
- Note Minor issues

### 5. Mark Complete, Next Task

### 6. Final Review
After all tasks complete, dispatch final code-reviewer for overall review.

### 7. Complete Development
Switch to skills/finishing-a-development-branch

## Red Flags
**Never:**
- Skip code review between tasks
- Proceed with unfixed Critical issues
- Dispatch multiple implementation subagents in parallel (conflicts)
- Implement without reading plan task
