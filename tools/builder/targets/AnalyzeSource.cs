using System.Threading.Tasks;

namespace Builder
{
	[Target(
		BuildTarget.AnalyzeSource,
		BuildTarget.Restore
	)]
	public static class AnalyzeSource
	{
		public static async Task OnExecute(BuildContext context)
		{
			context.BuildStep("Analyzing source");

			await context.Exec("dotnet", $"format --check --verbosity {context.Verbosity}");
		}
	}
}
