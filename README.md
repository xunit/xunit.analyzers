# About the project

[![Build Status](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2Fxunit%2Fxunit.analyzers%2Fbadge&style=flat-square&label=build)](https://actions-badge.atrox.dev/xunit/xunit.analyzers/goto)
[![NuGet Package](https://img.shields.io/nuget/v/xunit.analyzers.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/xunit.analyzers/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/xunit.analyzers.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/xunit.analyzers/)
[![MyGet Preview Package](https://img.shields.io/myget/xunit/vpre/xunit.analyzers.svg?style=flat-square&label=myget&logo=nuget)](https://www.myget.org/feed/xunit/package/nuget/xunit.analyzers)

This project contains source code analysis and cleanup rules for xUnit.net.

**Requirements**: xUnit.net v2.0+ and Visual Studio 2015 Update 2 or later.

**Documentation**: a list of supported rules is available at https://xunit.github.io/xunit.analyzers/

**Bugs and issues**: please visit the [core xUnit.net project issue tracker](https://github.com/xunit/xunit/issues).

To build the project, you will need Visual Studio 2017. The VSIX project can be set as your startup project, to debug the analyzers inside a special instance of Visual Studio.

## How to install

- xUnit.net 2.3.0 and higher: the analyzer package is referenced by the main [`xunit` NuGet package](https://www.nuget.org/packages/xunit) out of the box

- xUnit.net 2.2.0 and earlier: you have to install the [`xunit.analyzers` NuGet package](https://www.nuget.org/packages/xunit.analyzers) explicitly

## How to uninstall

If you installed xUnit.net 2.3.0 or higher and do not wish to use the analyzers package, replace the package reference to [`xunit`](https://www.nuget.org/packages/xunit) with the correspoding versions of [`xunit.core` ](https://www.nuget.org/packages/xunit.core) and [`xunit.assert`](https://www.nuget.org/packages/xunit.assert)

## Analysis and Code Fix in Action

![Analyzer in action animation](https://cloud.githubusercontent.com/assets/607223/25752060/fb4af444-316b-11e7-9e7c-fc69ade132fb.gif)

# About xUnit.net

[<img align="right" src="https://xunit.github.io/images/dotnet-fdn-logo.png" width="100" />](https://www.dotnetfoundation.org/)

xUnit.net is a free, open source, community-focused unit testing tool for the .NET Framework. Written by the original inventor of NUnit v2, xUnit.net is the latest technology for unit testing C#, F#, VB.NET and other .NET languages. xUnit.net works with ReSharper, CodeRush, TestDriven.NET and Xamarin. It is part of the [.NET Foundation](https://www.dotnetfoundation.org/), and operates under their [code of conduct](https://www.dotnetfoundation.org/code-of-conduct). It is licensed under [Apache 2](https://opensource.org/licenses/Apache-2.0) (an OSI approved license).
