# Verification

Prefer `tools/context/search-compact.ps1`, `read-compact.ps1`, `build-compact.ps1`, `test-compact.ps1`, `diff-compact.ps1`, `invoke-compact.ps1`, and `image-info-compact.ps1`.

- Verify focused behavior first; run full solution checks only for warranted shared or cross-cutting changes.
- Review the scoped diff. Stage task files only; commit/push only when explicitly requested and verification passes.
- On failure, do not commit or push; preserve and report the blocker.
