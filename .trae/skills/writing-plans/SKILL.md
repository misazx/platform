---
name: Writing Plans
description: Create detailed implementation plans with bite-sized tasks for engineers with zero codebase context
when_to_use: when design is complete and you need detailed implementation tasks for engineers with zero codebase context
version: 2.1.0
---

# Writing Plans

## Overview

Write comprehensive implementation plans assuming the engineer has zero context for our codebase and questionable taste.

**Announce at start:** "I'm using the Writing Plans skill to create the implementation plan."

**Save plans to:** `docs/plans/YYYY-MM-DD-<feature-name>.md`

## Bite-Sized Task Granularity

**Each step is one action (2-5 minutes):**
- "Write the failing test" - step
- "Run it to make sure it fails" - step
- "Implement the minimal code to make the test pass" - step
- "Run the tests and make sure they pass" - step
- "Commit" - step

## Task Structure

```markdown
### Task N: [Component Name]

**Files:**
- Create: `exact/path/to/file.py`
- Modify: `exact/path/to/existing.py:123-145`
- Test: `tests/exact/path/to/test.py`

**Step 1: Write the failing test**
**Step 2: Run test to verify it fails**
**Step 3: Write minimal implementation**
**Step 4: Run test to verify it passes**
**Step 5: Commit**
```

## Remember
- Exact file paths always
- Complete code in plan (not "add validation")
- Exact commands with expected output
- DRY, YAGNI, TDD, frequent commits

## Execution Handoff

After saving the plan, offer execution choice:

**1. Subagent-Driven (this session)** - I dispatch fresh subagent per task, review between tasks
**2. Parallel Session (separate)** - Open new session with executing-plans, batch execution
