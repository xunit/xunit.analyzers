#if NETCOREAPP

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

public static class CodeAnalysisNetAnalyzers
{
	public static Lazy<Assembly> assembly = new(LoadAssembly, isThreadSafe: true);
	public static Lazy<Type> typeCA1515 = new(() => FindType("Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpMakeTypesInternal"), isThreadSafe: true);

	public static DiagnosticAnalyzer CA1515() =>
		Activator.CreateInstance(typeCA1515.Value) as DiagnosticAnalyzer ?? throw new InvalidOperationException($"Could not create instance of '{typeCA1515.Value.FullName}'");

	static Type FindType(string typeName) =>
		assembly.Value.GetType(typeName) ?? throw new InvalidOperationException($"Could not locate type '{typeName}' from Microsoft.CodeAnalysis.NetAnalyzers");

	static Assembly LoadAssembly()
	{
		var nugetPackagesFolder = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
		if (nugetPackagesFolder is null)
		{
			var homeFolder =
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? Environment.GetEnvironmentVariable("USERPROFILE")
					: Environment.GetEnvironmentVariable("HOME");

			nugetPackagesFolder = Path.Combine(homeFolder ?? throw new InvalidOperationException("Could not determine home folder"), ".nuget", "packages");
		}

		if (!Directory.Exists(nugetPackagesFolder))
			throw new InvalidOperationException($"NuGet package cache folder '{nugetPackagesFolder}' does not exist");

		var loadContext = AssemblyLoadContext.Default;
		loadContext.LoadFromAssemblyPath(Path.Combine(nugetPackagesFolder, "microsoft.codeanalysis.workspaces.common", "3.11.0", "lib", "netcoreapp3.1", "Microsoft.CodeAnalysis.Workspaces.dll"));
		loadContext.LoadFromAssemblyPath(Path.Combine(nugetPackagesFolder, "microsoft.codeanalysis.netanalyzers", "9.0.0-preview.24072.1", "analyzers", "dotnet", "cs", "Microsoft.CodeAnalysis.NetAnalyzers.dll"));
		return loadContext.LoadFromAssemblyPath(Path.Combine(nugetPackagesFolder, "microsoft.codeanalysis.netanalyzers", "9.0.0-preview.24072.1", "analyzers", "dotnet", "cs", "Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll"));
	}
}

#endif
