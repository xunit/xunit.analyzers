# Building xUnit.net Analyzers

The primary build system for xUnit.net Analyzers is done via command line, and officially supports Linux and Windows. Users running macOS can generally follow the Linux instructions (while installing the macOS equivalents of the dependencies).

# Pre-Requisites

You will need the following software installed (regardless of OS):

* [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
* [git](https://git-scm.com/downloads)

## Linux Pre-Requisites

Linux users will additionally need:

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

These are targeting our lowest common denominator for Roslyn (current version 4.12, the version that's supported in Visual Studio 2022 17.12).

There are also three projects which build against the latest version of Roslyn:

* `xunit.analyzers.latest`
* `xunit.analyzers.latest.fixes`
* `xunit.analyzers.latest.tests`

When running a command line build, we run a matrix of 4 test projects: Roslyn 4.12 vs. latest, and .NET Framework vs. .NET. It's important that you run `./build` (or `./build test`) from Windows before submitting PRs, because some bugs are often found only in one of the four combinations (and Linux cannot run the .NET Framework tests).

You will also occasionally see tests which only run in specific environments. Common `#if` statements you may see (or may need to use) include:

* `#if NETFRAMEWORK` (only runs for .NET Framework)
* `#if NETCOREAPP` (only runs for .NET)
* `#if ROSLYN_LATEST` (only runs with latest Roslyn, for C# language version 13+)

In production code, we try to minimize these when possible, and prefer to fall back to use dynamic runtime environment detection when we can (as we'd like to light up features in newer versions of Roslyn when available). While this isn't always possible, it is generally a goal we try to achieve. In test code, we tend to use these to more frequently to ensure we have complete coverage of features that should be available dynamically (whether they are lit up based on `#if` or by runtime environment detection).

# Adding tests

In general, the structure of the test project here is "have as few tests as possible". You'll find that most test files have just a couple tests in them, and they tend to be extremely large tests which test many scenarios at once.

The reason for this is that the Roslyn testing framework we use has absolutely awful "test initialization" cost. You are greatly rewarded for few large tests vs. many small tests. Things you will see here that you wouldn't necessarily see anywhere else in our source base:

* Extremely large tests that test many scenarios at once
* Tests oriented around test environments (rather than oriented around scenarios, state, bugs, etc.)
* Tests that duplicate code (rather than using templates and/or data-driven testing)

The one notable exception you'll see is when the thing under test cannot have multiple scenarios combined into the same test (the most common of which is testing assembly-level attributes which don't allow multiples).

This was a massive effort to undo our typical testing behavior, and the reward was a test suite that runs 4x faster on our local builds, and 2x faster in CI. We are keeping very close control over test growth in this project by ensuring that new test scenarios are added to existing tests whenever possible. This means that if you are adding a new test method, there's an extremely high chance you're undo our effort. Make sure you've ruled out the possibility of adding your test scenario. Don't be surprised (or offended) if we ask you to change the test code (or we just change it for you before merging the PR).
