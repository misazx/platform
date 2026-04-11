---
name: Dispatching Parallel Agents
description: Use multiple Claude agents to investigate and fix independent problems concurrently
when_to_use: when facing 3+ independent failures that can be investigated without shared state or dependencies
version: 1.1.0
languages: all
---

# Dispatching Parallel Agents

## Overview

When you have multiple unrelated failures, investigating them sequentially wastes time.

**Core principle:** Dispatch one agent per independent problem domain. Let them work concurrently.

## When to Use

**Use when:**
- 3+ test files failing with different root causes
- Multiple subsystems broken independently
- Each problem can be understood without context from others

**Don't use when:**
- Failures are related (fix one might fix others)
- Need to understand full system state
- Agents would interfere with each other

## The Pattern

1. **Identify Independent Domains** - Group failures by what's broken
2. **Create Focused Agent Tasks** - Each agent gets specific scope, clear goal, constraints
3. **Dispatch in Parallel** - All agents run concurrently
4. **Review and Integrate** - Read summaries, verify no conflicts, run full test suite

## Agent Prompt Structure

Good agent prompts are:
1. **Focused** - One clear problem domain
2. **Self-contained** - All context needed
3. **Specific about output** - What should the agent return?
