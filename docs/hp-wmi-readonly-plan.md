# HP WMI Read-Only Interface Plan

This is a planning document only. It does not authorize fan control, HP WMI writes, EC access, BIOS writes, service changes, or hardware control.

## 1. Relevant HP WMI classes/namespaces

- `root\wmi`: HP BIOS WMI namespace seen across the HP-focused references.
- `hpqBIntM`: HP BIOS method host class.
- `hpqBDataIn`: HP BIOS input payload class.
- `hpqBData`: payload field used by the BIOS input object.
- `hpqBIOSInt{outputSize}`: method-name pattern used by the HP BIOS interface.
- `rwReturnCode`: return-code field to inspect if method invocation is introduced later.
- Existing Windows identity sources remain separate from HP BIOS commands: `Win32_ComputerSystem`, `Win32_ComputerSystemProduct`, and `Win32_BIOS`.

## 2. Operations that appear read-only

- Already-safe probe: connect to `root\wmi` and check whether `hpqBIntM` / `hpqBDataIn` exist without invoking methods.
- Possible later read-only command candidates, still requiring explicit probe gating because they invoke BIOS WMI methods:
  - system design data: `0x28`
  - fan count: `0x10`
  - fan level: `0x2D`
  - fan RPM: `0x38` or legacy `0x11`
  - fan max state: `0x26`
  - temperature: `0x23`
  - GPU power state: `0x21`
  - GPU mode: `0x52` read form only
  - keyboard type: `0x01`
  - keyboard brightness: `0x04`
  - keyboard color table: `0x02`
  - backlight / animation status: `0x06`
  - light bar support: `0x01`
  - light bar RGB state: `0x04`
  - display overdrive state: `0x35`

## 3. Write-capable operations that must stay forbidden

- Fan writes: set fan level, set fan mode, set max fan, fan countdown/keepalive, automatic reapply loops.
- Power writes: CPU power limits, concurrent TDP, GPU power, GPU mode changes, PL4/loadline/IccMax-style tuning.
- Keyboard and lighting writes: backlight, brightness, zone colors, animations, light bar colors/brightness/animations, idle lighting.
- Display and battery writes: display overdrive set, battery care / charge-limit set.
- Any EC, BIOS, MMIO, MSR, PawnIO, native HP DLL, vendor service control, or proprietary binary path.

## 4. Most useful reference repo/file

- Primary: `D:\Projects\Workspace\references\ghelper-omen\app\Omen\HpWmiBios.cs`
- Supporting interface shape: `D:\Projects\Workspace\references\ghelper-omen\app\Omen\IHpWmiBios.cs`
- Cross-check only: `D:\Projects\Workspace\references\OmenXHub\OmenHardware.cs`
- Older/raw cross-check only: `D:\Projects\Workspace\references\OmenSuperHub\OmenHardware.cs`
- Safety/capability framing only: `D:\Projects\Workspace\references\omencore`

## 5. Smallest safe first implementation step

Create a read-only HP WMI client that reports namespace/class/method availability only. It should not invoke `hpqBIOSInt*` methods yet. This keeps the next step safer than even read-intent BIOS commands, because it only confirms the interface surface exposed by Windows WMI.

## 6. Exact files that should be created in VictusX next

- `app/Hardware/Hp/IHpWmiReadOnlyClient.cs`
- `app/Hardware/Hp/HpWmiReadOnlyClient.cs`
- `app/Hardware/Hp/HpWmiReadOnlySnapshot.cs`

The existing `app/Hardware/Hp/HpVictusCapabilityProbe.cs` may then be updated to include the read-only WMI surface snapshot in the existing `--hp-victus` report path.

## 7. Out of scope

- Invoking HP BIOS WMI commands, even those believed to be read-only.
- Fan control, thermal/performance mode control, GPU mode control, keyboard lighting control, battery charge control, display overdrive control.
- EC access, BIOS writes, registry/service writes, vendor DLLs, proprietary HP binaries, background hardware loops, telemetry loops, or UI changes.
- Assuming OMEN and Victus command behavior is identical.
