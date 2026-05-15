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

### 2. Install Visual Studio 2022 (Recommended)

**Visual Studio 2022 Community/Professional/Enterprise:**

1. Download from [Visual Studio website](https://visualstudio.microsoft.com/downloads/)
2. During installation, select these workloads:
   - **.NET Desktop Development**
   - **ASP.NET and web development** (for additional tools)
   
3. In the **Individual Components** tab, ensure you have:
   - **.NET 9.0 Runtime (Long Term Support)**
   - **NuGet package manager**
   - **Git for Windows** (if not already installed)
   - **GitHub Extension for Visual Studio** (optional but helpful)

**Required Extensions:**
After Visual Studio installation, install these extensions:
- **Avalonia for Visual Studio** - For XAML editing support
- **NLog Language Service** - For NLog configuration support

### 3. Alternative: Visual Studio Code Setup

If you prefer VS Code:

1. Install [Visual Studio Code](https://code.visualstudio.com/)
2. Install these essential extensions:
   - **C# Dev Kit**
   - **.NET Install Tool**
   - **Avalonia for Visual Studio Code**
   - **NuGet Gallery**
   - **GitLens**

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
- **H.Avalonia** references: H.Core, H.Infrastructure, H.Content, H.Avalonia.Core, H.Avalonia.Infrastructure
- All test projects should reference their corresponding main projects

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

**Command Line:**
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
1. Right-click on **H.Avalonia** project in Solution Explorer
2. Select "Set as StartUp Project"

### 2. Run the Application

**Visual Studio:**
- Press **F5** (Debug mode) or **Ctrl+F5** (Release mode)
- Or click the "Start" button in the toolbar

**Command Line:**
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

**Run Unit Tests:**
```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test H.Core.Test/H.Core.Test.csproj

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Structure:**
- Tests are organized by project (H.Core.Test, H.Avalonia.Test, etc.)
- Follow AAA pattern (Arrange, Act, Assert)
- Include equation references in scientific calculation tests

### 4. Debugging

**Visual Studio Debugging:**
1. Set breakpoints by clicking in the left margin
2. Press F5 to start debugging
3. Use F10 (Step Over) and F11 (Step Into) for stepping
4. Use the Immediate Window for expression evaluation

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
   - Solution: Install "Avalonia for Visual Studio" extension
   - Restart Visual Studio after installation

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

### Recommended Tools

**Development Productivity:**
- **ReSharper** - Code analysis and refactoring
- **Visual Studio IntelliCode** - AI-assisted coding


**Version Control:**
- **GitHub Desktop** - User-friendly Git interface



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