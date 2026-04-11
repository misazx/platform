---
name: Executing Plans
description: Execute detailed plans in batches with review checkpoints
when_to_use: when partner provides a complete implementation plan to execute in controlled batches with review checkpoints
version: 2.2.0
---

# Executing Plans

## Overview

Load plan, review critically, execute tasks in batches, report for review between batches.

**Core principle:** Batch execution with checkpoints for architect review.

**Announce at start:** "I'm using the Executing Plans skill to implement this plan."

## The Process

### Step 1: Load and Review Plan
1. Read plan file
2. Review critically - identify any questions or concerns
3. If concerns: Raise them before starting
4. If no concerns: Create TodoWrite and proceed

### Step 2: Execute Batch
**Default: First 3 tasks**

For each task:
1. Mark as in_progress
2. Follow each step exactly
3. Run verifications as specified
4. Mark as completed

### Step 3: Report
When batch complete:
- Show what was implemented
- Show verification output
- Say: "Ready for feedback."

### Step 4: Continue
Based on feedback:
- Apply changes if needed
- Execute next batch
- Repeat until complete

### Step 5: Complete Development
- Switch to skills/finishing-a-development-branch
- Follow that skill to verify tests, present options, execute choice

## Remember
- Review plan critically first
- Follow plan steps exactly
- Don't skip verifications
- Between batches: just report and wait
- Stop when blocked, don't guess
