# VictusX Icon Wiring Checkpoint

## Approved Identity

- Approval date: 2026-09-08.
- `app/Assets/VictusX.Source.png` is the approved source master for the single VictusX application identity.
- `app/Assets/VictusX.ico` is generated from that source without redesigning or stretching it.
- The previous Silent, Balanced, and Turbo icon variants are superseded and removed.

## Provenance

The project owner directed creation of this icon specifically for VictusXHub during the owner's ChatGPT image-generation design session. The owner states that it was created as a new design rather than sourced from an existing third-party icon and that no HP, OMEN, Victus, G-Helper, ASUS, ROG, Windows, or other vendor logo or marketing asset was intentionally used as source material.

No separate third-party artwork source or attribution requirement is identified by the recorded owner declaration. This record makes no broader copyright or trademark claim.

| Asset | SHA-256 |
| --- | --- |
| `app/Assets/VictusX.Source.png` | `17C680EC0DA1D3EE6C60F39B9BAE123E8E6EC6C9888AC2C3BCD7E5B51A9CD506` |
| `app/Assets/VictusX.ico` | `D34E7BFF3074B0F3787B214FC97F5979A406801838E01D824D43FE25C20479D3` |

## Asset Validation

The 1254x1254 PNG is readable, square, uncorrupted, and opaque RGB. The generated ICO is a loadable Windows icon with PNG-compressed 32-bit frames at:

```text
256x256, 128x128, 64x64, 48x48, 40x40, 32x32, 24x24, 20x20, 16x16
```

## Final Wiring

- `app/VictusX.csproj` uses `app/Assets/VictusX.ico` unconditionally as `ApplicationIcon` and embeds the same file as `GHelper.Assets.VictusX.ico`.
- HP mode uses that one embedded icon for the tray regardless of performance-mode status. Performance-mode status text remains independent and does not drive visual identity.
- The executable-associated icon remains the fallback if the embedded HP resource cannot be loaded.
- Outside HP mode, the inherited `Properties.Resources.standard` startup icon and GPU-mode tray swapping remain unchanged.
- The fixed executable icon supplies main-window, taskbar, Alt-Tab, and Explorer identity through normal WinForms/Windows executable icon behavior.

This is visual resource wiring only. It adds no mode detection, hardware access, or write behavior. Final visual confirmation on the exact distributable remains part of clean-machine validation.
