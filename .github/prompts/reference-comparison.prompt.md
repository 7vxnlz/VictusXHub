# Reference Comparison Prompt

Compare behavior without copying code.

Model guidance:

- Use GPT-5.5 Medium for simple comparisons.
- Use GPT-5.5 High for architectural or licensing-sensitive comparisons.
- Use GPT-5.6 High for hardware behavior that may affect safety.

Workflow:

1. Read `AGENTS.md`, `REFERENCE_POLICY.md`, and `TOKEN_STRATEGY.md`.
2. Define the exact behavior being compared.
3. Search VictusControl first.
4. Search only the relevant reference repository paths.
5. Read 1-5 reference files maximum unless justified.
6. Record the reference repository name and upstream commit SHA.
7. Summarize findings in original words.
8. Identify reusable ideas, not copied code.
9. State licensing concerns before recommending reuse.
10. Do not modify files unless the user separately authorizes implementation.
