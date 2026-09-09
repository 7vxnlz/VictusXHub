# Reference Policy

Reference repositories are research material. They must not contaminate the VictusControl product repository or its default AI context.

## Expected References

- `seerge/g-helper`
- `ib-3/ghelper-omen`
- `theantipopau/omencore`
- `breadeding/OmenSuperHub`
- `MasonDye/OmenXHub`
- `affaan-m/ECC`

## Rules

- Do not copy reference repositories wholesale.
- Do not stage or commit reference repositories into VictusControl.
- Do not modify reference repositories from a VictusControl task.
- Inspect references only when the user asks or when a task cannot be answered from VictusControl.
- Keep references excluded from default AI context and Repomix packs.
- Record reviewed upstream commit SHAs when reference behavior affects a decision.
- Prefer behavioral reimplementation over copying code.
- Check licenses before copying any code.
- Avoid proprietary HP DLLs, vendor binaries, driver packages, or decompiled assets.

## Comparison Workflow

1. Define the exact behavior or subsystem being compared.
2. Search VictusControl first.
3. Search the reference repo second.
4. Read only matching files or symbols.
5. Record repo name and commit SHA.
6. Summarize behavior in original words.
7. Reimplement narrowly in VictusControl only after explicit implementation approval.

## Licensing Posture

Treat all reference code as copyrighted. MIT-licensed material may still require attribution. GPL-licensed material may impose reciprocal obligations if copied. Proprietary HP binaries should not be redistributed.
