#!/usr/bin/env pwsh
#Requires -Version 5.1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($null -eq (Get-Command "dotnet" -ErrorAction Ignore)) {
    throw "Could not find 'dotnet'; please install the  .NET Core SDK"
}

Push-Location (Split-Path $MyInvocation.MyCommand.Definition)

try {
    & dotnet run --project tools/builder --no-launch-profile -- $args
    if (-not $?) {
        Exit $LASTEXITCODE
    }
}
finally {
    Pop-Location
}
