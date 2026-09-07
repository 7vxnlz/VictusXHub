# SetFanMax Payload-Length Final Audit

## 1. Current Device Facts

| Fact | Known value |
| --- | --- |
| Model | HP Victus Gaming Laptop 16-s0035nt |
| SKU | `7Z5Z2EA#AB8` |
| BIOS | `F.31` |
| Thermal policy | V1 (`1`) |
| Fan count | `2` |
| Max-fan readback | `FanMaxGet` works; observed disabled |
| Fan-level readback | `FanGetLevel` works; values remain raw-only |

These are proven read-only facts. None identifies the SetFanMax input ABI.

## 2. Four-Byte Evidence

ghelper-omen and omencore use command `0x20008`, command type `0x27`, `hpqBIOSInt0`, and a four-byte input `{ state, 0, 0, 0 }`. omencore has the closest known cohort: `8BD4`, Victus 16-s0xxx, V1, two fan zones, BIOS `F.30`. However, its SetFanMax helper is generic, the cohort is `UserVerified=false`, and no exact-cohort write/readback record validates four bytes. The device under audit uses BIOS `F.31`.

## 3. One-Byte Evidence

OmenSuperHub and OmenXHub use the same command and method with a one-byte input containing only the state. Their helpers are generic rather than selected by model or BIOS. OmenXHub is derived primarily from OmenSuperHub, so matching code is shared-lineage evidence rather than independent validation. Neither project provides an exact `16-s0035nt`, `7Z5Z2EA`, or `F.31` SetFanMax trace, and neither reviewed flow pairs the write with `FanMaxGet` before, after, and after restore.

## 4. Why Neither Is Sufficient

- No independently reviewable record binds either length to this exact model, SKU, and BIOS.
- V1 policy, two fans, and successful read-only commands do not define a write input size.
- Generic helpers show project assumptions, not firmware contracts.
- A successful WMI return alone would not prove the latch changed or restored.
- The references use different surrounding mode, level, retry, and EC behaviors, so outcomes cannot be attributed to payload length alone.
- The closest four-byte cohort differs by BIOS and is explicitly unverified; the one-byte projects lack a comparable cohort record.

## 5. Evidence Required To Select One Byte

Selection of `1` requires one independently reviewable record for this exact device/BIOS, or documented firmware-equivalent hardware, showing all of the following:

1. `hpqBIOSInt0`, command `0x20008`, command type `0x27`, and input `Size=1`.
2. No alternate-length retry or fallback.
3. A disabled `FanMaxGet` baseline, confirmed enabled state after the bounded action, and confirmed disabled state after restore.
4. The same one-byte shape for enable and restore.
5. Recorded return codes, timing, AC power, thermal observation, recovery readiness, and explicit human review.

## 6. Evidence Required To Select Four Bytes

Selection of `4` requires the same independently reviewable device/BIOS evidence, specifically showing:

1. `hpqBIOSInt0`, command `0x20008`, command type `0x27`, and input `Size=4`.
2. Payloads exactly `01-00-00-00` and `00-00-00-00`, with no alternate-length retry or fallback.
3. A disabled `FanMaxGet` baseline, confirmed enabled state after the bounded action, and confirmed disabled state after restore.
4. The same four-byte shape for enable and restore.
5. Recorded return codes, timing, AC power, thermal observation, recovery readiness, and explicit human review.

## 7. Why Guessing Is Forbidden

`hpqBDataIn.Size` is part of the firmware command contract. Choosing a shape by repository count, trailing-zero intuition, model-family similarity, or successful read-only behavior would silently convert uncertainty into a BIOS write. A rejected command, accepted-but-ignored command, persistent max-fan latch, or interaction with another fan mode cannot be ruled out. No default, fallback, probing sequence, or trial of both lengths is acceptable.

## 8. Payload-Length Decision

`DeviceValidatedInputLength` remains **unset**. Neither `1` nor `4` is selected.

## 9. Go / No-Go State

SetFanMax remains **NO-GO**. No implementation task, payload construction for execution, WMI write path, fan-control UI, or hardware experiment is authorized.

## 10. Exact Next Safe Step

Perform a documentation-only search and independent review for an existing, sanitized field record naming this exact model/SKU/BIOS (or proven firmware-equivalent hardware) and recording command `0x27`, exact input size, return code, and `FanMaxGet` baseline/post-action/post-restore results. If that complete record is not found, stop: keep the input length unset and the implementation NO-GO.

Supporting evidence: [model/BIOS matrix](set-fan-max-model-bios-evidence-matrix.md), [omencore four-byte deep dive](omencore-victus-16s-setfanmax-evidence-deep-dive.md), and [OmenSuperHub/OmenXHub one-byte deep dive](omensuperhub-omenxhub-setfanmax-one-byte-evidence-deep-dive.md).

The [SetFanMax proof gap checklist](set-fan-max-proof-gap-checklist.md) is the final source of truth for all evidence still required. This audit does not select a payload length; `DeviceValidatedInputLength` remains unset and the gate remains **NO-GO**.
