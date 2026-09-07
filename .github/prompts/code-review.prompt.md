# Code Review Prompt

Review the diff, not the whole repository.

Model guidance:

- Use GPT-5.5 Low for small diffs.
- Use GPT-5.5 Medium for multi-file diffs.
- Use GPT-5.5 High for safety-sensitive, concurrency, persistence, or release changes.

Workflow:

1. Read `AGENTS.md`, `TOKEN_STRATEGY.md`, and relevant recipes.
2. Inspect `git diff` or the exact provided diff.
3. Read only files needed to understand risks.
4. Prioritize correctness, safety, tests, maintainability, and licensing issues.
5. Avoid broad repository scans.
6. Do not inspect reference repositories unless the diff changes reference policy or explicitly cites references.
7. Report findings first, ordered by severity.
8. Include file and line references where possible.
9. Mention test gaps and residual risk.
10. Keep summaries brief.
