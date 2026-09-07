# VictusX Icon Wiring Checkpoint

## Final Wiring

- `app/VictusX.csproj` uses `app/Assets/VictusX.ico` unconditionally as `ApplicationIcon`, fixing executable, window, taskbar, Explorer, and Alt-Tab identity to VictusX.
- All four VictusX icons are embedded as named managed resources for reliable tray loading.
- In HP mode, `Program.GetTrayIcon()` selects through `HpTrayIconSelector`. Current HP mode is unverified, so the base icon is used; the available routing for future verified state is:
  - Silent -> `VictusX.Silent.ico`
  - Balanced -> `VictusX.Balanced.ico`
  - Turbo/Performance -> `VictusX.Turbo.ico`
  - unknown or unsupported -> `VictusX.ico`
- `Settings.VisualiseIcon` refreshes only the HP tray icon from that selector. The fixed application icon never changes with mode.
- Outside HP mode, the inherited `Properties.Resources.standard` startup icon and GPU-mode tray swapping remain unchanged.

## Asset Validation

Validation completed on 2026-09-07. Each supplied file is a loadable Windows ICO container with seven 32-bit frames:

```text
256x256, 128x128, 64x64, 48x48, 32x32, 24x24, 16x16
```

| Asset | SHA-256 |
| --- | --- |
| `VictusX.ico` | `B3EF54509D0F636D763AA00B957D3A428557B3836D63F67029BDF9094329E9F6` |
| `VictusX.Silent.ico` | `406EE94792BF3BF9790454C655EA5A4965D06E7E75372883E1A03677105D093A` |
| `VictusX.Balanced.ico` | `D10DF15027780A8B88B2F633DFDE97C48DA319ED1CD749D8E94F9EDDD5388832` |
| `VictusX.Turbo.ico` | `C3A74F991351F01F55FD537387B4F58C32126CE2FD0AE7B0673F194856069DE5` |

The built executable exposes an associated application icon, and the built assembly contains all four expected `GHelper.Assets.VictusX*.ico` resources.

## Boundaries

The HP caller uses `HpPerformanceModeStatus.CurrentBaseMode` (unknown), not saved `Modes.GetCurrentBase()` configuration. It adds no mode detection, mode writes, WMI/BIOS access, or hardware behavior. Inherited GPU-mode icon replacement remains isolated to non-HP mode. See [performance evidence](hp-performance-mode-evidence.md).

The files were supplied as the final VictusX asset set; no artwork was generated or modified. Technical source integration is complete. Final package inspection must still match these hashes and confirm the project owner's provenance/license statement and any required attribution before release.
