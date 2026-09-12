# Token Strategy

Optimize relevance before breadth. Repository instructions already injected into the session should not be reread. Start with the compact `.github/ai/AI_WORKING_STATE.md`, then use exact `rg` searches, one domain route when useful, and only the implementation/tests needed by the task. The nearest scoped `AGENTS.md` remains mandatory for that domain.

Do not load `.github/ai/SESSION_STATE.md`, all documentation, a whole source directory, installed skill documentation speculatively, or reference repositories by default. Skills and historical evidence are demand-loaded only when the task triggers them.

For substantial VictusXHub tasks, use the repository-local `victusxhub-workflow` skill and initially load only the relevant reference.

## Targets

- Normal focused implementation: <= 12k approximate tokens and <= 8 selected files.
- Medium cross-cutting task: <= 20k approximate tokens and <= 12 selected files.
- Investigation: <= 30k approximate tokens and <= 20 selected files.
- Above 30k: justify expansion first and prefer splitting the task. These are targets, never correctness- or safety-breaking limits.
- References: 1-5 exact files, after reading `docs/development/REFERENCE_POLICY.md` and checking `docs/reference-index.md`.

## Selection and Tools

Prefer exact symbol/file selection after `rg`, and use `git diff` for ongoing work. Read a pack definition as a route; do not materialize it merely because it exists. Use `pack.ps1` only for handoff, cross-file comparison, or budget enforcement. Never pack the whole repository.

```powershell
tools/context/measure.ps1 -Path app/Hardware/Hp,tests/VictusXHub.Tests/Hardware/Hp -Top 20
tools/context/pack.ps1 -Pack telemetry -TokenBudget 20000 -OutputPath .tmp/telemetry-context.md
```

`measure.ps1` uses characters/4 for relative context weight, not billing. `pack.ps1` accepts the named pack's checked whole-file and `#Lstart-Lend` selectors, rejects missing/out-of-repository files, requires generated files under `.tmp/`, and enforces its budget. Repomix is optional and whole-file-only.

## Local session audit

`tools/context/audit-codex-session.ps1 -Latest` reports local aggregate session metadata only. Use `-Last 5` to compare recent sessions or `-Json` for structured statistics; `ApproxTokens` remains characters/4, not billing.

Use compact defaults for routine work, retaining raw commands for concrete debugging or omitted evidence:

```powershell
tools/context/search-compact.ps1 -Pattern 'HpDiagnostic' -Path app,tests
tools/context/read-compact.ps1 -Path app/Program.cs -StartLine 1 -EndLine 160
tools/context/build-compact.ps1
tools/context/test-compact.ps1 -Target tests/VictusXHub.Tests/VictusXHub.Tests.csproj -NoBuild
tools/context/diff-compact.ps1
tools/context/invoke-compact.ps1 -FilePath git -ArgumentList @('show', '--stat')
tools/context/image-info-compact.ps1 -Path app/Assets/VictusX.Source.png
```

`invoke-compact.ps1` accepts an executable plus argument array, not a command string; it records the full local log while returning only safe aggregate status.
