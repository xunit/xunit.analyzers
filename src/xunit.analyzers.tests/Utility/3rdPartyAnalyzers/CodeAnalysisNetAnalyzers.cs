using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

public class CodeAnalysisNetAnalyzers : AnalyzerLoaderBase
{
	static readonly Lazy<Assembly> assemblyCSharpNetAnalyzers = new(LoadCSharpNetAnalyzers, isThreadSafe: true);
	static readonly Lazy<Assembly> assemblyNetAnalyzers = new(LoadNetAnalyzers, isThreadSafe: true);
	static readonly Lazy<Type> typeCA1515 = new(() => FindType(assemblyCSharpNetAnalyzers, "Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpMakeTypesInternal"), isThreadSafe: true);
	static readonly Lazy<Type> typeCA2007 = new(() => FindType(assemblyNetAnalyzers, "Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskAnalyzer"), isThreadSafe: true);

	public static DiagnosticAnalyzer CA1515() =>
		Activator.CreateInstance(typeCA1515.Value) as DiagnosticAnalyzer ?? throw new InvalidOperationException($"Could not create instance of '{typeCA1515.Value.FullName}'");

	public static DiagnosticAnalyzer CA2007() =>
		Activator.CreateInstance(typeCA2007.Value) as DiagnosticAnalyzer ?? throw new InvalidOperationException($"Could not create instance of '{typeCA2007.Value.FullName}'");

	static Assembly LoadCSharpNetAnalyzers()
	{
		// Make sure we load the dependencies first
		var _ = assemblyNetAnalyzers.Value;

		return LoadAssembly(Path.Combine(NuGetPackagesFolder, "microsoft.codeanalysis.netanalyzers", "9.0.0-preview.24216.2", "analyzers", "dotnet", "cs", "Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll"));
	}

	static Assembly LoadNetAnalyzers()
	{
		LoadAssembly(Path.Combine(NuGetPackagesFolder, "microsoft.codeanalysis.workspaces.common", "3.11.0", "lib", "netcoreapp3.1", "Microsoft.CodeAnalysis.Workspaces.dll"));
		return LoadAssembly(Path.Combine(NuGetPackagesFolder, "microsoft.codeanalysis.netanalyzers", "9.0.0-preview.24216.2", "analyzers", "dotnet", "cs", "Microsoft.CodeAnalysis.NetAnalyzers.dll"));
	}
}
