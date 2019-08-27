using System.IO;
using System.Threading.Tasks;

[Target(nameof(Restore),
        nameof(DownloadNuGet))]
public static class Restore
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Restoring NuGet packages");

        await context.Exec("dotnet", "restore");
    }
}
