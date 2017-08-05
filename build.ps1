param(
    [string] $target = "test",
    [string] $configuration = "Release",
    [string] $buildAssemblyVersion = "",
    [string] $buildSemanticVersion = ""
)

if ($PSScriptRoot -eq $null) {
    fatal "This build script requires PowerShell 3 or later."
}

$buildModuleFile = join-path $PSScriptRoot "build\tools\xunit-build-module.psm1"

if ((test-path $buildModuleFile) -eq $false) {
    write-host "Could not find build module. Did you forget to 'git submodule update --init'?" -ForegroundColor Red
    exit -1
}

Set-StrictMode -Version 2
Import-Module $buildModuleFile -Scope Local -Force -ArgumentList "4.1.0"
Set-Location $PSScriptRoot

$packageOutputFolder = (join-path (Get-Location) "artifacts\packages")
$parallelFlags = "-parallel all -maxthreads 16"
$testOutputFolder = (join-path (Get-Location) "artifacts\test")

# Helper functions

function _xunit_console([string] $command) {
    _exec ('& "' + $PSScriptRoot + '\packages\xunit.runner.console\tools\net452\xunit.console.x86.exe" ' + $command)
}

# Top-level targets

function __target_appveyor() {
    __target_ci
    __target__pushmyget
}

function __target_build() {
    __target_packagerestore

    _build_step "Compiling binaries"
        _msbuild "xunit.analyzers.sln" $configuration
}

function __target_ci() {
    __target__setversion
    __target_test
    __target__packages
}

function __target_packagerestore() {
    _download_nuget

    _build_step "Restoring NuGet packages"
        _mkdir packages
        _exec ('& "' + $nugetExe + '" restore xunit.analyzers.sln -NonInteractive')
        _exec ('& "' + $nugetExe + '" install xunit.runner.console -OutputDirectory "' + (Join-Path $PSScriptRoot "packages") + '" -NonInteractive -pre -ExcludeVersion')
}

function __target_packages() {
    __target_build
    __target__packages
}

function __target_test() {
    __target_build
    __target__test
}

# Dependent targets

function __target__packages() {
    _download_nuget

    _build_step "Creating NuGet packages"
        Get-ChildItem -Recurse -Filter *.nuspec | _nuget_pack -outputFolder $packageOutputFolder -configuration $configuration
}

function __target__pushmyget() {
    _build_step "Pushing packages to MyGet"
        if ($env:MyGetApiKey -eq $null) {
            Write-Host -ForegroundColor Yellow "Skipping MyGet push because environment variable 'MyGetApiKey' is not set."
            Write-Host ""
        } else {
            Get-ChildItem -Filter *.nupkg $packageOutputFolder | _nuget_push -source https://www.myget.org/F/xunit/api/v2/package -apiKey $env:MyGetApiKey
        }
}

function __target__setversion() {
    if ($buildAssemblyVersion -ne "") {
        _build_step ("Setting assembly version: '" + $buildAssemblyVersion + "'")
            Get-ChildItem -Recurse -Filter AssemblyInfo.cs | _replace -match '\("99\.99\.99\.0"\)' -replacement ('("' + $buildAssemblyVersion + '")')
    }

    if ($buildSemanticVersion -ne "") {
        _build_step ("Setting semantic version: '" + $buildSemanticVersion + "'")
            Get-ChildItem -Recurse -Filter AssemblyInfo.cs | _replace -match '\("99\.99\.99-dev"\)' -replacement ('("' + $buildSemanticVersion + '")')
            Get-ChildItem -Recurse -Filter *.nuspec | _replace -match '99\.99\.99-dev' -replacement $buildSemanticVersion
    }
}

function __target__test() {
    _build_step "Running unit tests"
        _xunit_console ("test\xunit.analyzers.tests\bin\" + $configuration + "\netcoreapp1.1\xunit.analyzers.tests.dll -xml artifacts\test\TestResults.xml -diagnostics")
}

# Dispatch

$targetFunction = (Get-ChildItem ("Function:__target_" + $target.ToLowerInvariant()) -ErrorAction SilentlyContinue)
if ($targetFunction -eq $null) {
    _fatal "Unknown target '$target'"
}

_build_step "Performing pre-build verifications"
    _require dotnet "Could not find 'dotnet'. Please ensure .NET CLI Tooling is installed."
    _require msbuild "Could not find 'msbuild'. Please ensure MSBUILD.EXE v15 is on the path."
    _verify_msbuild15

_mkdir $packageOutputFolder
_mkdir $testOutputFolder
& $targetFunction
