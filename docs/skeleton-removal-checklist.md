# Skeleton Removal Checklist

## Remove In Next Step

- `VictusControl.sln`
- `src/`
- `tests/`

## Preserve

- `AGENTS.md`
- `.github/ai/AI_CONTEXT.md`
- `.github/ai/SESSION_STATE.md`
- `.github/ai/TOKEN_STRATEGY.md`
- `.github/ai/CONTEXT_RECIPES.md`
- `docs/development/REFERENCE_POLICY.md`
- `docs/development/REFERENCE_SOURCES.md`
- `.gitignore`
- `.gitattributes`
- `.repomixignore`
- `repomix.config.json`
- `.github/copilot-instructions.md`
- `.github/prompts/`
- `docs/`

## Expected Git Status After Removal

- deleted: `VictusControl.sln`
- deleted: all tracked files under `src/`
- deleted: all tracked files under `tests/`
- no G-Helper files added yet
- no preserved workflow/docs files deleted

## Future Removal Commit Message

```text
chore: remove clean skeleton before G-Helper import
```
