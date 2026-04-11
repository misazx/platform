---
name: Root Cause Tracing
description: Systematically trace bugs backward through call stack to find original trigger
when_to_use: when errors occur deep in execution and you need to trace back to find the original trigger
version: 1.1.0
languages: all
---

# Root Cause Tracing

## Overview

Bugs often manifest deep in the call stack. Your instinct is to fix where the error appears, but that's treating a symptom.

**Core principle:** Trace backward through the call chain until you find the original trigger, then fix at the source.

## The Tracing Process

1. **Observe the Symptom**
2. **Find Immediate Cause** - What code directly causes this?
3. **Ask: What Called This?** - Trace the call chain
4. **Keep Tracing Up** - What value was passed?
5. **Find Original Trigger** - Where did the bad value originate?

## Adding Stack Traces

When you can't trace manually, add instrumentation with `console.error()` and `new Error().stack`.

## Key Principle

**NEVER fix just where the error appears.** Trace back to find the original trigger.

Fix at source, then add defense-in-depth validation at each layer.
