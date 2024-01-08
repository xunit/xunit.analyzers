# Building xUnit.net Analyzers

The primary build system for xUnit.net Analyzers is done via command line, and officially supports Linux and Windows. Users
running macOS can generally follow the Linux instructions (while installing the macOS equivalents of the dependencies).

# Pre-Requisites

You will need the following software installed (regardless of OS):

* [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
* [git](https://git-scm.com/downloads)

## Linux Pre-Requisites

Linux users will additionally need:

* [Mono](https://www.mono-project.com/download/stable/) 6.12+
* [bash](https://www.gnu.org/software/bash/)

Note: Linux users cannot run the .NET Framework tests, as they are incompatible. For this reason, we recommend that
users either work primarily in Windows, or verify their tests work as expected in a Windows VM, before submitting PRs.

## Windows Pre-Requisites

Windows users will additionally need:

* .NET Framework 4.7.2 or later (part of the Windows OS)
* PowerShell (or [PowerShell Core](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-6))

Ensure that you have configured PowerShell to be able to run local unsigned scripts (either by running
`Set-ExecutionPolicy RemoteSigned` from within PowerShell, or by launching PowerShell with the
`-ExecutionPolicy RemoteSigned` command line switch).

# Command-Line Build

1. **Linux users:** Open a terminal to your favorite shell.

    **Windows users:** Open PowerShell (or PowerShell Core).

1. From the root folder of the source repo, this command will build the code & run all tests:

    `./build`

    To build a specific target (or multiple targets):

    `./build [target [target...]]`

    The common targets (case-insensitive) include:

    * `Restore`: Perform package restore
    * `Build`: Build the source
    * `Test`: Run all unit tests
    * `TestCore`: Run all unit tests (.NET Core)
    * `TestFx`: Run all unit tests (.NET Framework)
    * `Packages`: Create NuGet packages

    You can get a list of options:

    `./build --help`

# Editing source

In order to support multiple versions of Roslyn (the compiler API), we have created separated projects for backward
compatibility. The primary projects targeting the latest supported version of Roslyn are:

* `xunit.analyzers` (for code analysis)
* `xunit.analyzers.fixes` (for automated fixes for issues raised in code analysis)
* `xunit.analyzers.tests` (for unit tests of both above projects)

When working on new or existing analyzers, fixes, and tests, please work in these projects. They reside in the folder
with the source code for the appropriate project.

You will also see several backward compatibility projects, with `roslyn` in their name, to indicate which older
version of the Roslyn API they target (for example, `xunit.analyzers.roslyn311` targets Roslyn 3.11, which
[supports Visual Studio 2019 16.11](https://learn.microsoft.com/en-us/visualstudio/extensibility/roslyn-version-support)).
These projects automatically pick up source files from the main projects. It is expected that the single set of
source code applies to all compiled projects.

If code needs to be conditioned based on the version of Roslyn, there are two categories of preprocessor definitions
that you can use to gate your code and/or tests. For exact versions, you can reference a specific version like
`ROSLYN_3_11`, and for minimum versions, you can reference a symbol like `ROSLYN_3_11_OR_GREATER`. Note that we only
provide definitions for the exact versions we support. For a list of those symbols and supported versions,
please see [`Directory.Build.props`](https://github.com/xunit/xunit.analyzers/blob/main/src/Directory.Build.props).

While iterating in the IDE, it will be common to work just inside the primary projects, including just running the tests
from the primary test project. Before submitting a PR, please run `./build test` (from Windows) so that you can ensure
that all code &amp; tests work with all supported versions of Roslyn.
