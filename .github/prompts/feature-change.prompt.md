# Feature Change Prompt

Implement only the requested feature. Avoid speculative scaffolding.

Model guidance:

- Use GPT-5.5 Low for small localized features.
- Use GPT-5.5 Medium for cross-module features.
- Use GPT-5.6 High for hardware-facing or safety-sensitive features.

Workflow:

1. Read `AGENTS.md`, `AI_CONTEXT.md`, `SESSION_STATE.md`, `TOKEN_STRATEGY.md`, and the matching recipe in `CONTEXT_RECIPES.md`.
2. Search for existing abstractions and similar behavior.
3. Select exact files before reading broadly.
4. Use Repomix only if more than four selected files must be shared as a pack.
5. Do not include reference repositories unless explicitly required.
6. Make a minimal patch.
7. Update focused tests for changed behavior.
8. Review `git diff`.
9. Run focused verification.
10. Update `SESSION_STATE.md` if the work meaningfully changes project state.
