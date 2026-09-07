# Bug Investigation Prompt

Use the smallest context that can explain the bug.

Model guidance:

- Start with Terra Low for simple failures.
- Use GPT-5.5 Medium for multi-file defects.
- Use GPT-5.5 High for safety-sensitive or lifecycle bugs.

Workflow:

1. Read `AGENTS.md`, `AI_CONTEXT.md`, `SESSION_STATE.md`, and `TOKEN_STRATEGY.md`.
2. Ask for or inspect the exact error, failing test, log excerpt, or diff.
3. Search with exact symbols or error text.
4. Read only the likely implementation, caller, and test files.
5. Do not scan the whole repository.
6. Do not inspect reference repositories unless the bug is explicitly about reference comparison.
7. Explain the likely cause with file references.
8. If authorized to fix, make the smallest patch.
9. Verify with focused tests or the narrowest build available.
10. Summarize cause, changed files, and verification.
