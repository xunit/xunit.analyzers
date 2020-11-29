using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Builder
{
	[Target(
		BuildTarget.Packages,
		BuildTarget.Build, BuildTarget.DownloadNuGet
	)]
	public static class Packages
	{
		public static async Task OnExecute(BuildContext context)
		{
			context.BuildStep("Creating NuGet packages");

			var versionOverride = string.Format(
				"{0}{1}.{2}+{3}",
				Environment.GetEnvironmentVariable("NBGV_SimpleVersion"),
				Environment.GetEnvironmentVariable("NBGV_PrereleaseVersion"),
				Environment.GetEnvironmentVariable("NBGV_VersionHeight"),
				Environment.GetEnvironmentVariable("NBGV_GitCommitIdShort"));
			var versionOption = versionOverride == ".+" ? string.Empty : $"-Version \"{versionOverride}\"";

			var nuspecFiles =
				Directory.GetFiles(context.BaseFolder, "*.nuspec", SearchOption.AllDirectories)
					.OrderBy(x => x)
					.Select(x => x[(context.BaseFolder.Length + 1)..]);

			foreach (var nuspecFile in nuspecFiles)
				await context.Exec(context.NuGetExe, $"pack {nuspecFile} -NonInteractive -NoPackageAnalysis -OutputDirectory {context.PackageOutputFolder} -Properties Configuration={context.ConfigurationText} -Verbosity {context.VerbosityNuGet} {versionOption}");
		}
	}
}
