using System.Threading.Tasks;

[Target(
	BuildTarget.Build,
	BuildTarget.UpdateSubModules, BuildTarget.Restore
)]
public static class Build
{
	public static async Task OnExecute(BuildContext context)
	{
		context.BuildStep("Compiling binaries");

		await context.Exec("dotnet", $"msbuild -nologo -maxCpuCount -restore:False -verbosity:{context.Verbosity} -p:Configuration={context.ConfigurationText}");
	}
}
