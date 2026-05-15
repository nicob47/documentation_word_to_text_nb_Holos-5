# Holos Development Environment Setup Guide

## Overview

This guide will help new developers set up their development environment for the Holos application. Holos is a sophisticated agricultural carbon footprint calculation and farm management desktop application built with .NET 9, Avalonia UI, and modern architectural patterns.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Development Tools Installation](#development-tools-installation)
3. [Repository Setup](#repository-setup)
4. [Project Configuration](#project-configuration)
5. [Building the Solution](#building-the-solution)
6. [Running the Application](#running-the-application)
7. [Development Workflow](#development-workflow)
8. [Troubleshooting](#troubleshooting)
9. [Additional Resources](#additional-resources)

---

## Prerequisites

Before you begin, ensure your development machine meets these requirements:

### System Requirements
- **Operating System**: Windows 10/11 (x64) or Windows Server 2016+ 
- **RAM**: Minimum 8GB (16GB+ recommended for optimal performance)
- **Storage**: At least 10GB free disk space for tools and source code
- **Processor**: x64 processor

### Required Accounts
- **GitHub Account**: For repository access
- **Microsoft Account**: For Visual Studio licensing (if using Visual Studio)

---

## Development Tools Installation

### 1. Install .NET 9 SDK

The Holos application targets .NET 9, so you'll need the latest .NET 9 SDK.

**Download and Install:**
1. Visit the [.NET Download page](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Download the **.NET 9 SDK** (not just the runtime)
3. Run the installer and follow the setup wizard
4. Verify installation by opening a command prompt and running:
   ```bash
   dotnet --version
   ```
   You should see version 9.0.x or higher.

### 2. Install Visual Studio 2022

Any of three IDEs are sufficient for working on Holos — Visual Studio 2022, Visual Studio
Code, or JetBrains Rider. Pick whichever matches your workflow. Visual Studio 2022 is
covered here because it is the most common choice for new .NET contributors and provides
the fullest Avalonia previewer experience; see `H.Content/Documentation/Developer Guide/Developer_Guide_EN.md`
for VS Code and Rider setup steps.

**Visual Studio 2022 Community/Professional/Enterprise:**

1. Download from [Visual Studio website](https://visualstudio.microsoft.com/downloads/)
2. During installation, select these workloads:
   - **.NET Desktop Development**
   - **ASP.NET and web development** (for additional tools)
   
3. In the **Individual Components** tab, ensure you have:
   - **.NET 9.0 Runtime (Long Term Support)**
   - **NuGet package manager**
   - **Git for Windows** (if not already installed)
   - **GitHub Extension for Visual Studio** — optional; the built-in Git tooling in Visual Studio is sufficient on its own.

**Required Extensions:**
After Visual Studio installation, install these extensions:
- **Avalonia for Visual Studio** - For XAML editing support
- **NLog Language Service** - For NLog configuration support

### 3. Install Visual Studio Code

VS Code is fully supported for editing, building, running, and debugging Holos. Day-to-day
development (C# editing, debugging, running the GUI, running tests, git workflows) all
work well. The Avalonia previewer is more limited than in Visual Studio; for complex XAML
layout work some contributors switch to VS for the design surface, but everything else
runs equally well in either IDE.

**Install VS Code:**

1. Download from https://code.visualstudio.com/ and run the installer.
2. Accept the defaults (the "Add to PATH" option matters for the dotnet CLI workflow below).

**Install the required extensions** from the VS Code marketplace
(`Ctrl+Shift+X` to open the Extensions panel):

| Extension | Publisher | What it provides |
|---|---|---|
| **C# Dev Kit** | Microsoft | Solution Explorer, project/file scaffolding, integrated test runner, debugger. Pulls in the C# language extension automatically. |
| **C#** | Microsoft | Roslyn-based language server — IntelliSense, refactoring, hover docs, code lenses. Installed automatically with C# Dev Kit. |
| **.NET Install Tool** | Microsoft | Manages multiple .NET SDK versions side by side. |
| **Avalonia for VSCode** | AvaloniaTeam | Syntax highlighting + completion for `.axaml`. Note: the live previewer is more limited than the Visual Studio version. |
| **NuGet Gallery** (optional) | patcx | Convenient GUI for browsing / adding NuGet packages without dropping to the CLI. |
| **GitLens** (optional) | GitKraken | Inline git blame, history, and branch tools. |

**Open the solution:**

1. **File → Open Folder** → select the cloned `Holos-5` repository root (not a sub-folder).
2. C# Dev Kit will detect `Holos.sln` and prompt to load it. Accept.
3. Wait for the Roslyn language server to finish indexing — the status bar at the bottom
   shows progress. Indexing the full solution takes 30–90 seconds on first open.
4. The **Solution Explorer** appears under the .NET icon in the Activity Bar on the left
   edge. Use it the same way you would Visual Studio's Solution Explorer.

**Build, run, and test from the integrated terminal** (`` Ctrl+` `` to open):

```bash
# Restore + build the whole solution
dotnet build Holos.sln

# Run the GUI
dotnet run --project H.GUI.Avalonia/H.Avalonia/H.Avalonia.csproj

# Run the CLI
dotnet run --project H.CLI/H.CLI.csproj

# Run all unit tests for a project
dotnet test H.Core.Test/H.Core.Test.csproj
dotnet test H.GUI.Avalonia/H.Avalonia.Test/H.Avalonia.Test.csproj

# Run a single test class
dotnet test H.Core.Test/H.Core.Test.csproj --filter "FullyQualifiedName~FarmAnalysisServiceTests"
```

The **Run and Debug** view (`Ctrl+Shift+D`) shows discovered tests once C# Dev Kit
finishes indexing — click the gutter "play" icon next to any `[TestMethod]` to run it
under the debugger.

**Configure F5 debugging:**

Create `.vscode/launch.json` at the repo root with the GUI + CLI run configurations:

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

And `.vscode/tasks.json` to back the `preLaunchTask` references above:

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

Neither file is checked in (the `.vscode/` directory is developer-local). Once saved,
**Run → Start Debugging** (`F5`) builds and launches the GUI under the debugger;
breakpoints set in any C# file in the workspace will hit.

**XAML caveat:** the VS Code Avalonia extension provides syntax highlighting and
auto-complete inside `.axaml` files but does not ship a full live previewer comparable to
Visual Studio. If you're doing heavy XAML layout work, open the affected `.axaml` in
Visual Studio for the live designer, then switch back to VS Code for the C# / view-model
edits. For day-to-day work (changing a binding, adjusting a margin, tweaking a converter)
the VS Code experience is fine.

**Logs:** Avalonia + NLog output appears in the **Debug Console** when launched via F5,
or in the integrated terminal when launched via `dotnet run`. NLog also writes
`logs/app-<YYYY-MM-DD>.log` in the run directory. Configuration lives in
`H.GUI.Avalonia/H.Avalonia/NLog.config`.

### 4. Install Git

If Git is not already installed:

1. Download from [Git official website](https://git-scm.com/downloads)
2. During installation, choose these options:
   - Use Git from the Windows Command Prompt
   - Use the OpenSSL library
   - Checkout Windows-style, commit Unix-style line endings
   - Use Windows' default console window

---

## Repository Setup

### 1. Clone the Repository

**Using Visual Studio:**
1. Open Visual Studio
2. Select "Clone a repository"
3. Enter repository URL: `https://github.com/holos-aafc/Holos-5`
4. Choose your local path (e.g., `C:\source\repos\Holos-5`)
5. Click "Clone"

**Using Command Line:**
```bash
# Navigate to your preferred development directory
cd C:\source\repos

# Clone the repository
git clone https://github.com/holos-aafc/Holos-5.git

# Navigate into the project directory
cd Holos-5
```

### 2. Repository Structure Overview

After cloning, you'll see this project structure:

```
Holos-5/
  H.Core/                        # Core business logic and calculations (carbon/N/animal math).
  H.Core.Test/                   # MSTest unit tests for H.Core.
  H.CLI/                         # Command-line bulk-analysis tool.
  H.CLI.Test/                    # MSTest tests for H.CLI.
  H.GUI.Avalonia/
    H.Avalonia/                  # Avalonia GUI (main entry point).
    H.Avalonia.Test/             # MSTest tests for the GUI VMs.
  H.Infrastructure/              # Shared helpers (KML parsing, network, units).
  H.Infrastructure.Test/         # Infrastructure tests.
  H.Localization/                # Translated resource strings (English + French).
  H.Integration/                 # Integration tests that touch real data files.
  H.Content/                     # CSVs, training material, and the Developer Guide docs.
  H.Economic.Data/               # Economics templates + helper docs.
  Holos.sln                      # Main solution file.
  ARCHITECTURE.md                # Application architecture guide.
  CODING_STYLE_GUIDE.md          # Coding standards and conventions.
  README.md                      # Project overview.
  H.Content/Documentation/Developer Guide/
    Developer_Guide_EN.md        # Quick-start developer reference.
    Carbon_Model_Flow.md         # End-to-end carbon-pipeline diagram + file index.
```

---

## Project Configuration

### 1. Open the Solution

**Visual Studio:**
1. Open Visual Studio
2. File → Open → Project/Solution
3. Navigate to the cloned repository
4. Open `Holos.sln`

**Visual Studio Code:**
1. Open VS Code
2. File → Open Folder
3. Select the `Holos-5` directory
4. VS Code will automatically detect the .NET solution

### 2. Restore NuGet Packages

The solution uses several NuGet packages. Restore them:

**Visual Studio:**
- Right-click on the solution in Solution Explorer
- Select "Restore NuGet Packages"

**Command Line:**
```bash
# From the repository root directory
dotnet restore
```

**Key Dependencies:**
- **Avalonia UI 11.0.2** - Cross-platform UI framework
- **Prism.Avalonia 8.1.97** - MVVM framework
- **Microsoft.Extensions.Logging** - Logging framework
- **NLog** - Logging implementation
- **DryIoc** - Dependency injection container

### 3. Verify Project References

Ensure all project references are properly loaded:
- **H.Avalonia** references: H.Core, H.Infrastructure, H.Content, H.Localization
- All test projects should reference their corresponding main projects.

---

## Building the Solution

### 1. Build Configuration

**Set Build Configuration:**
1. In Visual Studio, set the solution configuration to **Debug** for development
2. Set the platform to **Any CPU**
3. Set **H.Avalonia** as the startup project

### 2. Build the Solution

**Visual Studio:**
- Build → Build Solution (Ctrl+Shift+B)
- Or right-click solution → Build Solution

**Visual Studio Code:**
- C# Dev Kit registers a default `build` task. Run it via **Terminal → Run Build Task...**
  (`Ctrl+Shift+B`) and pick the .NET solution build entry.
- Or trigger the build from the Solution Explorer (.NET icon in the Activity Bar) — right-click
  the solution and choose **Build**.
- The integrated terminal (`` Ctrl+` ``) also runs the same `dotnet build` commands listed
  below — useful when you want to scope a build to a single project.

**Command Line (works equally well from any IDE's integrated terminal):**
```bash
# Build the entire solution
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Build specific project
dotnet build H.GUI.Avalonia/H.Avalonia/H.Avalonia.csproj
```

### 3. Verify Successful Build

A successful build should:
- Show "Build succeeded" in the output window
- Have no compilation errors
- Generate output assemblies in `bin/Debug/net9.0/` (the projects target `net9.0`, not `net9.0-windows`).

---

## Running the Application

### 1. Set Startup Project

**Visual Studio:**
1. Right-click on **H.Avalonia** project in Solution Explorer.
2. Select "Set as StartUp Project".

**Visual Studio Code:**
- VS Code doesn't use a sticky "startup project". Instead, the active run target is whichever
  configuration is currently selected in the **Run and Debug** dropdown (`Ctrl+Shift+D`).
  After you create `.vscode/launch.json` (template in the VS Code install section above),
  select `Launch H.Avalonia (Debug)` from that dropdown.

### 2. Run the Application

**Visual Studio:**
- Press **F5** (Debug mode) or **Ctrl+F5** (Release mode).
- Or click the "Start" button in the toolbar.

**Visual Studio Code:**
- Press **F5** with `Launch H.Avalonia (Debug)` selected in the Run and Debug view. VS Code
  runs the `build-avalonia` pre-launch task from `tasks.json`, then attaches the debugger to
  the freshly-built assembly. Output (Avalonia logs, NLog lines) lands in the Debug Console.
- For a no-debug run, use **Ctrl+F5** in the Run and Debug view, or just use the command
  line:
  ```bash
  dotnet run --project H.GUI.Avalonia/H.Avalonia/H.Avalonia.csproj
  ```

**Command Line (any IDE, or no IDE):**
```bash
# Run from the main GUI project directory
cd H.GUI.Avalonia/H.Avalonia
dotnet run

# Or from solution root
dotnet run --project H.GUI.Avalonia/H.Avalonia/H.Avalonia.csproj
```

### 3. Application Startup Flow

When the application starts:
1. **Dependency Injection Setup** - Services and views are registered
2. **Logging Configuration** - NLog is initialized
3. **Storage Initialization** - Data storage is configured
4. **Main Window Creation** - The primary UI window opens
5. **Region Registration** - UI regions are set up for navigation

---

## Development Workflow

### 1. Branching Strategy

Follow this Git workflow:

```bash
# Create a new feature branch
git checkout -b feature/your-feature-name

# Make your changes and commit
git add .
git commit -m "Add: Brief description of your changes"

# Push to your branch
git push origin feature/your-feature-name

# Create pull request through GitHub
```

### 2. Code Standards

Follow the established conventions documented in `CODING_STYLE_GUIDE.md`:

- Use **PascalCase** for classes, methods, and properties
- Use **camelCase** for local variables and parameters  
- Use **_camelCase** for private fields
- Document public APIs with XML comments
- Include equation numbers for scientific calculations
- Follow the region organization pattern

### 3. Testing

**Visual Studio:**
- **Test → Test Explorer** discovers both `H.Core.Test` and `H.Avalonia.Test` automatically.
- Run a single test by right-clicking it; debug a single test by right-clicking → **Debug**.

**Visual Studio Code:**
- The **Testing** view (flask icon in the Activity Bar) shows the discovered test tree once
  C# Dev Kit finishes indexing.
- Click the inline gutter "play" icon next to any `[TestMethod]` to run just that test.
- Click the "play with bug" icon to run it under the debugger — breakpoints in test code or
  in the code under test will hit.

**Command Line (any IDE):**
```bash
# Run all tests in a project
dotnet test H.Core.Test/H.Core.Test.csproj

# Run a single test class
dotnet test H.Core.Test/H.Core.Test.csproj --filter "FullyQualifiedName~FarmAnalysisServiceTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Structure:**
- Tests are organized by project (H.Core.Test, H.Avalonia.Test, etc.)
- Follow AAA pattern (Arrange, Act, Assert)
- Include equation references in scientific calculation tests

### 4. Debugging

**Visual Studio:**
1. Set breakpoints by clicking in the left margin.
2. Press **F5** to start debugging.
3. Use **F10** (Step Over) and **F11** (Step Into) for stepping.
4. Use the **Immediate Window** for expression evaluation.
5. NLog output appears in **Debug → Windows → Output**, "Debug" pane.

**Visual Studio Code:**
1. Set breakpoints by clicking in the gutter next to the line numbers.
2. Open the **Run and Debug** view (`Ctrl+Shift+D`), pick `Launch H.Avalonia (Debug)` (or
   `Launch H.CLI (Debug)`) from the dropdown, and press **F5**. The pre-launch task in
   `tasks.json` rebuilds the project before attach.
3. Stepping shortcuts mirror Visual Studio: **F10** (Step Over), **F11** (Step Into),
   **Shift+F11** (Step Out).
4. The **Debug Console** at the bottom shows NLog output + the live `dotnet` process
   stderr/stdout. It also accepts C# expressions evaluated against the current stack frame —
   the VS Code equivalent of Visual Studio's Immediate Window.
5. Variable inspection and the call stack live in the Run and Debug sidebar on the left
   while paused at a breakpoint.

**Logging:**
- Every class in the codebase logs through **NLog** with a single unified format:
  `HH:mm:ss.ffff [LEVEL] [Class.Method] message`. NLog config lives at
  `H.GUI.Avalonia/H.Avalonia/NLog.config`.
- Use `ILogger` (injected via DI) when the class is constructed by the container; use a
  static `private static readonly Logger _log = LogManager.GetCurrentClassLogger();` field
  when it isn't (providers, helpers, partial classes). Both route through the same NLog
  pipeline.
- **Do not use `System.Diagnostics.Trace.*`** — the codebase migrated off Trace in favour of
  NLog so the Output window doesn't show two competing log formats. Full rationale in the
  "Logging" section of `CODING_STYLE_GUIDE.md`.
- Logs land in three places at once: console (Debug+), VS Output > Debug pane (Debug+, only
  when a debugger is attached), and `logs/app-<YYYY-MM-DD>.log` under the run directory (Info+).

---

## Troubleshooting

### Common Issues and Solutions

**Build Errors:**

1. **"The target framework 'net9.0' is not supported"**
   - Solution: Ensure the .NET 9 SDK is properly installed (not just the runtime).
   - Verify with: `dotnet --list-sdks` — you should see `9.0.x`.

2. **NuGet package restore failures**
   - Solution: Clear NuGet cache
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

3. **Avalonia designer not working**
   - In Visual Studio: install the "Avalonia for Visual Studio" extension and restart.
   - In VS Code: the "Avalonia for VSCode" extension provides syntax highlighting + completion
     but does not include a full live previewer. This is expected — open the affected
     `.axaml` in Visual Studio if you need the live design surface.

4. **VS Code Solution Explorer is empty / tests aren't discovered**
   - Wait for the Roslyn / C# Dev Kit indexer to finish — first-open indexing on the full
     solution takes 30–90 seconds. The status bar at the bottom shows progress.
   - If indexing finishes and the Solution Explorer is still empty, run
     `> .NET: Reload Projects` from the Command Palette (`Ctrl+Shift+P`).

**Runtime Errors:**

1. **Application doesn't start**
   - Check the Output window for error messages
   - Verify H.Avalonia is set as startup project
   - Ensure all dependencies are properly restored

2. **Missing dependencies**
   - Run `dotnet restore` from the solution directory
   - Check for package conflicts in Package Manager

**Git Issues:**

1. **Authentication problems**
   - Use GitHub Desktop or configure Git credentials
   - Consider using SSH keys for authentication

2. **Merge conflicts**
   - Use Visual Studio's merge conflict resolution tools
   - Or resolve manually and commit the resolution

### Getting Help

1. **Internal Documentation**
   - `ARCHITECTURE.md` — overall application structure and bootstrap flow.
   - `CODING_STYLE_GUIDE.md` — coding standards, the Avalonia binding pitfall, the logging pattern.
   - `H.Content/Documentation/Developer Guide/Developer_Guide_EN.md` — quick-start reference for VS / VS Code / Rider, dotnet CLI commands, solution layout.
   - `H.Content/Documentation/Developer Guide/Carbon_Model_Flow.md` — end-to-end Mermaid diagram of the carbon analysis pipeline plus a class-by-class file index. Start here before touching the carbon or nitrogen calculators.
   - Examine existing code for patterns and examples — the ~60 files most central to the carbon pipeline have detailed class-level docstrings naming their role and collaborators.

2. **External Resources**
   - [Avalonia Documentation](https://docs.avaloniaui.net/)
   - [Prism Documentation](https://prismlibrary.com/)
   - [.NET 9 Documentation](https://docs.microsoft.com/dotnet/)

3. **Team Resources**
   - Reach out to team members for code reviews
   - Use GitHub issues for bug reports and feature requests
   - Participate in team code review process

---

## Additional Resources

### Optional Tools

The Holos project does not require or officially endorse any third-party productivity
tools. The tools below are simply examples that some contributors have found useful — none
of them are needed to build, run, or test the application, and using them is entirely a
personal choice. The supported build path is `dotnet build` / `dotnet test` from any of
the three IDEs covered above.

**Examples some contributors use:**
- **JetBrains ReSharper / Rider** — third-party code analysis and refactoring tooling.
- **Visual Studio IntelliCode** — Microsoft's AI-assisted code completion.
- **GitHub Desktop** — graphical Git interface; built-in Git in any of the three supported IDEs is also sufficient.

If you choose to use any of these, treat their suggestions (renames, refactorings,
formatting changes) as advisory only — the project's authoritative style is what's in
[`CODING_STYLE_GUIDE.md`](CODING_STYLE_GUIDE.md).



### Documentation Links

- **Project Architecture**: [`ARCHITECTURE.md`](ARCHITECTURE.md)
- **Coding Standards**: [`CODING_STYLE_GUIDE.md`](CODING_STYLE_GUIDE.md)
- **Developer Quick-Start (VS / VS Code / Rider)**: [`Developer_Guide_EN.md`](H.Content/Documentation/Developer%20Guide/Developer_Guide_EN.md)
- **Carbon Pipeline Flowchart**: [`Carbon_Model_Flow.md`](H.Content/Documentation/Developer%20Guide/Carbon_Model_Flow.md)
- **Avalonia UI**: https://docs.avaloniaui.net/
- **Prism Framework**: https://prismlibrary.com/docs/
- **.NET 9**: https://docs.microsoft.com/dotnet/



---

## Next Steps

After completing this setup:

1. **Read the Architecture Guide** — understand the application structure by reviewing [`ARCHITECTURE.md`](ARCHITECTURE.md).
2. **Skim the Developer Guide** — [`H.Content/Documentation/Developer Guide/Developer_Guide_EN.md`](H.Content/Documentation/Developer%20Guide/Developer_Guide_EN.md) covers the day-to-day workflow (build / run / test / debug for each supported IDE) and the localization + logging patterns.
3. **Trace the Carbon Pipeline** — open [`H.Content/Documentation/Developer Guide/Carbon_Model_Flow.md`](H.Content/Documentation/Developer%20Guide/Carbon_Model_Flow.md). The Mermaid flowchart shows every step from "user authors a wheat field" through to "GHG results chart renders", and the file index at the bottom maps each step to its implementation. Essential context before touching carbon or nitrogen code.
4. **Study the Coding Standards** — [`CODING_STYLE_GUIDE.md`](CODING_STYLE_GUIDE.md). Pay attention to the Avalonia `StringFormat` pitfall and the logging pattern (ILogger / NLog Logger, never `Trace.*`).
5. **Explore the Codebase** — start with `App.axaml.cs` (bootloader) to understand DI registration. The ~60 files central to the carbon pipeline have detailed class-level docstrings naming their role and collaborators.
6. **Run the Application** — get familiar with the UI and basic functionality.
7. **Set Up Your First Development Task** — choose a small feature or bug fix to start with.
8. **Join the Development Process** — participate in code reviews and team discussions.

Welcome to the Holos development team!