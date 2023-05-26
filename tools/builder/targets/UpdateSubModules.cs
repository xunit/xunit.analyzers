using System.IO;
using System.Threading.Tasks;

[Target(BuildTarget.UpdateSubModules)]
public static class UpdateSubModules
{
	public static async Task OnExecute(BuildContext context)
	{
		var mediaFiles = Directory.GetFiles(Path.Combine(context.BaseFolder, "tools", "media"));
		if (mediaFiles.Length == 0)
		{
			context.BuildStep("Updating submodules");

			await context.Exec("git", "submodule update --init");
		}
	}
}
