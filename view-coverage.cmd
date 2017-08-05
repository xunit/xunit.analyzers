@if not defined _echo @echo off
setlocal EnableDelayedExpansion

:: Please keep the OpenCover and ReportGenerator versions in sync with those in xunit.analyzers.tests.csproj.

set PackageRoot="%USERPROFILE%\.nuget\packages"
set OpenCoverVersion=4.6.519
set OpenCoverPath="%PackageRoot%\OpenCover\%OpenCoverVersion%\tools\OpenCover.Console.exe"
set DotNetPath="%PROGRAMFILES%\dotnet\dotnet.exe"

set ReportGeneratorVersion=2.5.10
set ReportGeneratorPath="%PackageRoot%\ReportGenerator\%ReportGeneratorVersion%\tools\ReportGenerator.exe"
set ReportDirectory="%~dp0reports"

pushd "%~dp0test"

for /d %%d in (*.tests) do (
    pushd "%%d"

    %OpenCoverPath% -target:%DotNetPath% -targetargs:"test" -register:user -filter:"+[*]*" -oldStyle

    set TestAssembly="%%d"
    set TargetDirectory="%ReportDirectory:"=%\!TestAssembly:~1,-1!"
    set ReportFile="!TargetDirectory:~1,-1!\index.htm"

    %ReportGeneratorPath% -reports:results.xml -targetdir:!TargetDirectory!
    start !ReportFile:~1,-1!

    popd
)

popd
