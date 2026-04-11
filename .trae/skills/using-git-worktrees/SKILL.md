---
name: Using Git Worktrees
description: Create isolated git worktrees with smart directory selection and safety verification
when_to_use: when starting feature work that needs isolation from current workspace, before executing implementation plans
version: 1.1.0
---

# Using Git Worktrees

## Overview

Git worktrees create isolated workspaces sharing the same repository.

**Core principle:** Systematic directory selection + safety verification = reliable isolation.

## Directory Selection Process

1. Check existing directories: `.worktrees/` (preferred) or `worktrees/`
2. Check CLAUDE.md for preference
3. Ask user if neither exists

## Safety Verification

**MUST verify .gitignore before creating worktree** for project-local directories.

## Creation Steps

1. Detect project name
2. Create worktree with new branch: `git worktree add "$path" -b "$BRANCH_NAME"`
3. Run project setup (npm install, cargo build, etc.)
4. Verify clean baseline with tests
5. Report location

## Red Flags

**Never:**
- Create worktree without .gitignore verification (project-local)
- Skip baseline test verification
- Proceed with failing tests without asking
