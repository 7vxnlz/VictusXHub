# OmenSuperHub and OmenXHub SetFanMax One-Byte Evidence Deep Dive

## Files Reviewed

Current reference commits:

- OmenSuperHub: `a6ab6988c446ee5421466097fdf60c0d521e5c81`
- OmenXHub: `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`

Files inspected:

- OmenSuperHub `OmenHardware.cs`
- OmenSuperHub `Program.cs`
- OmenXHub `OmenHardware.cs`
- OmenXHub `Services/TrayService.cs`
- OmenXHub `README.en.md`

Targeted history searches also inspected the former OmenSuperHub `PlatformSettings.cs` model map and OmenXHub `App/OmenLighting.cs` identity matches. Neither supplies SetFanMax validation for this device.

## SetFanMax Method And Input Shape

Both projects define the same calls:

- Enable: command type `0x27`, data `01`, output size `0`
- Disable: command type `0x27`, data `00`, output size `0`
- Command: default `0x20008`
- WMI namespace/class: `root\wmi` / `hpqBIntM`
- Input object: `hpqBDataIn`
- Input `Size`: derived from `data.Length`, therefore `1`
- Invoked method: `hpqBIOSInt0`

The one-byte methods are shared static helpers with no model, BIOS, thermal-policy, or board-ID branch. OmenSuperHub's form dates to its initial project history. OmenXHub describes itself as based primarily on OmenSuperHub, so the matching implementation is shared-lineage evidence, not two independent device validations.

## Model And BIOS Evidence

No current or historical match was found for `16-s0035nt`, `7Z5Z2EA`, or a SetFanMax result on BIOS `F.31`.

Historical OmenSuperHub platform data maps board `8BD4` to the `Bigred` platform family, but does not connect that entry to command `0x27`, a one-byte input, a BIOS revision, or a successful write/restore trace. OmenXHub contains an `8BCD/F.31` keyboard-lighting note; it concerns another board and subsystem.

OmenXHub broadly lists OMEN and Victus compatibility and V1 thermal-policy handling, but says it was primarily developed for an OMEN 10 Intel system and does not guarantee all platforms. Its `GetFanLevel` comment names a generic `Victus_S` path, not this exact model or SetFanMax ABI.

## Surrounding Fan Flow

OmenSuperHub uses BIOS WMI for the reviewed fan flow:

- `SetFanMode`: command type `0x1A`
- `SetFanLevel`: command type `0x2E`
- Max mode: repeated `SetMaxFanSpeedOn`
- Fixed-level or automatic transitions: `SetMaxFanSpeedOff` before level/timer behavior
- Thermal protection: switches to automatic/cool behavior and sends max-fan off

OmenXHub also sends SetFanMax through BIOS WMI, but its wider fan path is mixed. `SetFanLevel` first uses BIOS WMI and may fall back to direct EC access through its EC service. Its AMD timer repeatedly sends SetFanMax off followed by SetFanLevel as a software-control keepalive. Several UI/runtime “max” paths use max-fan off plus a 100-percent level write rather than the SetFanMax-on latch.

These multi-command flows cannot show that the one-byte SetFanMax call alone produced or restored a particular hardware state.

## Restore And Readback

Both projects use `SetMaxFanSpeedOff` as a release, preparation, or restore action. No matching `FanMaxGet`/command `0x26` implementation or before/after/restore readback was found in the reviewed primary flow. A zero WMI return code can show that firmware accepted a request, but does not prove the max-fan latch changed or was restored.

OmenXHub additionally documents cases where repeated WMI/EC fan writes caused zero-speed or non-responsive behavior. That history supports stricter isolation and readback requirements, not payload approval.

## Comparison With This Victus

Similarities:

- HP BIOS WMI transport and command family.
- V1 thermal policy is supported by both projects.
- Broad Victus and software fan-control assumptions.
- This device has two fans and working read-only `FanMaxGet` and `FanGetLevel` queries.

Differences and missing links:

- No exact model, SKU, board-ID, or BIOS `F.31` one-byte record.
- The one-byte helper is generic rather than cohort-selected.
- OmenXHub is derived from OmenSuperHub, reducing evidentiary independence.
- OmenXHub's broader fan flow can mix BIOS WMI and EC behavior.
- Neither reviewed flow proves max-fan state with `FanMaxGet` before, after, and after restore.

## Decision

This evidence is **not enough** to choose one byte. It establishes that one byte is a reference-backed implementation shape, but not that it is the validated ABI for this Victus. It also does not disprove the four-byte shape used by other references.

`DeviceValidatedInputLength` remains **unset** and SetFanMax remains **NO-GO**.

## Current Recommendation

Keep all write code absent. The next safe task is a documentation-only search for an independently reviewable `16-s0035nt`, `7Z5Z2EA`, `8BD4`, or BIOS `F.31` field record that includes command `0x27`, exact input size, return code, and `FanMaxGet` baseline/post-action/post-restore results. Do not infer device validation from these shared generic helpers.
