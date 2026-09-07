# Skeleton Removal Checklist

## Remove In Next Step

- `VictusControl.sln`
- `src/`
- `tests/`

## Preserve

- `AGENTS.md`
- `AI_CONTEXT.md`
- `SESSION_STATE.md`
- `TOKEN_STRATEGY.md`
- `CONTEXT_RECIPES.md`
- `REFERENCE_POLICY.md`
- `REFERENCE_SOURCES.md`
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
