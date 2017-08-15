
## About xUnit.net Analyzers

[<img align="right" src="https://xunit.github.io/images/dotnet-fdn-logo.png" width="100" />](https://www.dotnetfoundation.org/)

xUnit.net is a free, open source, community-focused unit testing tool for the .NET Framework. Written by the original inventor of NUnit v2, xUnit.net is the latest technology for unit testing C#, F#, VB.NET and other .NET languages. xUnit.net works with ReSharper, CodeRush, TestDriven.NET and Xamarin. It is part of the [.NET Foundation](https://www.dotnetfoundation.org/), and operates under their [code of conduct](https://www.dotnetfoundation.org/code-of-conduct). It is licensed under [Apache 2](https://opensource.org/licenses/Apache-2.0) (an OSI approved license).

This project contains source code analysis and cleanup rules for xUnit.net. It supports xUnit.net v2.0+ and Visual Studio 2015 Update 2 and above.

To start using the analyzers in your test project, simply add a reference to the [xunit.analyzers NuGet package](https://www.nuget.org/packages/xunit.analyzers/).

To open an issue for this project, please visit the [core xUnit.net project issue tracker](https://github.com/xunit/xunit/issues).

To build the project, you will need Visual Studio 2017. The VSIX project can be set as your startup project, to debug the analyzers inside a special instance of Visual Studio.

## Analysis and Code Fix in Action

![Analyzer in action animation](https://cloud.githubusercontent.com/assets/607223/25752060/fb4af444-316b-11e7-9e7c-fc69ade132fb.gif)

## Build Status

Channel  | Status
-------- | :-------:
CI |  <a href="https://ci.appveyor.com/project/xunit/xunit.analyzers"><img src="https://ci.appveyor.com/api/projects/status/qvurc9j02j8a8qy4/branch/master?svg=true" /></a>
NuGet | <a href="https://www.nuget.org/packages/xunit.analyzers/"><img src="https://img.shields.io/nuget/v/xunit.analyzers.svg?style=flat)" /></a>
MyGet<br>([gallery](https://www.myget.org/gallery/xunit/)) | <a href="https://www.myget.org/feed/xunit/package/nuget/xunit.analyzers"><img src="https://img.shields.io/myget/xunit/vpre/xunit.analyzers.svg?style=flat)"/></a>

## Supported Rules

Take a look at [Descriptors.cs](https://github.com/xunit/xunit.analyzers/blob/master/src/xunit.analyzers/Descriptors.cs) for a list of supported rules.
