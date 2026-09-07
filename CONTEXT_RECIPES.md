# Context Recipes

Every task starts with `AGENTS.md` and `AI_WORKING_STATE.md`. Then choose one route.

| Task | Pack | Search next | Usually exclude |
| --- | --- | --- | --- |
| Fan safety/research | `fan-research` | exact command, gate, mapper, test name | UI, packaging, unrelated decoders |
| Read-only telemetry | `telemetry` | source/provider/formatter symbol | write runners, reference repos |
| HP shell/Diagnostic UI | `ui` | control name and event handler | fan transports, packaging |
| Packaging/release evidence | `packaging` | blocker or artifact type | application internals |
| Icon/branding/metadata | `identity` | visible string or resource path | hardware code |

## Workflow

1. Read the selected pack definition; do not automatically read every listed file if search narrows it further.
2. Run `rg` for the exact symbol/behavior.
3. Inspect `git diff` when continuing uncommitted work.
4. Read the implementation and its focused tests.
5. Measure only when the selection is becoming large.
6. Materialize a pack only for handoff, cross-file comparison, or token-budget enforcement.

For reference comparisons, read `REFERENCE_POLICY.md`, use `docs/reference-index.md`, verify commits in `REFERENCE_SOURCES.md`, and open only the matching files. For failures, start with the failing output plus one implementation/test pair. For documentation-only work, read the target document and the current decision source; source code is optional unless the claim needs verification.
