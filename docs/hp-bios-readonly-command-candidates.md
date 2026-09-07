# HP BIOS WMI Read-Only Command Candidates

No command is approved for invocation yet. The current milestone only classifies candidates for later review.

| Candidate command | Reference repo/file | Why it may be read-only | Risk | Approved for invocation now | Reason if not approved |
|---|---|---|---|---|---|
| `SystemDesignData` | `ghelper-omen/app/Omen/HpWmiBios.cs`; cross-check `OmenXHub/OmenHardware.cs`, `OmenSuperHub/OmenHardware.cs` | Named and used as a `GetSystemDesignData` query returning platform/design bytes. | Low | No | Needs one explicit invocation milestone, before/after logging, and sanitized output handling. |
| `FanCount` | `ghelper-omen/app/Omen/HpWmiBios.cs`; cross-check `OmenXHub/OmenHardware.cs`, `OmenSuperHub/OmenHardware.cs` | Named as fan-count/status query and does not set fan level or mode. | Medium | No | Fan-related reads should wait until system-design invocation is proven safe. |
| `FanLevel` | `ghelper-omen/app/Omen/HpWmiBios.cs`; cross-check `OmenXHub/OmenHardware.cs`, `OmenSuperHub/OmenHardware.cs` | Named as current fan-level query. | Medium | No | Fan-level semantics vary by model; keep blocked until Victus-specific readback is understood. |
| `FanRpm` | `ghelper-omen/app/Omen/HpWmiBios.cs`; cross-check `OmenXHub/OmenHardware.cs` | Named as direct RPM query on supported firmware. | Medium | No | Firmware/model-specific; blocked until lower-risk reads succeed. |
| `FanMaxState` | `ghelper-omen/app/Omen/HpWmiBios.cs` | Named as max-fan state query, separate from max-fan write. | Medium | No | Fan-control-adjacent; blocked until fan read safety is reviewed separately. |
| `Temperature` | `ghelper-omen/app/Omen/HpWmiBios.cs`; cross-check `OmenXHub/OmenHardware.cs`, `OmenSuperHub/OmenHardware.cs` | Named as sensor temperature query. | Low | No | Wait for the first system-design read before adding more command invocations. |
| `GpuPowerState` | `ghelper-omen/app/Omen/HpWmiBios.cs` | Named as GPU power-state query, separate from GPU power write. | Medium | No | GPU state commands are close to write-capable flows; keep blocked. |
| `KeyboardType` | `ghelper-omen/app/Omen/HpWmiBios.cs` | Named as keyboard type query. | Low | No | Safe-looking, but not needed before system capability proof. |
| `KeyboardBrightness` | `ghelper-omen/app/Omen/HpWmiBios.cs` | Named as brightness query, separate from brightness writes. | Medium | No | Lighting paths need separate privacy/safety review. |
| `KeyboardColorTable` | `ghelper-omen/app/Omen/HpWmiBios.cs` | Named as color-table query. | Medium | No | Lighting paths stay blocked until keyboard capability work begins. |
| `LightBarSupport` | `ghelper-omen/app/Omen/HpWmiBios.cs` | Named as light-bar support probe. | Medium | No | Light-bar paths are not needed for first read-only WMI invocation. |
| `LightBarRgb` | `ghelper-omen/app/Omen/HpWmiBios.cs` | Named as current RGB state query. | Medium | No | Lighting state reads are deferred. |
| `DisplayOverdrive` | `ghelper-omen/app/Omen/HpWmiBios.cs` | Named as display overdrive status query. | Medium | No | Display-control-adjacent; defer until core HP WMI plumbing is proven. |

## Blocked or ambiguous commands

- `GpuMode`: kept blocked as unknown because references use the same command ID around GPU mode read and write flows.
- Fan level V2 / command `0x37`: not added as an approved candidate because references also use `0x37` for power-limit style commands.
- Battery care: not approved for HP BIOS WMI invocation; the referenced read path uses `root\HP\InstrumentedBIOS`, while charge-limit writes remain forbidden.

## Next safe step

Approve only `SystemDesignData` for a future single-shot `--hp-victus` read-only invocation milestone, with full before/after logging and sanitized report output.
