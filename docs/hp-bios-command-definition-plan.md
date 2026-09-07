# HP BIOS WMI Command Definition Plan

## 1. Why command definitions come before command invocation

Command definitions let VictusX document known HP BIOS WMI command IDs, command families, expected output sizes, and safety classification before any `hpqBIOSInt*` method is called. This creates a reviewable boundary between read-only metadata discovery and actual BIOS WMI command execution.

## 2. Exposed methods on this machine

The current HP capability report shows these `hpqBIntM` methods:

- `hpqBIOSInt0`
- `hpqBIOSInt4`
- `hpqBIOSInt128`
- `hpqBIOSInt1024`
- `hpqBIOSInt4096`

`hpqBDataIn` is available but exposes no methods in the current report.

## 3. What a safe command definition should contain

- Command name.
- Command family, such as system, fan, thermal, GPU, keyboard, battery, display, or lighting.
- Command ID.
- BIOS command group if known, such as default or keyboard.
- Expected input length.
- Expected output method size, matching exposed `hpqBIOSInt*` methods.
- Safety classification: metadata-only, read-intent, write-capable, or forbidden.
- Notes about model uncertainty and whether the command has been verified on HP Victus 16-s0035nt / 7Z5Z2EA.

## 4. Command areas forbidden for now

- Fan level, fan mode, max fan, fan countdown, and fan keepalive writes.
- CPU, GPU, TDP, PL4, loadline, ICCMax, and performance-mode writes.
- Keyboard backlight, brightness, color, animation, idle-mode, and light-bar writes.
- Battery care / charge-limit writes.
- Display overdrive writes.
- EC, BIOS, MMIO, MSR, PawnIO, vendor DLL, native HP SDK, service control, or proprietary binary paths.

## 5. Next tiny code step

Create a definitions-only file under `app/Hardware/Hp/` that declares HP BIOS WMI command metadata as inert data. It must not open WMI, create command buffers, invoke `hpqBIOSInt*`, or connect to hardware.
