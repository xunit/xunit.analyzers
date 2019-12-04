using System.Threading.Tasks;

[Target(nameof(Build),
        nameof(Restore))]
public static class Build
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Compiling binaries");

        await context.Exec("dotnet", $"build --no-restore --configuration {context.ConfigurationText}");
    }
}
