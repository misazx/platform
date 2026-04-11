---
name: Systematic Debugging
description: Four-phase debugging framework that ensures root cause investigation before attempting fixes. Never jump to solutions.
when_to_use: when encountering any bug, test failure, or unexpected behavior, before proposing fixes
version: 2.1.0
languages: all
---

# Systematic Debugging

## Overview

Random fixes waste time and create new bugs. Quick patches mask underlying issues.

**Core principle:** ALWAYS find root cause before attempting fixes. Symptom fixes are failure.

**Violating the letter of this process is violating the spirit of debugging.**

## The Iron Law

```
NO FIXES WITHOUT ROOT CAUSE INVESTIGATION FIRST
```

If you haven't completed Phase 1, you cannot propose fixes.

## The Four Phases

You MUST complete each phase before proceeding to the next.

### Phase 1: Root Cause Investigation

1. **Read Error Messages Carefully** - Don't skip past errors or warnings
2. **Reproduce Consistently** - Can you trigger it reliably?
3. **Check Recent Changes** - Git diff, recent commits
4. **Gather Evidence in Multi-Component Systems** - Add diagnostic instrumentation at each component boundary
5. **Trace Data Flow** - See skills/root-cause-tracing for backward tracing technique

### Phase 2: Pattern Analysis

1. **Find Working Examples** - Locate similar working code in same codebase
2. **Compare Against References** - Read reference implementation COMPLETELY
3. **Identify Differences** - List every difference
4. **Understand Dependencies** - What other components does this need?

### Phase 3: Hypothesis and Testing

1. **Form Single Hypothesis** - "I think X is the root cause because Y"
2. **Test Minimally** - SMALLEST possible change, one variable at a time
3. **Verify Before Continuing** - Did it work? Yes → Phase 4, No → New hypothesis
4. **When You Don't Know** - Say "I don't understand X", don't pretend

### Phase 4: Implementation

1. **Create Failing Test Case** - Simplest possible reproduction
2. **Implement Single Fix** - ONE change at a time
3. **Verify Fix** - Test passes? No other tests broken?
4. **If Fix Doesn't Work** - If < 3 attempts: Return to Phase 1. If ≥ 3: Question the architecture

## Red Flags - STOP and Follow Process

- "Quick fix for now, investigate later"
- "Just try changing X and see if it works"
- "Add multiple changes, run tests"
- "Skip the test, I'll manually verify"
- **"One more fix attempt" (when already tried 2+)**

**ALL of these mean: STOP. Return to Phase 1.**

## Quick Reference

| Phase | Key Activities | Success Criteria |
|-------|---------------|------------------|
| **1. Root Cause** | Read errors, reproduce, check changes, gather evidence | Understand WHAT and WHY |
| **2. Pattern** | Find working examples, compare | Identify differences |
| **3. Hypothesis** | Form theory, test minimally | Confirmed or new hypothesis |
| **4. Implementation** | Create test, fix, verify | Bug resolved, tests pass |
