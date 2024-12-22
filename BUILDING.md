# Building xUnit.net Analyzers

The primary build system for xUnit.net Analyzers is done via command line, and officially supports Linux and Windows. Users running macOS can generally follow the Linux instructions (while installing the macOS equivalents of the dependencies).

# Pre-Requisites

You will need the following software installed (regardless of OS):

* [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
* [git](https://git-scm.com/downloads)

## Linux Pre-Requisites

Linux users will additionally need:

* [Mono](https://www.mono-project.com/download/stable/) 6.12+
* [bash](https://www.gnu.org/software/bash/)

Note: Linux users cannot run the .NET Framework tests, as they are incompatible. For this reason, we recommend that users either work primarily in Windows, or verify their tests work as expected in a Windows VM, before submitting PRs.

## Windows Pre-Requisites

Windows users will additionally need:

* .NET Framework 4.7.2 or later (part of the Windows OS)
* [PowerShell 7+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-windows)

Ensure that you have configured PowerShell to be able to run local unsigned scripts (either by running `Set-ExecutionPolicy RemoteSigned` from within PowerShell, or by launching PowerShell with the `-ExecutionPolicy RemoteSigned` command line switch).

_Note that the built-in version of PowerShell may work, but is unsupported by us. If you have PowerShell-related issues, please make sure you have installed PowerShell 7+ and the command prompt you opened is for PowerShell 7+, and not the built-in version of PowerShell._

# Command-Line Build

1. **Linux users:** Open a terminal to your favorite shell.

    **Windows users:** Open PowerShell 7+.

1. From the root folder of the source repo, this command will build the code & run all tests:

    `./build`

    To build a specific target (or multiple targets):

    `./build [target [target...]]`

    The common targets (case-insensitive) include:

    * `Restore`: Perform package restore
    * `Build`: Build the source
    * `Test`: Run all unit tests

    You can get a list of options:

    `./build --help`

# Editing source

The primary projects for editing are:

* `xunit.analyzers` (for code analysis)
* `xunit.analyzers.fixes` (for automated fixes for issues raised in code analysis)
* `xunit.analyzers.tests` (for unit tests of both above projects)

These are targeting our lowest common denominator for Roslyn (current version 3.11, the version that's supported in Visual Studio 2019 16.11).

There are also three projects which build against the latest version of Roslyn:

* `xunit.analyzers.latest`
* `xunit.analyzers.latest.fixes`
* `xunit.analyzers.latest.tests`

When running a command line build, we run a matrix of 4 test projects: Roslyn 3.11 vs. latest, and .NET Framework vs. .NET. It's important that you run `./build` (or `./build test`) from Windows before submitting PRs, because some bugs are often found only in one of the four combinations (and Mono cannot run the .NET Framework tests).

You will also occasionally see tests which only run in specific environments. Common `#if` statements you may see (or may need to use) include:

* `#if NETFRAMEWORK` (only runs for .NET Framework)
* `#if NETCOREAPP` (only runs for .NET)
* `#if ROSLYN_LATEST` (only runs with latest Roslyn, which includes getting analysis test support to C# language > version 9)

In production code, we try to minimize these when possible, and prefer to fall back to use dynamic runtime environment detection when we can (as we'd like to light up features in newer versions of Roslyn when available). While this isn't always possible, it is generally a goal we try to achieve. In test code, we tend to use these to more frequently to ensure we have complete coverage of features that should be available dynamically (whether they are lit up based on `#if` or by runtime environment detection).
