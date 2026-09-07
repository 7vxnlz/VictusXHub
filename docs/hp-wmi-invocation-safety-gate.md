# HP WMI Invocation Safety Gate

## 1. Verify before any `hpqBIOSInt*` invocation

- `--hp-victus` mode is explicit.
- Machine identity still reports HP Victus.
- `root\wmi`, `hpqBIntM`, and `hpqBDataIn` are available.
- Target `hpqBIOSInt*` method is exposed on this machine.
- Command definition is marked read-intent, not write-capable or forbidden.
- Input/output sizes match the command definition.

## 2. Invocation types allowed first

- One-shot, user-local, report-only read-intent commands.
- Prefer the least invasive command first: `SystemDesignData` via `hpqBIOSInt128`.
- No polling, timers, retries, keepalive, or automatic reapply behavior.

## 3. Invocation types forbidden

- Fan, thermal, GPU, power, battery, display, keyboard, lighting, EC, BIOS, MMIO, MSR, PawnIO, service-control, vendor-DLL, or native-SDK writes.
- Any command classified as write-capable, forbidden, or unknown.

## 4. Required logging/reporting

- Before invocation: command name, method name, expected input size, expected output size, safety classification.
- After invocation: success/failure, return-code field if present, output length, sanitized error details.
- Never log serial number, UUID, username, MAC address, or full sensitive identifiers.

## 5. Exact next safe coding step

Create a disabled-by-default read-intent invocation client that can execute only `SystemDesignData` under `--hp-victus`, write the sanitized result to the existing HP capability report, and reject every non-read-intent command.
