# Token Strategy

Optimize relevance before breadth. Repository instructions already injected into the session should not be reread. Start with the compact `AI_WORKING_STATE.md`, then use exact `rg` searches, one domain route when useful, and only the implementation/tests needed by the task. The nearest scoped `AGENTS.md` remains mandatory for that domain.

Do not load `SESSION_STATE.md`, all documentation, a whole source directory, installed skill documentation speculatively, or reference repositories by default. Skills and historical evidence are demand-loaded only when the task triggers them.

## Targets

- Normal focused implementation: <= 12k approximate tokens and <= 8 selected files.
- Medium cross-cutting task: <= 20k approximate tokens and <= 12 selected files.
- Investigation: <= 30k approximate tokens and <= 20 selected files.
- Above 30k: justify expansion first and prefer splitting the task. These are targets, never correctness- or safety-breaking limits.
- References: 1-5 exact files, after reading `REFERENCE_POLICY.md` and checking `docs/reference-index.md`.

## Selection and Tools

Prefer exact symbol/file selection after `rg`, and use `git diff` for ongoing work. Read a pack definition as a route; do not materialize it merely because it exists. Use `pack.ps1` only for handoff, cross-file comparison, or budget enforcement. Never pack the whole repository.

```powershell
tools/context/measure.ps1 -Path app/Hardware/Hp,tests/VictusX.Tests/Hardware/Hp -Top 20
tools/context/pack.ps1 -Pack telemetry -TokenBudget 20000 -OutputPath .tmp/telemetry-context.md
```

`measure.ps1` uses characters/4 for relative context weight, not billing. `pack.ps1` accepts the named pack's checked whole-file and `#Lstart-Lend` selectors, rejects missing/out-of-repository files, requires generated files under `.tmp/`, and enforces its budget. Repomix is optional and whole-file-only.
