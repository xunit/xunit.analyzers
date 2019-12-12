using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(BuildTarget.PublishPackages,
        BuildTarget.DownloadNuGet, BuildTarget.Packages)]
public static class PublishPackages
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Publishing NuGet packages");

        var publishSource = Environment.GetEnvironmentVariable("PublishSource");
        var publishApiKey = Environment.GetEnvironmentVariable("PublishApiKey");
        if (string.IsNullOrWhiteSpace(publishSource) || string.IsNullOrWhiteSpace(publishApiKey))
        {
            context.WriteLineColor(ConsoleColor.Yellow, $"Skipping package publishing because environment variables 'PublishSource' and/or 'PublishApiKey' are not set.{Environment.NewLine}");
            return;
        }

        var packageFiles = Directory.GetFiles(context.PackageOutputFolder, "*.nupkg", SearchOption.AllDirectories)
                                    .OrderBy(x => x)
                                    .Select(x => x.Substring(context.BaseFolder.Length + 1));

        foreach (var packageFile in packageFiles)
        {
            var args = $"push -source {publishSource} -apiKey {publishApiKey} {packageFile}";
            var redactedArgs = args.Replace(publishApiKey, "[redacted]");
            await context.Exec(context.NuGetExe, args, redactedArgs);
        }
    }
}
