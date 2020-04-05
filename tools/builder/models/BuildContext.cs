using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Bullseye;
using Bullseye.Internal;
using McMaster.Extensions.CommandLineUtils;
using SimpleExec;

[Command(Name = "build", Description = "Build utility for xUnit.net analyzers")]
[HelpOption("-?|-h|--help")]
public class BuildContext
{
	// Versions of downloaded dependent software

	public string NuGetVersion => "5.3.1";

	// Calculated properties

	public string BaseFolder { get; private set; }

	public string ConfigurationText => Configuration.ToString();

	public bool NeedMono { get; private set; }

	public string NuGetExe { get; private set; }

	public string NuGetUrl { get; private set; }

	public string PackageOutputFolder { get; private set; }

	public string TestOutputFolder { get; private set; }

	// User-controllable command-line options

	[Option("-c|--configuration", Description = "The target configuration (default: 'Release'; values: 'Debug', 'Release')")]
	public Configuration Configuration { get; } = Configuration.Release;

	[Option("-N|--no-color", Description = "Disable colored output")]
	public bool NoColor { get; }

	[Option("-s|--skip-dependencies", Description = "Do not run targets' dependencies")]
	public bool SkipDependencies { get; }

	[Argument(0, "targets", Description = "The target(s) to run (default: 'PR'; values: 'Build', 'CI', 'Packages', 'PR', 'Restore', 'Test', 'TestCore', 'TestFx')")]
	public BuildTarget[] Targets { get; } = new[] { BuildTarget.PR };

	[Option("-v|--verbosity", Description = "Set verbosity level (default: 'minimal'; values: 'q[uiet]', 'm[inimal]', 'n[ormal]', 'd[etailed]', and 'diag[nostic]'")]
	public BuildVerbosity Verbosity { get; } = BuildVerbosity.minimal;

	internal BuildVerbosity VerbosityNuGet
	{
		get
		{
			switch (Verbosity)
			{
				case BuildVerbosity.diagnostic: return BuildVerbosity.detailed;
				case BuildVerbosity.minimal: return BuildVerbosity.normal;
				default: return Verbosity;
			}
		}
	}

	// Helper methods for build target consumption

	public void BuildStep(string message)
	{
		WriteLineColor(ConsoleColor.White, $"==> {message} <==");
		Console.WriteLine();
	}

	public async Task Exec(string name, string args, string redactedArgs = null, string workingDirectory = null)
	{
		if (redactedArgs == null)
			redactedArgs = args;

		if (NeedMono && name.EndsWith(".exe"))
		{
			args = $"{name} {args}";
			redactedArgs = $"{name} {redactedArgs}";
			name = "mono";
		}

		WriteLineColor(ConsoleColor.DarkGray, $"EXEC: {name} {redactedArgs}{Environment.NewLine}");

		await Command.RunAsync(name, args, workingDirectory ?? BaseFolder, /*noEcho*/ true);

		Console.WriteLine();
	}

	async Task<int> OnExecuteAsync()
	{
		Exception error = default;

		try
		{
			NeedMono = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

			// Find the folder with the solution file
			BaseFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			while (true)
			{
				if (Directory.GetFiles(BaseFolder, "*.sln").Count() != 0)
					break;

				BaseFolder = Path.GetDirectoryName(BaseFolder);
				if (BaseFolder == null)
					throw new InvalidOperationException("Could not locate a solution file in the directory hierarchy");
			}

			// Dependent folders
			PackageOutputFolder = Path.Combine(BaseFolder, "artifacts", "packages");
			Directory.CreateDirectory(PackageOutputFolder);

			TestOutputFolder = Path.Combine(BaseFolder, "artifacts", "test");
			Directory.CreateDirectory(TestOutputFolder);

			var homeFolder = NeedMono
				? Environment.GetEnvironmentVariable("HOME")
				: Environment.GetEnvironmentVariable("USERPROFILE");

			var nuGetCliFolder = Path.Combine(homeFolder, ".nuget", "cli", NuGetVersion);
			Directory.CreateDirectory(nuGetCliFolder);

			NuGetExe = Path.Combine(nuGetCliFolder, "nuget.exe");
			NuGetUrl = $"https://dist.nuget.org/win-x86-commandline/v{NuGetVersion}/nuget.exe";

			// Parse the targets and Bullseye-specific arguments
			var bullseyeArguments = Targets.Select(x => x.ToString());
			if (SkipDependencies)
				bullseyeArguments = bullseyeArguments.Append("--skip-dependencies");

			// Find target classes
			var targetCollection = new TargetCollection();
			var targets
				= Assembly.GetExecutingAssembly()
					.ExportedTypes
					.Select(x => new { type = x, attr = x.GetCustomAttribute<TargetAttribute>() })
					.Where(x => x.attr != null);

			foreach (var target in targets)
			{
				var method = target.type.GetRuntimeMethod("OnExecute", new[] { typeof(BuildContext) });

				if (method == null)
					targetCollection.Add(new Target(target.attr.TargetName, target.attr.DependentTargets));
				else
					targetCollection.Add(new ActionTarget(target.attr.TargetName, target.attr.DependentTargets, () => (Task)method.Invoke(null, new[] { this })));
			}

			// Let Bullseye run the target(s)
			await targetCollection.RunAsync(bullseyeArguments.ToList(), SkipDependencies, false, false, new NullLogger(), null);

			return 0;
		}
		catch (Exception ex)
		{
			error = ex;
			while (error is TargetInvocationException || error is TargetFailedException)
				error = error.InnerException;
		}

		Console.WriteLine();

		if (error is NonZeroExitCodeException nonZeroExit)
		{
			WriteLineColor(ConsoleColor.Red, "==> Build failed! <==");
			return nonZeroExit.ExitCode;
		}

		WriteLineColor(ConsoleColor.Red, $"==> Build failed! An unhandled exception was thrown <==");
		Console.WriteLine(error.ToString());
		return -1;
	}

	public void WriteColor(ConsoleColor foregroundColor, string text)
	{
		if (!NoColor)
			Console.ForegroundColor = foregroundColor;

		Console.Write(text);

		if (!NoColor)
			Console.ResetColor();
	}

	public void WriteLineColor(ConsoleColor foregroundColor, string text)
		=> WriteColor(foregroundColor, $"{text}{Environment.NewLine}");
}
