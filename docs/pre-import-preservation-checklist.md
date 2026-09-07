# Pre-Import Preservation Checklist

## Preserve Before G-Helper Import

- AI workflow files: `AGENTS.md`, `AI_CONTEXT.md`, `SESSION_STATE.md`, `TOKEN_STRATEGY.md`, `CONTEXT_RECIPES.md`, `REFERENCE_POLICY.md`, `REFERENCE_SOURCES.md`, `.repomixignore`, `repomix.config.json`
- GitHub AI guidance: `.github/copilot-instructions.md`, `.github/prompts/`
- Planning docs: `docs/`
- Current skeleton record: `VictusControl.sln`, `src/`, `tests/` should be archived or explicitly listed before removal

## Can Be Removed In Skeleton-Removal Step

- `VictusControl.sln`
- `src/`
- `tests/`

## Must Not Be Overwritten By G-Helper Import

- `AGENTS.md`
- `AI_CONTEXT.md`
- `SESSION_STATE.md`
- `TOKEN_STRATEGY.md`
- `CONTEXT_RECIPES.md`
- `REFERENCE_POLICY.md`
- `REFERENCE_SOURCES.md`
- `.repomixignore`
- `repomix.config.json`
- `.github/copilot-instructions.md`
- `.github/prompts/`
- `docs/`

## Next Action

Create an explicit skeleton archive-or-delete task; do not copy G-Helper files yet.
