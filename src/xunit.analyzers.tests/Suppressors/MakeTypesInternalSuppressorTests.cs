#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using Xunit.Supressors;

public sealed class MakeTypesInternalSuppressorTests
{
	[Fact]
	public Task Fact_DiagnosticIsSuppressed()
	{
		const string code = @"using Xunit;

public class {|#0:Program|}
{
    public static void Main(string[] args)
    {
    }
}

public class {|#1:UnitTests|}
{
    [Fact]
    public void Test()
    {
    }
}
";
		DiagnosticResult expectedDiagnostic = new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(false);
		DiagnosticResult suppressedDiagnostic = new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(1).WithIsSuppressed(true);

		return new Verify
		{
			TestCode = code,
			ExpectedDiagnostics = { expectedDiagnostic, suppressedDiagnostic },
			TestState =
			{
				OutputKind = OutputKind.ConsoleApplication
			}
		}.RunAsync();
	}

	[Fact]
	public Task Theory_DiagnosticIsSuppressed()
	{
		const string code = @"using Xunit;

public class {|#0:Program|}
{
    public static void Main(string[] args)
    {
    }
}

public class {|#1:UnitTests|}
{
    [Theory]
    public void Test()
    {
    }
}
";
		DiagnosticResult expectedDiagnostic = new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(false);
		DiagnosticResult suppressedDiagnostic = new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(1).WithIsSuppressed(true);

		return new Verify
		{
			TestCode = code,
			ExpectedDiagnostics = { expectedDiagnostic, suppressedDiagnostic },
			TestState =
			{
				OutputKind = OutputKind.ConsoleApplication
			}
		}.RunAsync();
	}

	[Fact]
	public Task InlineData_DiagnosticIsNotSuppressed()
	{
		const string code = @"using Xunit;

public class {|#0:Program|}
{
    public static void Main(string[] args)
    {
    }
}

public class {|#1:UnitTests|}
{
    [InlineData]
    public void Test()
    {
    }
}
";
		DiagnosticResult expectedDiagnostic1 = new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(false);
		DiagnosticResult expectedDiagnostic2 = new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(1).WithIsSuppressed(false);

		return new Verify
		{
			TestCode = code,
			ExpectedDiagnostics = { expectedDiagnostic1, expectedDiagnostic2 },
			TestState =
			{
				OutputKind = OutputKind.ConsoleApplication
			}
		}.RunAsync();
	}
	
	private class Verify : CSharpCodeFixTest<MakeTypesInternalSuppressor, EmptyCodeFixProvider, XunitVerifier>
	{
		private readonly DiagnosticAnalyzer makeTypesInternalAnalyzer;

		public Verify()
		{
			string nugetPackagesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
			AssemblyLoadContext loadContext = AssemblyLoadContext.Default;
			loadContext.LoadFromAssemblyPath(Path.Combine(nugetPackagesFolder, "microsoft.codeanalysis.workspaces.common", "3.11.0", "lib", "netcoreapp3.1", "Microsoft.CodeAnalysis.Workspaces.dll"));
			loadContext.LoadFromAssemblyPath(Path.Combine(nugetPackagesFolder, "microsoft.codeanalysis.netanalyzers", "9.0.0-preview.24072.1", "analyzers", "dotnet", "cs", "Microsoft.CodeAnalysis.NetAnalyzers.dll"));
			var assembly = loadContext.LoadFromAssemblyPath(Path.Combine(nugetPackagesFolder, "microsoft.codeanalysis.netanalyzers", "9.0.0-preview.24072.1", "analyzers", "dotnet", "cs", "Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll"));

			var makeTypesInternalAnalyzerType = assembly.GetType("Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpMakeTypesInternal");
			if (makeTypesInternalAnalyzerType is null)
			{
				throw new InvalidOperationException("Unable to find CSharpMakeTypesInternal type");
			}

			makeTypesInternalAnalyzer = (DiagnosticAnalyzer) Activator.CreateInstance(makeTypesInternalAnalyzerType)!;
			
			ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages(ImmutableArray.Create(new PackageIdentity("xunit", "2.7.0")));
		}

		protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
		{
			yield return makeTypesInternalAnalyzer;
			yield return new MakeTypesInternalSuppressor();
		}
	}
}
#endif
