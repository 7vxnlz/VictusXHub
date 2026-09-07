# Implementation Readiness Plan

This plan defines the first coding milestone for VictusControl. It is intentionally limited to project boundaries and verification. It does not include application behavior, HP WMI commands, fan control, telemetry, UI polish, or copied reference code.

## First Coding Milestone

Milestone: `v0.1.0-solution-skeleton`

Create a clean .NET solution skeleton with explicit project boundaries, empty or minimal compilable projects, and test projects wired to the solution. The milestone should prove that the repository can build and test a layered structure before any hardware or UI behavior exists.

## Why This Comes First

This milestone must come before HP WMI, fan control, telemetry, or UI polish because those areas are safety-sensitive and easy to entangle if the project shape is improvised later. A skeleton establishes the dependency direction, testing surface, and naming conventions while the cost of change is still tiny.

It also keeps future AI sessions token-efficient: assistants can inspect one layer at a time, choose files by recipe, and avoid dragging hardware implementation details into UI, settings, or test tasks.

## Files And Projects To Create

Expected structure for the first coding milestone:

```text
VictusControl.slnx
src/
  VictusControl.App/
    VictusControl.App.csproj
  VictusControl.Application/
    VictusControl.Application.csproj
  VictusControl.Domain/
    VictusControl.Domain.csproj
  VictusControl.Hardware.Abstractions/
    VictusControl.Hardware.Abstractions.csproj
  VictusControl.Infrastructure/
    VictusControl.Infrastructure.csproj
tests/
  VictusControl.Domain.Tests/
    VictusControl.Domain.Tests.csproj
  VictusControl.Application.Tests/
    VictusControl.Application.Tests.csproj
```

Recommended dependency direction:

- `VictusControl.App` may reference `Application`, `Domain`, `Hardware.Abstractions`, and `Infrastructure` only as needed for startup wiring.
- `VictusControl.Application` may reference `Domain` and `Hardware.Abstractions`.
- `VictusControl.Infrastructure` may reference `Application`, `Domain`, and `Hardware.Abstractions` only when concrete infrastructure services are introduced later.
- `VictusControl.Hardware.Abstractions` may reference `Domain` if shared domain types are needed later.
- `VictusControl.Domain` should not reference other VictusControl projects.
- Tests should reference only the projects they test.

The first coding milestone may include only tiny compile anchors if required by SDK tooling, such as assembly-level metadata or placeholder files. These anchors must not contain product behavior.

## Files Not To Create Yet

Do not create these in the first milestone:

- HP WMI implementation projects or classes
- fan control classes, fan curves, thermal profiles, or hardware command encoders
- telemetry polling, sensor services, LibreHardwareMonitor integration, HWiNFO integration, or EC access
- tray app behavior, settings screens, dashboards, icons, themes, or polished UI pages
- installers, packaging scripts, update mechanisms, GitHub Actions, signing configuration, or release automation
- vendor binaries, HP DLLs, drivers, PawnIO payloads, native driver projects, or copied reference assets
- broad architecture documents beyond this readiness plan unless explicitly requested

## Relevant References

Relevant for this milestone:

- `seerge/g-helper`: useful only for compact app/repo ergonomics and a small Windows utility mindset.
- `theantipopau/omencore`: useful only for solution separation, tests, and keeping hardware concerns isolated from app code.
- `affaan-m/ECC`: useful only if refining AI workflow conventions, not for application structure.

Use these references through `docs/reference-inventory.md` first. Read exact files only if the skeleton task raises a specific question.

## References Not Needed Yet

Not needed for the first milestone:

- `ib-3/ghelper-omen`: defer until HP WMI and Victus-specific hardware questions begin.
- `breadeding/OmenSuperHub`: defer until fan UX, tray/config behavior, or alternative Omen control concepts are scoped.
- `MasonDye/OmenXHub`: defer until WPF service boundaries, fan pages, or richer app-shell comparisons are scoped.

No reference source should be copied into the skeleton.

## Recommended Model

Use `GPT-5.5 Medium` for the first coding milestone.

Reason: the work is multi-project and boundary-oriented, but not hardware-risky. It needs enough reasoning to keep dependencies clean without escalating to a high-risk hardware model.

## Build And Test Verification

Expected verification after the skeleton is created:

```text
dotnet build VictusControl.slnx
dotnet test VictusControl.slnx
```

If `.slnx` is not supported by the installed SDK, use a standard `.sln` instead and document the reason in the session summary. Do not add CI or packaging in this milestone.

## Risks

- Overengineering the skeleton before real requirements arrive.
- Choosing a UI framework too early and letting it shape domain/application boundaries.
- Creating empty projects with fake abstractions that future code has to unwind.
- Accidentally introducing hardware semantics into project names or placeholders.
- Pulling reference code into the skeleton instead of independently creating project boundaries.
- Selecting SDK versions that are unavailable on the maintainer machine.

## Milestone Commit Message

Recommended commit message after the first coding milestone:

```text
chore: add solution skeleton
```

For a versioned milestone commit, this is also acceptable:

```text
v0.1.0: add solution skeleton
```

## Immediately After The Milestone

After `v0.1.0-solution-skeleton`, update `SESSION_STATE.md` with the created projects and build/test status. Then make the next planning decision before coding hardware:

Recommended next step: define the first domain and hardware-abstraction contracts for device identity and capability detection, still without HP WMI command implementation.
