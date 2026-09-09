# G-Helper Base Decision

VictusControl will use G-Helper as the direct base if G-Helper source is imported.

VictusControl will proceed as GPL-3.0-compatible for any direct G-Helper source import. Attribution, license notices, source availability, and modification history must be preserved.

Existing AI workflow files must be preserved, including `AGENTS.md`, `.github/ai/AI_CONTEXT.md`, `.github/ai/SESSION_STATE.md`, `.github/ai/TOKEN_STRATEGY.md`, `.github/ai/CONTEXT_RECIPES.md`, `docs/development/REFERENCE_POLICY.md`, `docs/development/REFERENCE_SOURCES.md`, `.repomixignore`, `repomix.config.json`, `.github/copilot-instructions.md`, `.github/prompts/`, and current planning docs.

The current clean `src/` and `tests/` skeleton must not be mixed with G-Helper code. It should be replaced or archived only in a separate explicit step.

The next step is a tiny import-preparation step, not a source import.
