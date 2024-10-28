# About This Project

This project contains source code analysis and cleanup rules for xUnit.net. Analysis and fixes are only supported with C#.

**Requirements**: xUnit.net v2 2.0+. Supported in Visual Studio 2019 16.11+ or 2022 17.4+ (as well as via command line builds with Roslyn 3.11+).
Other environments (such as Mono or JetBrains Rider) may be able to use these analyzers as well; support and issue resolution will be provided by
those third parties and not by xUnit.net itself.

**Documentation**: a list of supported rules is available at https://xunit.net/xunit.analyzers/rules/

**Bugs and issues**: please visit the [core xUnit.net project issue tracker](https://github.com/xunit/xunit/issues).

**Building**: see [BUILDING.md](BUILDING.md).

## How to install

- xUnit.net v2 2.3.0 and higher: the analyzer package is referenced by the main [`xunit` NuGet package](https://www.nuget.org/packages/xunit) out of the box.
If you choose to reference [`xunit.core`](https://www.nuget.org/packages/xunit.core) instead, you can reference
[`xunit.analyzers`](https://www.nuget.org/packages/xunit.analyzers) explicitly.

- xUnit.net v2 2.2.0 and earlier: you have to install the [`xunit.analyzers` NuGet package](https://www.nuget.org/packages/xunit.analyzers) explicitly.

## How to uninstall

If you installed xUnit.net v2 2.3.0 or higher and do not wish to use the analyzers package, replace the package reference
to [`xunit`](https://www.nuget.org/packages/xunit) with the corresponding versions of [`xunit.core`](https://www.nuget.org/packages/xunit.core)
and [`xunit.assert`](https://www.nuget.org/packages/xunit.assert)

## Analysis and Code Fix in Action

![Analyzer in action animation](https://cloud.githubusercontent.com/assets/607223/25752060/fb4af444-316b-11e7-9e7c-fc69ade132fb.gif)

## Latest Builds

|                           | Stable                                                                                                                                   | Latest CI ([how to use](/docs/using-ci-builds))                                                                                                                                                             | Build status
| ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------
| v3 core framework         | [![](https://img.shields.io/nuget/v/xunit.v3.svg?logo=nuget)](https://www.nuget.org/packages/xunit.v3)                                   | [![](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/xunit/xunit/shield/xunit.v3/latest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit.v3)                                   | [![](https://img.shields.io/endpoint.svg?url=https://actions-badge.atrox.dev/xunit/xunit/badge%3Fref%3Dmain&amp;label=build)](https://actions-badge.atrox.dev/xunit/xunit/goto?ref=main)
| v2 core framework         | [![](https://img.shields.io/nuget/v/xunit.svg?logo=nuget)](https://www.nuget.org/packages/xunit)                                         | [![](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/xunit/xunit/shield/xunit/latest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit)                                         | [![](https://img.shields.io/endpoint.svg?url=https://actions-badge.atrox.dev/xunit/xunit/badge%3Fref%3Dv2&amp;label=build)](https://actions-badge.atrox.dev/xunit/xunit/goto?ref=v2)
| xunit.analyzers           | [![](https://img.shields.io/nuget/v/xunit.analyzers.svg?logo=nuget)](https://www.nuget.org/packages/xunit.analyzers)                     | [![](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/xunit/xunit/shield/xunit.analyzers/latest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit.analyzers)                     | [![](https://img.shields.io/endpoint.svg?url=https://actions-badge.atrox.dev/xunit/xunit.analyzers/badge%3Fref%3Dmain&amp;label=build)](https://actions-badge.atrox.dev/xunit/xunit.analyzers/goto?ref=main)
| xunit.runner.visualstudio | [![](https://img.shields.io/nuget/v/xunit.runner.visualstudio.svg?logo=nuget)](https://www.nuget.org/packages/xunit.runner.visualstudio) | [![](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/xunit/xunit/shield/xunit.runner.visualstudio/latest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit.runner.visualstudio) | [![](https://img.shields.io/endpoint.svg?url=https://actions-badge.atrox.dev/xunit/visualstudio.xunit/badge%3Fref%3Dmain&amp;label=build)](https://actions-badge.atrox.dev/xunit/visualstudio.xunit/goto?ref=main)

*For complete CI package lists, please visit the [feedz.io package search](https://feedz.io/org/xunit/repository/xunit/search). A free login is required.*

## About xUnit.net

[<img align="right" width="100px" src="https://raw.githubusercontent.com/xunit/media/main/dotnet-foundation.svg" />](https://dotnetfoundation.org/projects/project-detail/xunit)

xUnit.net is a free, open source, community-focused unit testing tool for the .NET Framework. Written by the original inventor of NUnit v2, xUnit.net is the latest technology for unit testing C# and F# (other .NET languages may work as well, but are unsupported). xUnit.net works with Visual Studio, Visual Studio Code, ReSharper, CodeRush, and TestDriven.NET. It is part of the [.NET Foundation](https://www.dotnetfoundation.org/), and operates under their [code of conduct](https://www.dotnetfoundation.org/code-of-conduct). It is licensed under [Apache 2](https://opensource.org/licenses/Apache-2.0) (an OSI approved license).

For project documentation, please visit the [xUnit.net project home](https://xunit.net/).
