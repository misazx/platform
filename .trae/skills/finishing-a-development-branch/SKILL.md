---
name: Finishing a Development Branch
description: Complete feature development with structured options for merge, PR, or cleanup
when_to_use: when implementation is complete, all tests pass, and you need to decide how to integrate the work
version: 1.1.0
---

# Finishing a Development Branch

## Overview

Guide completion of development work by presenting clear options and handling chosen workflow.

**Core principle:** Verify tests → Present options → Execute choice → Clean up.

## The Process

### Step 1: Verify Tests
Run project's test suite. If tests fail, stop. Don't proceed.

### Step 2: Determine Base Branch

### Step 3: Present Options
```
1. Merge back to <base-branch> locally
2. Push and create a Pull Request
3. Keep the branch as-is (I'll handle it later)
4. Discard this work
```

### Step 4: Execute Choice

### Step 5: Cleanup Worktree (for Options 1, 2, 4)

## Red Flags
**Never:**
- Proceed with failing tests
- Merge without verifying tests on result
- Delete work without confirmation
- Force-push without explicit request
