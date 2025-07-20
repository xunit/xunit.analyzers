using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.Test,
	BuildTarget.Build
)]
public class Test
{
	public static Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running tests");

		var testDLLs =
			Directory
				.GetFiles(Path.Join(context.BaseFolder, "src"), "*.tests*.csproj", SearchOption.AllDirectories)
				.Select(csproj => '"' + Path.Combine(Path.GetDirectoryName(csproj)!, "bin", context.ConfigurationText, "net8.0", Path.GetFileNameWithoutExtension(csproj) + ".net8.0.dll") + '"');

		if (context.NeedMono)
		{
			context.WriteLineColor(ConsoleColor.Yellow, "Skipping .NET Framework tests on Mono");
			Console.WriteLine();
		}
		else
			testDLLs = testDLLs.Concat(
				Directory
					.GetFiles(Path.Join(context.BaseFolder, "src"), "*.tests*.csproj", SearchOption.AllDirectories)
					.Select(csproj => '"' + Path.Combine(Path.GetDirectoryName(csproj)!, "bin", context.ConfigurationText, "net472", Path.GetFileNameWithoutExtension(csproj) + ".net472.exe") + '"')
			);


		return context.Exec(context.ConsoleRunner, $"{string.Join(" ", testDLLs)} -ctrf {Path.Join(context.TestOutputFolder, "results.ctrf")}");
	}
}
