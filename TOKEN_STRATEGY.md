# Token Strategy

Optimize relevance, not raw smallness. The default startup context is:

1. `AGENTS.md`
2. `AI_WORKING_STATE.md`
3. nearest scoped `AGENTS.md`
4. one `docs/context-packs/*.md` file
5. exact implementation/tests found by `rg`

Do not load `SESSION_STATE.md`, all docs, a whole source directory, or reference repositories by default. Historical evidence remains searchable and should be opened only when its decision is active.

## Budgets

- Focused change: target 8k-20k tokens and no more than 12 files.
- Cross-cutting investigation: up to 35k tokens and 25 files with a stated reason.
- References: 1-5 exact files, after reading `REFERENCE_POLICY.md` and checking `docs/reference-index.md`.

## Tools

```powershell
tools/context/measure.ps1 -Path app/Hardware/Hp,tests/VictusX.Tests/Hardware/Hp -Top 20
tools/context/pack.ps1 -Pack telemetry -TokenBudget 20000 -OutputPath .tmp/telemetry-context.md
tools/context/pack.ps1 -Pack fan-research -UseRepomix -TokenBudget 20000 -OutputPath .tmp/fan-context.md
```

`measure.ps1` reports approximate tokens using characters/4; use it for relative weight, not billing. `pack.ps1` reads only the `## Files` list in a named pack, supports checked `path#Lstart-Lend` slices for monolithic files, rejects missing/out-of-repo files, and fails when selected content exceeds the budget. Repomix is opt-in, requires whole-file selectors and an installed executable, and its output is checked again and removed if over budget.

Prefer `git diff` for ongoing work and add individual files after symbol search. Never solve a missing-context problem by packing the whole repository.
