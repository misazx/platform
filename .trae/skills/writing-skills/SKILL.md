---
name: Writing Skills
description: TDD for process documentation - test with subagents before writing, iterate until bulletproof
when_to_use: when creating new skills, editing existing skills, or verifying skills work before deployment
version: 5.1.0
languages: all
---

# Writing Skills

## Overview

**Writing skills IS Test-Driven Development applied to process documentation.**

**Core principle:** If you didn't watch an agent fail without the skill, you don't know if the skill teaches the right thing.

## SKILL.md Structure

```markdown
---
name: Human-Readable Name
description: One-line summary
when_to_use: when [trigger/situation]
version: X.Y.Z
---

# Skill Name

## Overview
## When to Use
## Core Pattern
## Quick Reference
## Implementation
## Common Mistakes
```

## The Iron Law (Same as TDD)

```
NO SKILL WITHOUT A FAILING TEST FIRST
```

## Key Principles

- Rich `when_to_use` for discovery
- Keyword coverage for search
- Descriptive naming (verb-first)
- Token efficiency (frequently-loaded skills: <200 words)
- One excellent example beats many mediocre ones
- Close every loophole explicitly for discipline skills
