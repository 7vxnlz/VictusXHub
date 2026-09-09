# Bug Investigation Prompt

Follow injected repository instructions and the bug route in `.github/ai/CONTEXT_RECIPES.md`.

Start from the exact error, failing test, log excerpt, or diff. Search its exact text/symbols and inspect only the likely implementation, caller, and focused tests. Explain the cause with file references. If a fix is authorized, make the smallest patch and run focused verification; retain actionable failure diagnostics. Use reference repositories only when the bug explicitly requires comparison.
