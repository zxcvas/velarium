# Velarium — Development Cycle

This describes the intended repeatable workflow. We will refine it after using it on real work.

## High-Level Loop
1. **Organizer Review**
   - Read current task_board.md, recent changes, open questions.
   - Update priorities based on PROJECT_PLAN and player (human) direction.

2. **Task Dispatch**
   - Break work into small, well-scoped tasks.
   - Assign to appropriate subagents or request human input on decisions.
   - Record in production/task_board.md or issues.

3. **Execution**
   - Subagents implement in isolated steps (prefer small diffs).
   - They create or update code, docs, tests as appropriate.
   - They may call tools, write files, run builds/tests.

4. **Review & Integration**
   - Organizer (or reviewer subagent) inspects changes.
   - Run build + manual verification of the exe / systems.
   - Fix issues in follow-up tasks if needed.

5. **Commit**
   - Clear commit messages referencing the task.
   - Update task_board and any logs.
   - Push when appropriate.

6. **Human Checkpoint**
   - Report status.
   - Human decides next focus area.
   - We commit the skeleton first, then expand deliberately.

## Principles
- **No deep work until scaffolded.** This initial pass is deliberately shallow.
- Work in small, reviewable chunks.
- Prefer runnable state after every significant step.
- Agents should ask clarifying questions when requirements are ambiguous rather than guessing.
- Update docs (plan, schedule, board) as part of the work, not afterthought.

## Agent Roles Summary
- **Organizer**: Owns the cycle, maintains overview, dispatches, verifies.
- **Subagents**: Specialized roles (see agents/ folder). They do focused work when dispatched.

## Tooling Notes
- Use build.ps1 / run.ps1 for verification.
- Prefer editing existing files over creating many new ones early.
- Keep the exe building and runnable at all times.

## Cadence (to be tuned)
- Small focused sessions for single areas.
- After each area is "stood up", human + organizer decide the next priority.

This cycle will be stress-tested and improved starting with the first real task after the skeleton commit.
