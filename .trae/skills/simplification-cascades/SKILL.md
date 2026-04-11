---
name: Simplification Cascades
description: When complexity spirals, simplify by removing special cases until the core pattern emerges
when_to_use: when the same thing is implemented 5+ ways, growing special cases, excessive if/else
version: 1.0.0
---

# Simplification Cascades

## Overview

When you see the same concept implemented multiple ways with growing special cases, it's a sign the core abstraction is wrong.

**Core principle:** Remove special cases until the universal pattern emerges.

## The Process

1. **List all variations** - Document every special case
2. **Find the common core** - What do they ALL share?
3. **Remove one special case** - Make it use the common core
4. **Verify nothing breaks** - Run tests
5. **Repeat** - Until all special cases are gone

## Remember
- Complexity is a symptom, not a feature
- Each special case is a missed abstraction
- Simplify one case at a time, verify each step
