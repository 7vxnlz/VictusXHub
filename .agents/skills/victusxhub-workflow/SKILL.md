---
name: victusxhub-workflow
description: Route substantial VictusXHub implementation, HP read-only hardware, OEM/reference investigation, and verification work in this repository.
---

# VictusXHub Workflow

For substantial repository work, follow `AGENTS.md`, scoped instructions, and `.github/ai/AI_WORKING_STATE.md`.

Load one initial mode only:

- Implementation, UI, tooling, or bug fixes: [implementation](references/implementation.md)
- HP firmware, WMI, PawnIO, or read-only hardware: [HP read-only](references/hp-readonly.md)
- OEM/reference/protocol research: [reverse engineering](references/reverse-engineering.md)
- Verification or review: [verification](references/verification.md)

Search precisely and read selected files, not broad trees. Prefer `tools/context/*-compact.ps1`; load more context only when evidence requires it. Correctness and HP safety override token savings.
