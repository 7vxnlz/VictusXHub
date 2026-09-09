# Skeleton Replacement Plan

## Clean Skeleton Files

The current clean skeleton is `VictusControl.sln`, `src/`, and `tests/`.

## Workflow And Docs To Preserve

Preserve AI workflow and planning files: `AGENTS.md`, `.github/ai/AI_CONTEXT.md`, `.github/ai/SESSION_STATE.md`, `.github/ai/TOKEN_STRATEGY.md`, `.github/ai/CONTEXT_RECIPES.md`, `docs/development/REFERENCE_POLICY.md`, `docs/development/REFERENCE_SOURCES.md`, `.repomixignore`, `repomix.config.json`, `.github/copilot-instructions.md`, `.github/prompts/`, and `docs/`.

## Delete Or Archive

Do not mix the current skeleton with G-Helper source. Prefer archiving the skeleton first so the existing contracts/tests remain recoverable during the import experiment. Delete it only if the user explicitly chooses a clean replacement.

## Exact Next Small Step

Create a tiny archive/delete checklist that lists the exact current skeleton paths and the exact preserved workflow/docs paths. Do not copy G-Helper files yet.
