---
description: How to save AI development session artifacts to project work history
---

# Save Work History Workflow

Save a summary of the current AI development session to the project's work history for future reference.

## Step 1 — Identify the Scope

Determine which project(s) were modified during the session. Each project has its own `agent-work-history/` folder.

## Step 2 — Create the Session File

Create a new markdown file in the appropriate `agent-work-history/` folder:

- Filename format: `YYYY-MM-DD-short-description.md`
- If the root of the repo was the primary scope, use the root `agent-work-history/` folder
- If work was scoped to a specific project (e.g., `Scheduler/`), use that project's `agent-work-history/` folder

## Step 3 — Document the Session

Include the following sections in the file:

```markdown
# [Session Title]

**Date:** YYYY-MM-DD

## Summary

Brief description of what was accomplished.

## Changes Made

- List of files created or modified
- Brief description of each change

## Key Decisions

- Any architectural or design decisions made during the session

## Testing / Verification

- What was tested and how
- Results of verification
```
