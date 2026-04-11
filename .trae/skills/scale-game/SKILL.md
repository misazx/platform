---
name: Scale Game
description: Push requirements to extremes to reveal hidden assumptions and design limits
when_to_use: when unsure if design will work at production scale, edge cases unclear, unsure of limits
version: 1.0.0
---

# Scale Game

## Overview

Scale reveals what optimization hides. Push your design to extremes to find breaking points.

**Core principle:** If it breaks at 100x scale, it's a design problem, not a capacity problem.

## The Process

1. **State current assumptions** - What are the expected numbers?
2. **Multiply by 10** - What breaks first?
3. **Multiply by 100** - What breaks next?
4. **Multiply by 1000** - What's fundamentally wrong?
5. **Design for the extreme** - Then scale back to reality
