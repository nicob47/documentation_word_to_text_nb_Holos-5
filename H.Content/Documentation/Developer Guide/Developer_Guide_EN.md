# Holos Developer Guide

A starting reference for contributors. For environment setup at a deeper level
(troubleshooting, repository checkout, build matrix), see the root-level
[`DEVELOPER_ONBOARDING_GUIDE.md`](../../../DEVELOPER_ONBOARDING_GUIDE.md). For the
overall architectural model see [`ARCHITECTURE.md`](../../../ARCHITECTURE.md); for
the carbon-analysis pipeline specifically see [`Carbon_Model_Flow.md`](./Carbon_Model_Flow.md);
for the code style we follow see [`CODING_STYLE_GUIDE.md`](../../../CODING_STYLE_GUIDE.md).

## Localization

All text in the GUI must have an entry in a resource file. This enables the GUI to support
multiple languages. Canada requires that all software interfaces support both official
languages (English and French). Text that needs to be translated from English to French is
recorded in an online document shared with Holos collaborators:

https://docs.google.com/spreadsheets/d/1qKW-QNd9eCmvQP8VAPcQOgrb9fi_gqSGWFhmVh_JYLA/edit?usp=drive_link

When you add a new user-visible string:

1. Add the English entry to the appropriate `AppStrings.resx` file under `H.Localization/Resources/Strings/`.
2. Reference it from XAML or code via the `Loc` static resource (`{Binding [Your_Key].Value, Source={StaticResource Loc}}`).
3. Add the same key to `AppStrings.fr.resx` with the French translation, or to the shared
   spreadsheet above if translation is pending.

## Prerequisites

- **.NET 9 SDK** — the solution targets `net9.0`. Visual Studio 2022 (17.11+) and the latest
  Rider both bundle a compatible SDK; VS Code users install it separately from
  https://dotnet.microsoft.com/download.
- **Git** — needed for repository checkout and the v4 → v5 cherry-pick workflow described
  in `CONTRIBUTING.md`.
- **Windows 10/11 (x64)** for the supported GUI build. Linux + macOS builds work for the
  core libraries (Avalonia is cross-platform) but are not part of the official release matrix.

Any one of three IDEs is sufficient — pick whichever matches your workflow.

---

## Option 1: Visual Studio 2022 (recommended for most contributors)

### Installation

1. Download Visual Studio 2022 (17.11 or later) from https://visualstudio.microsoft.com/downloads/.
2. In the installer's **Workloads** tab, select:
   - **.NET Desktop Development** — pulls in the .NET 9 SDK, MSBuild, and the WPF / Avalonia preview tooling.
   - **Visual Studio Extension Development** — needed if you intend to modify project / item templates.
3. Finish the install and reboot.

### Avalonia extension

The Avalonia previewer + IntelliSense in `.axaml` files comes from a separate extension:

- Open Visual Studio → **Extensions** → **Manage Extensions**.
- Search for **Avalonia for Visual Studio 2022** (publisher: AvaloniaUI).
- Install, restart Visual Studio.

### Startup project

In Solution Explorer, right-click `H.Avalonia` (under `H.GUI.Avalonia`) and choose
**Set as Startup Project**. F5 then launches the GUI under the debugger.

For the CLI build instead, set `H.CLI` as the startup project. For running the test suites,
use **Test Explorer** (Test → Test Explorer) — it picks up `H.Core.Test` and
`H.Avalonia.Test` automatically.

---

## Option 2: Visual Studio Code

VS Code is fully supported for editing, building, and running tests. The Avalonia previewer
is more limited than in Visual Studio, but day-to-day development (C# editing, debugging,
running the GUI, running tests, git workflows) all work well.

### Extensions

Install these from the VS Code marketplace:

| Extension | Publisher | What it provides |
|---|---|---|
| **C# Dev Kit** | Microsoft | Solution Explorer, project/file scaffolding, test runner, integrated debugger. |
| **C#** | Microsoft | Roslyn-based language server (IntelliSense, refactoring, hover docs). Installed automatically by C# Dev Kit. |
| **.NET Install Tool** | Microsoft | Manages multiple .NET SDK versions. |
| **Avalonia for VSCode** | AvaloniaTeam | Syntax highlighting + completion for `.axaml`. Previewer support is limited compared to Visual Studio — open the Avalonia previewer in VS if you need a live design surface. |
| **GitLens** (optional) | GitKraken | Inline git blame, history, and branch tools. |

After installation:

1. **File → Open Folder** → select the repository root.
2. C# Dev Kit will offer to load the `Holos.sln` solution. Accept.
3. Wait for the Roslyn / OmniSharp language server to finish indexing (status bar bottom-left).

### Build, run, and test from VS Code

Open the integrated terminal (`` Ctrl+` ``) and use the .NET CLI directly:

```bash
# Restore + build the whole solution
dotnet build Holos.sln

# Run the GUI
dotnet run --project H.GUI.Avalonia/H.Avalonia/H.Avalonia.csproj

# Run the CLI
dotnet run --project H.CLI/H.CLI.csproj

# Run the unit tests (per project)
dotnet test H.Core.Test/H.Core.Test.csproj
dotnet test H.GUI.Avalonia/H.Avalonia.Test/H.Avalonia.Test.csproj

# Run a single test by name
dotnet test H.Core.Test/H.Core.Test.csproj --filter "FullyQualifiedName~FarmAnalysisServiceTests"
```

The C# Dev Kit also adds a **Run and Debug** view that picks up tests and lets you set
breakpoints in test methods — click the "play" gutter icon next to any `[TestMethod]` to run
just that test under the debugger.

### Debug configuration

For F5 to launch the GUI under the debugger, create `.vscode/launch.json` at the repo
root with this content:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch H.Avalonia (Debug)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build-avalonia",
      "program": "${workspaceFolder}/H.GUI.Avalonia/H.Avalonia/bin/Debug/net9.0/H.Avalonia.dll",
      "args": [],
      "cwd": "${workspaceFolder}/H.GUI.Avalonia/H.Avalonia",
      "console": "internalConsole",
      "stopAtEntry": false
    },
    {
      "name": "Launch H.CLI (Debug)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build-cli",
      "program": "${workspaceFolder}/H.CLI/bin/Debug/net9.0/H.CLI.dll",
      "args": [],
      "cwd": "${workspaceFolder}/H.CLI",
      "console": "integratedTerminal"
    }
  ]
}
```

And `.vscode/tasks.json`:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build-avalonia",
      "type": "process",
      "command": "dotnet",
      "args": ["build", "${workspaceFolder}/H.GUI.Avalonia/H.Avalonia/H.Avalonia.csproj"],
      "group": { "kind": "build", "isDefault": true },
      "problemMatcher": "$msCompile"
    },
    {
      "label": "build-cli",
      "type": "process",
      "command": "dotnet",
      "args": ["build", "${workspaceFolder}/H.CLI/H.CLI.csproj"],
      "group": "build",
      "problemMatcher": "$msCompile"
    }
  ]
}
```

Neither file is checked in (they're developer-local). Once they're saved, **Run → Start
Debugging** (`F5`) launches the GUI under the debugger; breakpoints set in any C# file in
the workspace will hit.

### Tips when working in VS Code

- **Solution Explorer**: provided by C# Dev Kit — opens via the .NET icon in the Activity
  Bar (left edge). Treat it the same way you would Visual Studio's Solution Explorer.
- **Logs**: Avalonia + NLog output lands in the **Debug Console** when F5'd from VS Code,
  or in the integrated terminal when running via `dotnet run`. NLog also writes
  `logs/app-<date>.log` in the run directory — see `NLog.config` for the layout.
- **XAML preview**: the VS Code Avalonia extension doesn't ship a full live previewer.
  For complex XAML changes, do the layout work in Visual Studio (which has the Avalonia
  designer) and switch back to VS Code for the C# / view-model work.
- **Tests**: open `Testing` from the Activity Bar (the flask icon) to see the test tree.

---

## Option 3: JetBrains Rider

Rider works out of the box with `Holos.sln` — no special configuration. The bundled Avalonia
plugin gives previewer + completion equivalent to Visual Studio. Set `H.Avalonia` as the
startup project (right-click → **Properties** → set the run configuration) and press the
green play button.

Rider's built-in test runner picks up both `H.Core.Test` and `H.Avalonia.Test` automatically.
The integrated terminal supports the same `dotnet test` / `dotnet build` commands listed
above for VS Code.

---

## Solution layout

```
Holos-5/
├── H.Core/                          ← Calculation engine. All carbon / N / animal math.
├── H.Core.Test/                     ← MSTest unit tests for H.Core.
├── H.GUI.Avalonia/
│   ├── H.Avalonia/                  ← Avalonia GUI (main entry point).
│   └── H.Avalonia.Test/             ← MSTest tests for the GUI VMs.
├── H.CLI/                           ← Command-line bulk-analysis tool.
├── H.Content/                       ← CSVs, documentation, training material.
│   └── Documentation/
│       └── Developer Guide/         ← You are here.
├── H.Infrastructure/                ← Shared helpers (KML parsing, network, units).
├── H.Localization/                  ← Translated resource strings (English + French).
├── H.Integration/                   ← Integration tests that touch real data files.
└── H.Economic.Data/                 ← Economics templates + helper docs.
```

For a deeper architectural narrative — how the partials fit together, how DI is configured,
how the analysis pipeline flows — see [`ARCHITECTURE.md`](../../../ARCHITECTURE.md) and the
visual flowchart in [`Carbon_Model_Flow.md`](./Carbon_Model_Flow.md).

## Logging

The whole codebase logs through **NLog** ([`NLog.config`](../../../H.GUI.Avalonia/H.Avalonia/NLog.config))
with a unified format: `HH:mm:ss.ffff [LEVEL] [Class.Method] message`.

- `ILogger` injected via DI is preferred for classes the container constructs.
- Classes the container doesn't construct (providers, helpers, partial classes) use a static
  `private static readonly Logger _log = LogManager.GetCurrentClassLogger();` field.
- Do **not** use `System.Diagnostics.Trace.*` — those calls bypass NLog and produce a
  competing format. The migration is complete; new code should follow the established
  pattern. Full rationale in [`CODING_STYLE_GUIDE.md`](../../../CODING_STYLE_GUIDE.md) under
  "Logging".

Logs land in three places at once:

| Target | Where | When |
|---|---|---|
| Console | Integrated terminal / `dotnet run` output | Debug+ level |
| Debug | VS Output > Debug pane (or VS Code Debug Console) | Debug+ level, only when a debugger is attached |
| File | `logs/app-<YYYY-MM-DD>.log` relative to the run directory | Info+ level |

## Tests

The two MSTest projects (`H.Core.Test`, `H.Avalonia.Test`) cover roughly 1,650 unit tests
total. Pre-commit baseline:

- `H.Core.Test`: ~1,320 passed / 0 failed / 14 skipped.
- `H.Avalonia.Test`: 348 passed / 0 failed / 6 skipped.

Run both before opening a PR. The skipped tests are documented in their `[Ignore]`
attributes — usually a pending v4 → v5 port or a flaky integration test that needs network
access.

Filter by class with `--filter "FullyQualifiedName~ClassName"` to narrow the run while
working on one area.

## Coding style

Style + naming conventions live in [`CODING_STYLE_GUIDE.md`](../../../CODING_STYLE_GUIDE.md).
The two pitfalls most likely to bite a new contributor:

- **Avalonia binding pitfall**: `StringFormat` + two-way bindings throw at runtime. Use
  `NumericUpDown.FormatString` or read-only contexts instead. Documented in the style guide.
- **Logging**: use `_logger.LogX(...)` / `_log.X(...)` — never `Trace.*` (see above).

## Working with the carbon model

A wheat field's journey from the GUI authoring screens through to the GHG results chart
runs through ~60 documented classes. The end-to-end Mermaid flowchart and the file index
that maps each step to its implementation file are in
[`Carbon_Model_Flow.md`](./Carbon_Model_Flow.md). Start there before making changes to the
carbon or nitrogen calculators — the ordering invariants (carbon before nitrogen, animal
results primed between stage-state build and final pass, etc.) are not obvious from the
call sites alone.
