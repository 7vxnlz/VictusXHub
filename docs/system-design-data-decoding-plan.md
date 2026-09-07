# SystemDesignData Decoding Plan

## Expected result

The elevated `SystemDesignData` read-only HP WMI invocation returned 128 bytes. References identify this as HP's `Default 0x28` system design/capability block.

## Reference evidence

- `ghelper-omen/app/Omen/HpWmiBios.cs` uses command type `0x28` with `hpqBIOSInt128` for system data.
- `OmenCore/src/OmenCoreApp/Hardware/HpWmiBios.cs` documents a side-effect-free `DecodeSystemDesignData` parser and treats the first 12 bytes as the fields HP accessors read.
- `OmenSuperHub/OmenHardware.cs` and `OmenXHub/OmenHardware.cs` also read adapter wattage from bytes `[0..1]` and thermal policy from byte `[3]`.

## Known fields for the scaffold

The decoder currently treats only the first 12 bytes as reference-backed:

- bytes `[0..1]`: shipping adapter power rating, little-endian watts
- byte `[2]`: reserved/unknown
- byte `[3]`: thermal policy version
- byte `[4]`: platform feature flags
- byte `[5]`: PL4 default value
- byte `[6]` bit 0: BIOS-defined OC support declaration
- byte `[7]`: raw GPU mode switch bitfield
- byte `[8]`: default CPU power limit with dGPU loaded, watts
- byte `[9]`: load-line support/default nibbles
- byte `[10]`: sensor flags
- byte `[11]`: hotkey flags

## Unknown fields

Bytes `[12..127]` remain unknown in VictusX. The scaffold records only summary metadata for them:

- unknown byte count
- unknown byte range
- non-zero unknown byte count

It does not preserve or interpret the full unknown tail.

## Safe decoding strategy

- Decode only from a provided byte array.
- Require at least 12 bytes for known fields.
- Expect 128 bytes, but allow shorter captured buffers if the known prefix exists.
- Keep all unsupported or ambiguous meanings as raw flags or declarations.
- Do not infer that any control feature is safe merely because a flag is set.
- Do not connect decoding to live WMI invocation yet.

## What not to infer yet

- Fan control support is not implemented.
- Performance mode control is not implemented.
- Battery charge limit control is not implemented.
- RGB or keyboard lighting control is not implemented.
- GPU mode control is not implemented.
- EC access and BIOS writes remain forbidden.
- A decoded capability flag is not approval to expose UI or send control commands.

## Next step after decoder scaffold

Add a captured-output import path for developer-provided `SystemDesignData` bytes, then decode those bytes offline into a report section without invoking HP WMI again.
