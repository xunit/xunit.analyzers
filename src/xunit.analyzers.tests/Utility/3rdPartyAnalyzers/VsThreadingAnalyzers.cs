#if NETCOREAPP  // System.Collections.Immutable 1.6.0 conflicts with 6.0.0 in NetFx

using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

public class VsThreadingAnalyzers : AnalyzerLoaderBase
{
	static readonly Lazy<Assembly> assemblyVsThreading = new(LoadVsThreadingAnalyzers, isThreadSafe: true);
	static readonly Lazy<Type> typeVSTHRD200 = new(() => FindType(assemblyVsThreading, "Microsoft.VisualStudio.Threading.Analyzers.VSTHRD200UseAsyncNamingConventionAnalyzer"), isThreadSafe: true);

	public static DiagnosticAnalyzer VSTHRD200() =>
		Activator.CreateInstance(typeVSTHRD200.Value) as DiagnosticAnalyzer ?? throw new InvalidOperationException($"Could not create instance of '{typeVSTHRD200.Value.FullName}'");

	static Assembly LoadVsThreadingAnalyzers()
	{
		LoadAssembly(Path.Combine(NuGetPackagesFolder, "system.collections.immutable", "6.0.0", "lib", "net6.0", "System.Collections.Immutable.dll"));
		LoadAssembly(Path.Combine(NuGetPackagesFolder, "microsoft.codeanalysis.workspaces.common", "3.11.0", "lib", "netcoreapp3.1", "Microsoft.CodeAnalysis.Workspaces.dll"));
		return LoadAssembly(Path.Combine(NuGetPackagesFolder, "microsoft.visualstudio.threading.analyzers", "17.11.20", "analyzers", "cs", "Microsoft.VisualStudio.Threading.Analyzers.dll"));
	}
}

#endif
