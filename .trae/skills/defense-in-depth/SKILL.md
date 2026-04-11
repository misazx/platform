---
name: Defense-in-Depth Validation
description: Validate at every layer data passes through to make bugs impossible
when_to_use: when invalid data causes failures deep in execution, requiring validation at multiple system layers
version: 1.1.0
languages: all
---

# Defense-in-Depth Validation

## Overview

Single validation: "We fixed the bug". Multiple layers: "We made the bug impossible".

**Core principle:** Validate at EVERY layer data passes through. Make the bug structurally impossible.

## The Four Layers

### Layer 1: Entry Point Validation
Reject obviously invalid input at API boundary.

### Layer 2: Business Logic Validation
Ensure data makes sense for this operation.

### Layer 3: Environment Guards
Prevent dangerous operations in specific contexts.

### Layer 4: Debug Instrumentation
Capture context for forensics.

## Applying the Pattern

1. Trace the data flow
2. Map all checkpoints
3. Add validation at each layer
4. Test each layer independently

**Don't stop at one validation point.** Add checks at every layer.
