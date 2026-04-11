---
name: Condition-Based Waiting
description: Replace arbitrary timeouts with condition polling for reliable async tests
when_to_use: when tests have race conditions, timing dependencies, or inconsistent pass/fail behavior
version: 1.1.0
languages: all
---

# Condition-Based Waiting

## Overview

Flaky tests often guess at timing with arbitrary delays.

**Core principle:** Wait for the actual condition you care about, not a guess about how long it takes.

## Core Pattern

```typescript
// ❌ BEFORE: Guessing at timing
await new Promise(r => setTimeout(r, 50));

// ✅ AFTER: Waiting for condition
await waitFor(() => getResult() !== undefined);
```

## Quick Patterns

| Scenario | Pattern |
|----------|---------|
| Wait for event | `waitFor(() => events.find(e => e.type === 'DONE'))` |
| Wait for state | `waitFor(() => machine.state === 'ready')` |
| Wait for count | `waitFor(() => items.length >= 5)` |
| Wait for file | `waitFor(() => fs.existsSync(path))` |

## Common Mistakes

**❌ Polling too fast:** Use 10ms intervals, not 1ms
**❌ No timeout:** Always include timeout with clear error
**❌ Stale data:** Call getter inside loop for fresh data
