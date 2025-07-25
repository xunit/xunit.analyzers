using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

public static partial class SignAssemblies
{
	public static Task OnExecute(BuildContext context)
	{
		// Check early because we don't need to make copies or show the banner for non-signed scenarios
		if (!context.CanSign)
			return Task.CompletedTask;

		context.BuildStep("Signing binaries");

		// Note that any changes to .nuspec files means this list needs to be updated, and nuspec files should
		// always reference the original signed paths, and not dependency copies (i.e., xunit.v3.common.dll)
		var binaries =
			new[] {
				Path.Combine(context.BaseFolder, "src", "xunit.analyzers",       "bin", context.ConfigurationText, "netstandard2.0", "xunit.analyzers.dll"),
				Path.Combine(context.BaseFolder, "src", "xunit.analyzers.fixes", "bin", context.ConfigurationText, "netstandard2.0", "xunit.analyzers.fixes.dll"),
			}.Select(unsignedPath =>
			{
				var unsignedFolder = Path.GetDirectoryName(unsignedPath) ?? throw new InvalidOperationException($"Path '{unsignedPath}' did not have a folder");
				var signedFolder = Path.Combine(unsignedFolder, "signed");
				Directory.CreateDirectory(signedFolder);

				var signedPath = Path.Combine(signedFolder, Path.GetFileName(unsignedPath));
				File.Copy(unsignedPath, signedPath, overwrite: true);

				return signedPath;
			}).ToArray();

		return context.SignFiles(context.BaseFolder, binaries);
	}
}
