---
name: Testing Anti-Patterns
description: Common testing mistakes that make tests unreliable, misleading, or useless
when_to_use: when writing tests and wanting to avoid common anti-patterns that make tests flaky or meaningless
version: 1.0.0
---

# Testing Anti-Patterns

## Top Anti-Patterns

### 1. Testing Mock Behavior Instead of Real Behavior
**Bad:** Test that mock was called with specific args
**Good:** Test that real code produces correct output

### 2. Testing Implementation Details
**Bad:** Test private methods, internal state
**Good:** Test public API, observable behavior

### 3. Overspecified Tests
**Bad:** Assert exact JSON structure with 50 fields
**Good:** Assert the fields that matter for this test

### 4. Test Interdependence
**Bad:** Test B depends on Test A running first
**Good:** Each test is independent, can run in any order

### 5. Mystery Guest
**Bad:** Test depends on data in database/filesystem
**Good:** Test creates its own data or uses fixtures

### 6. Hard-Coded Timeouts
**Bad:** `setTimeout(done, 5000)`
**Good:** Wait for actual condition (see condition-based-waiting)

## Remember
- Test behavior, not implementation
- Each test should be independent
- Avoid flaky tests at all costs
