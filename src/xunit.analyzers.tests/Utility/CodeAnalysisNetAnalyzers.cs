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
	static Lazy<Assembly> assemblyCSharpNetAnalyzers = new(LoadCSharpNetAnalyzers, isThreadSafe: true);
	static Lazy<Assembly> assemblyNetAnalyzers = new(LoadNetAnalyzers, isThreadSafe: true);
	static Lazy<string> nuGetPackagesFolder = new(GetNuGetPackagesFolder, isThreadSafe: true);
	static Lazy<Type> typeCA1515 = new(() => FindType(assemblyCSharpNetAnalyzers, "Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpMakeTypesInternal"), isThreadSafe: true);
	static Lazy<Type> typeCA2007 = new(() => FindType(assemblyNetAnalyzers, "Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskAnalyzer"), isThreadSafe: true);

	public static DiagnosticAnalyzer CA1515() =>
		Activator.CreateInstance(typeCA1515.Value) as DiagnosticAnalyzer ?? throw new InvalidOperationException($"Could not create instance of '{typeCA1515.Value.FullName}'");

	public static DiagnosticAnalyzer CA2007() =>
		Activator.CreateInstance(typeCA2007.Value) as DiagnosticAnalyzer ?? throw new InvalidOperationException($"Could not create instance of '{typeCA2007.Value.FullName}'");

	static string NuGetPackagesFolder => nuGetPackagesFolder.Value;

	static Type FindType(
		Lazy<Assembly> assembly,
		string typeName) =>
			assembly.Value.GetType(typeName) ?? throw new InvalidOperationException($"Could not locate type '{typeName}' from Microsoft.CodeAnalysis.NetAnalyzers NuGet package");

	static string GetNuGetPackagesFolder()
	{
		var result = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
		if (result is null)
		{
			var homeFolder =
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? Environment.GetEnvironmentVariable("USERPROFILE")
					: Environment.GetEnvironmentVariable("HOME");

			result = Path.Combine(homeFolder ?? throw new InvalidOperationException("Could not determine home folder"), ".nuget", "packages");
		}

		if (!Directory.Exists(result))
			throw new InvalidOperationException($"NuGet package cache folder '{result}' does not exist");

		return result;
	}

	static Assembly LoadCSharpNetAnalyzers()
	{
		// Make sure we load the dependencies first
		var _ = assemblyNetAnalyzers.Value;

		return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(NuGetPackagesFolder, "microsoft.codeanalysis.netanalyzers", "9.0.0-preview.24216.2", "analyzers", "dotnet", "cs", "Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll"));
	}

	static Assembly LoadNetAnalyzers()
	{
		AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(NuGetPackagesFolder, "microsoft.codeanalysis.workspaces.common", "3.11.0", "lib", "netcoreapp3.1", "Microsoft.CodeAnalysis.Workspaces.dll"));
		return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(NuGetPackagesFolder, "microsoft.codeanalysis.netanalyzers", "9.0.0-preview.24216.2", "analyzers", "dotnet", "cs", "Microsoft.CodeAnalysis.NetAnalyzers.dll"));
	}
}

#endif
