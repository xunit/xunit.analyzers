using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class Packages
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Creating NuGet packages");

		// Clean up any existing packages to force re-packing
		var packageFiles = Directory.GetFiles(context.PackageOutputFolder, "*.nupkg");
		foreach (var packageFile in packageFiles)
			File.Delete(packageFile);

		// You can't see the created package name in .NET 9+ SDK without doing detailed verbosity
		var verbosity =
			context.DotNetSdkVersion.Major <= 8
				? context.Verbosity.ToString()
				: "detailed";

		await context.Exec("dotnet", $"pack --nologo --no-build --configuration {context.ConfigurationText} --output {context.PackageOutputFolder} --verbosity {verbosity} src/xunit.analyzers -p:NuspecFile=xunit.analyzers.nuspec");
	}
}
