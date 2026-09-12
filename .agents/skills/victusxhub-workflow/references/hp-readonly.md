# HP Read-only Work

Root `AGENTS.md`, `app/Hardware/Hp/AGENTS.md`, and the relevant context pack are authoritative.

- Establish exact-device evidence before behavior: capability is not current state; getter is not setter; raw is not interpreted.
- Ambiguous or unknown results remain unavailable. Do not guess nearby commands or fallback payloads.
- Hardware invocation needs explicit authorization. Establish command family, ABI, return semantics, provenance, and exact-device relevance before implementation.
