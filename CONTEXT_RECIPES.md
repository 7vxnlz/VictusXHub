# Context Recipes

Use this default flow:

`injected/repository instructions -> AI_WORKING_STATE.md -> exact rg search -> one domain route -> selected implementation + focused tests -> git diff -> focused verification -> broad verification once when warranted -> state delta -> commit/push only when explicitly requested`

Do not reread root `AGENTS.md` when it is already present in session instructions. Always read the nearest scoped `AGENTS.md` for the selected domain. Do not enumerate or load installed skill documentation speculatively; use a skill only when the task triggers it. A context-pack definition is a routing aid, not an instruction to materialize or read every listed file. Historical documents are demand-loaded when their decision or evidence is active.

| Task | Pack | Search next | Usually exclude |
| --- | --- | --- | --- |
| Fan safety/research | `fan-research` | exact command, gate, mapper, test | UI, packaging, unrelated decoders |
| Read-only telemetry | `telemetry` | source/provider/formatter symbol | write runners, reference repos |
| HP shell/Diagnostic UI | `ui` | control name and event handler | fan transports, packaging |
| Packaging/release | `packaging` | blocker or artifact type | application internals |
| Identity/branding | `identity` | visible string or resource path | hardware code |

For failures, start with failing output plus one implementation/test pair. Run focused tests first and full build/test once near completion when warranted. Do not repeat restore unless dependency/project state requires it. Summarize successful commands as concise PASS results; retain enough failure output to debug. For reference comparisons, follow `REFERENCE_POLICY.md` and `docs/reference-index.md` before opening 1-5 exact files. For documentation-only work, source is optional unless a claim requires verification.

## Session Lifecycle

Prefer one coherent task per session: focused work, verification, durable state update only, then commit/push when explicitly requested. Start a fresh session for the next unrelated task. Handoff only when continuing the same unfinished task and important transient findings are not yet represented in repository state; a completed committed task needs no handoff.
