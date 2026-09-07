# G-Helper Base Decision

VictusControl will use G-Helper as the direct base if G-Helper source is imported.

VictusControl will proceed as GPL-3.0-compatible for any direct G-Helper source import. Attribution, license notices, source availability, and modification history must be preserved.

Existing AI workflow files must be preserved, including `AGENTS.md`, `AI_CONTEXT.md`, `SESSION_STATE.md`, `TOKEN_STRATEGY.md`, `CONTEXT_RECIPES.md`, `REFERENCE_POLICY.md`, `REFERENCE_SOURCES.md`, `.repomixignore`, `repomix.config.json`, `.github/copilot-instructions.md`, `.github/prompts/`, and current planning docs.

The current clean `src/` and `tests/` skeleton must not be mixed with G-Helper code. It should be replaced or archived only in a separate explicit step.

The next step is a tiny import-preparation step, not a source import.
