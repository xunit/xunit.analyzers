using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

#if NETCOREAPP
using System.Runtime.Loader;
#endif

namespace Xunit.Analyzers;

public class AnalyzerLoaderBase
{
	static readonly Lazy<string> nuGetPackagesFolder = new(GetNuGetPackagesFolder, isThreadSafe: true);

	protected static string NuGetPackagesFolder => nuGetPackagesFolder.Value;

	protected static Type FindType(
		Lazy<Assembly> assembly,
		string typeName) =>
			assembly.Value.GetType(typeName) ?? throw new InvalidOperationException($"Could not locate type '{typeName}' from '{assembly.Value.GetName().Name}'");

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

	protected static Assembly LoadAssembly(string assemblyPath) =>
#if NETCOREAPP
		AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
#else
		Assembly.LoadFrom(assemblyPath);
#endif
}
