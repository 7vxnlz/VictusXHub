# HP Fan Read-Only Command Investigation

## References Reviewed

- `ghelper-omen` `1694844d2725e79a2b2065a0a1494fa1d143e3f4`: `app/Omen/HpWmiBios.cs`
- `omencore` `b39b44978902606aa708cc0d78bcfd87e95fd88b`: `src/OmenCoreApp/Hardware/HpWmiBios.cs`, fan capability notes
- `OmenSuperHub` `a6ab6988c446ee5421466097fdf60c0d521e5c81`: `OmenHardware.cs`
- `OmenXHub` `ca84cb011d1d3e5850445d19e45fbea06e83a8fd`: `OmenHardware.cs`

## Fan Commands Found

| CommandType | Name seen in references | Shape | Classification |
| --- | --- | --- | --- |
| `0x10` | `FanGetCount` / `GetFanCount` | default command, 4-byte input, 4-byte output | Read-only candidate |
| `0x2D` | `FanGetLevel` / `GetFanLevel` | default command, 4-byte input, 128-byte output | Read-only candidate |
| `0x26` | `FanMaxGet` / `GetFanMax` | default command, 4-byte input, 4-byte output | Read-only candidate |
| `0x38` | `FanGetRpm` / `GetFanRpmDirect` | default command, 4-byte input, 128-byte output | Read-only candidate, likely V2-specific |
| `0x2F` | `GetFanTable` | default command, 4-byte input, 128-byte output | Ambiguous read candidate |
| `0x2C` / `44` | `GetFanType` | default command, 4-byte input, 128-byte output | Ambiguous read candidate |
| `0x37` | `FanGetLevelV2`; also power-limit write in some comments | default command, 4-byte input for read path; 0-byte output in write-like path | Too ambiguous for now |
| `0x1A` | `SetFanMode` | writes mode payload, 0-byte output | Write/control |
| `0x2E` | `SetFanLevel` | writes fan level payload, 0-byte output | Write/control |
| `0x27` | `SetFanMax` | writes max-fan payload, 0-byte output | Write/control |

## Read-Only Candidates

- Start with `0x10 FanGetCount`: lowest surface area and already used by references as a basic availability/status query.
- Then consider `0x2D FanGetLevel`: likely useful for V1/Victus fan level readback, but report raw levels only until units are proven.
- `0x26 FanMaxGet` can report max-fan latch state, but must never be paired with `0x27`.
- `0x38 FanGetRpm` looks read-only, but references treat it as V2/OMEN Max-specific and it should not be first for this ThermalPolicy V1 Victus.

## Write Or Control Commands

- `0x1A SetFanMode` changes thermal/performance fan policy.
- `0x2E SetFanLevel` writes manual fan levels.
- `0x27 SetFanMax` toggles max-fan behavior.
- Any EC fallback or direct EC fan path remains forbidden.

## Too Risky Or Ambiguous

- `0x37` is unsafe to classify for this device now because references use it as V2 fan level readback in one context and power-limit/write-like behavior in another.
- `0x2F GetFanTable` may be read-shaped, but its payload semantics are not proven.
- `0x2C` / `44 GetFanType` is read-shaped only with the default command and zero input; references also use command type `44` in other command families, so it needs separate proof.

## Required Safety Gates

- Keep every fan command at `ReadIntent` until separately approved.
- Require `--hp-victus`, `--hp-wmi-readonly-test`, elevation, exposed method validation, exact command definition match, expected input/output size match, and single-shot execution.
- Log/report only command name, method, return code, byte count, decoded summary, and redacted errors.
- Never log full raw binary output.
- Add synthetic parser/report tests before decoding any new fan response.
- Do not add polling, heartbeat, retry loops, writes, restore behavior, UI controls, or EC fallback.

## Why SystemDesignData Is Not Enough

`DeclaresSoftwareFanControlSupport=true` is useful because it confirms the firmware advertises a fan-control capability bit on this real Victus. It does not identify safe command IDs, prove readback units, validate write payloads, provide fan bounds, or make fan control safe.

## Recommended Next Code Step

Add a catalog-only `SafeReadOnlyInvocation` candidate for a single manual `FanGetCount` probe, plus a report decoder that records only fan count and protection bits from synthetic tests first. Do not invoke it until the user explicitly approves a separate elevated read-only test.
