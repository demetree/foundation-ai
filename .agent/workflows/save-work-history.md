---
description: How to save AI development session artifacts to project work history
---

# Saving Work History

When completing a development session that involves significant changes (new features, refactors, bug fixes), the agent MUST archive the session's artifacts to the project that was modified.

## When to Save Work History

**ALWAYS save work history when:**
- A feature implementation is complete and verified
- A significant bug fix has been applied
- A refactoring effort has been completed
- Any work that would benefit from historical documentation

**Do NOT save work history for:**
- Simple questions/answers with no code changes
- Minor fixes (typos, small style changes under 10 lines)
- Incomplete or abandoned work
- Pure research/exploration sessions

## Automatic Session-End Behavior

At the end of every qualifying development session (before final notify_user), the agent should:

1. **Check if work history should be saved** - Did this session produce implementation_plan.md or walkthrough.md?
2. **If yes, automatically offer to save** - Include in the final message:
   > "I can archive this session's artifacts to the project's work history. Should I save to `<suggested-path>`?"
3. **Proceed with save unless user declines**

## Directory Structure

Work history should be saved to:
```
<ProjectRoot>/agent-work-history/<initials>-<month>-<day>-<year>-<brief-description>/
```

**Naming Convention:**
- `<initials>` - User's initials (ask if unknown, default to "ai")
- `<month>` - Lowercase three-letter month (jan, feb, mar, etc.)
- `<day>` - Day of month (1-31, no leading zero)
- `<year>` - Four digit year
- `<brief-description>` - 2-4 word kebab-case description of the work

**Examples:**
```
dk-jan-19-2026-roller-filter
ai-feb-3-2026-pdf-export-fix
dk-mar-15-2026-database-migration
```

## Files to Archive

Copy these files from the agent's artifact directory to the work history folder:

| File | Required | Description |
|------|----------|-------------|
| `implementation_plan.md` | Yes (if exists) | The approved implementation plan |
| `walkthrough.md` | Yes (if exists) | Completion walkthrough with verification |
| `task.md` | Optional | Task checklist (include if complex multi-step work) |

**Agent artifact directory location:**
```
C:\Users\demet\.gemini\antigravity\brain\<conversation-id>\
```

## Additional Context File

Create a `session-info.md` file in the work history folder with:

```markdown
# Session Information

- **Conversation ID:** <conversation-id>
- **Date:** <YYYY-MM-DD>
- **Time:** <HH:MM local time with timezone>
- **Duration:** <approximate session duration if known>

## Summary

<1-2 sentence summary of what was accomplished>

## Files Modified

<List of key files that were created/modified during this session>

## Related Sessions

<If this work continues from or relates to previous sessions, note them here>
```

## Complete Process

// turbo-all

1. Determine the target project folder (the main project that was modified)

2. Create the work history folder:
   ```powershell
   mkdir "<ProjectRoot>\agent-work-history\<folder-name>"
   ```

3. Copy implementation_plan.md (if exists):
   ```powershell
   Copy-Item "C:\Users\demet\.gemini\antigravity\brain\<conversation-id>\implementation_plan.md" "<target-folder>\"
   ```

4. Copy walkthrough.md (if exists):
   ```powershell
   Copy-Item "C:\Users\demet\.gemini\antigravity\brain\<conversation-id>\walkthrough.md" "<target-folder>\"
   ```

5. Copy task.md (optional, for complex work):
   ```powershell
   Copy-Item "C:\Users\demet\.gemini\antigravity\brain\<conversation-id>\task.md" "<target-folder>\"
   ```

6. Create session-info.md with the context information

7. Confirm to user:
   > "Session artifacts saved to `<target-folder>`"

## Example

After completing the "Roller Filter for Lot Heatmap" feature on Jan 19, 2026:

```
D:\repos\Compactica\Basecamp\Basecamp.Client\agent-work-history\dk-jan-19-2026-roller-filter\
├── implementation_plan.md
├── walkthrough.md
├── task.md
└── session-info.md
```
