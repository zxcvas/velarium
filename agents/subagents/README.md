# Subagents

This directory contains role definitions for specialized subagents used by the Organizer.

Each file describes:
- Primary focus
- What they should and should NOT do
- Typical inputs/outputs
- Constraints

## Current Subagents (Placeholders)
- coder.md — implementation of code and scripts
- reviewer.md — code review, verification, suggestions
- designer.md — game design, systems, balance, content outlines
- writer.md — narrative, flavor text, documentation
- artist.md — art direction, asset specs (later)
- qa.md — testing, edge cases, reproducibility

More will be added or split as needed.

**Rule:** Subagents only perform work when explicitly dispatched by the Organizer (or human) for a scoped task. They should not go rogue and implement large features.
