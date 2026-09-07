# Reference Sources

This file tracks the read-only reference repositories available beside VictusControl. Keep it short, factual, and commit-SHA based so future AI sessions can compare behavior without pulling whole repositories into context.

## Policy

- Reference repositories live outside the VictusControl product repository.
- Do not stage, commit, or copy reference repositories into VictusControl.
- Do not modify reference repositories from VictusControl tasks.
- Inspect references only for a specific question, subsystem, or comparison.
- Prefer behavioral reimplementation in original VictusControl code over copying source.
- Check each upstream license before reusing any code, text, assets, or build logic.
- Avoid proprietary HP DLLs, binaries, drivers, installer payloads, and decompiled assets.

## Sources

| Name | GitHub URL | Local path | Branch | Current commit SHA | Working tree status | Intended use in VictusControl | Reuse policy |
| --- | --- | --- | --- | --- | --- | --- | --- |
| seerge/g-helper | https://github.com/seerge/g-helper.git | `D:\Projects\Workspace\references\g-helper` | `main` | `5c26f5ac970dab9e26347d80976ebf1eece91b1e` | Clean | Reference for lightweight Windows utility patterns, app lifecycle ideas, settings, update flow, and general ergonomics. | Use as behavioral inspiration only. Do not copy source wholesale. Verify license before any narrow reuse. |
| ib-3/ghelper-omen | https://github.com/ib-3/ghelper-omen.git | `D:\Projects\Workspace\references\ghelper-omen` | `main` | `1694844d2725e79a2b2065a0a1494fa1d143e3f4` | Clean | Reference for HP/Omen-oriented adaptation ideas and comparison against G-Helper-derived patterns. | Use for comparison and behavior notes only. Avoid importing adapted source directly. |
| theantipopau/omencore | https://github.com/theantipopau/omencore.git | `D:\Projects\Workspace\references\omencore` | `main` | `b39b44978902606aa708cc0d78bcfd87e95fd88b` | Clean | Reference for HP/Omen hardware concepts, device interaction boundaries, and possible domain vocabulary. | Reimplement behavior independently. Do not copy hardware access code without explicit license review. |
| breadeding/OmenSuperHub | https://github.com/breadeding/OmenSuperHub.git | `D:\Projects\Workspace\references\OmenSuperHub` | `master` | `a6ab6988c446ee5421466097fdf60c0d521e5c81` | Clean | Reference for alternative Omen control approaches and feature discovery. | Treat as research material only. No wholesale reuse or vendor asset reuse. |
| MasonDye/OmenXHub | https://github.com/MasonDye/OmenXHub.git | `D:\Projects\Workspace\references\OmenXHub` | `main` | `ca84cb011d1d3e5850445d19e45fbea06e83a8fd` | Clean | Reference for additional Omen/Victus-adjacent behavior comparison. HEAD was repaired and is valid. | Use only for targeted comparisons. Do not copy implementation unless explicitly approved after license review. |
| affaan-m/ECC | https://github.com/affaan-m/ECC.git | `D:\Projects\Workspace\references\ECC` | `main` | `5eddf1a3ffd311423be2d4ba7d26f7209c91b033` | Clean | Reference for embedded controller concepts and terminology when relevant to safety discussions. | High caution. Do not copy low-level control logic without hardware safety review and license review. |

## Update Rules

- Refresh this file after cloning, repairing, pulling, or otherwise changing reference repository state.
- Record exact commit SHAs before using reference behavior to guide a VictusControl design or implementation decision.
- Keep detailed analysis out of this file; store only source tracking facts here.
