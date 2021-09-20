using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify_WithAbstractions = CSharpVerifier<TestCaseMustBeLongLivedMarshalByRefObjectTests.Analyzer_WithAbstractions>;
using Verify_WithExecution = CSharpVerifier<TestCaseMustBeLongLivedMarshalByRefObjectTests.Analyzer_WithExecution>;

public class TestCaseMustBeLongLivedMarshalByRefObjectTests
{
	readonly static string Template = @"
public class Foo {{ }}
public class MyLLMBRO: Xunit.LongLivedMarshalByRefObject {{ }}
public class MyTestCase: {0} {{ }}";

	public static TheoryData<string> Interfaces = new()
	{
		"{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|}",
		"{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Sdk.IXunitTestCase|}|}|}|}|}|}|}|}|}|}|}|}|}",
	};

	public static TheoryData<string, string> InterfacesWithBaseClasses
	{
		get
		{
			var result = new TheoryData<string, string>();

			foreach (var @interface in Interfaces.Select(x => (string)x[0]))
			{
				result.Add(@interface, "MyLLMBRO");
				result.Add(@interface, "Xunit.LongLivedMarshalByRefObject");
			}

			return result;
		}
	}

	[Fact]
	public async void NonTestCase_NoDiagnostics()
	{
		var source = "public class Foo { }";

		await Verify_WithExecution.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void XunitTestCase_NoDiagnostics()
	{
		var source = "public class MyTestCase: Xunit.Sdk.XunitTestCase { }";

		await Verify_WithExecution.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(InterfacesWithBaseClasses))]
	public async void InterfaceWithProperBaseClass_NoDiagnostics(
		string @interface,
		string baseClass)
	{
		var source = string.Format(Template, $"{baseClass}, {@interface}");

		await Verify_WithExecution.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async void InterfaceWithoutBaseClass_ReturnsError(string @interface)
	{
		var source = string.Format(Template, @interface);
		var expected =
			Verify_WithExecution
				.Diagnostic()
				.WithLocation(4, 14)
				.WithArguments("MyTestCase");

		await Verify_WithExecution.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async void InterfaceWithBadBaseClass_ReturnsError(string @interface)
	{
		var source = string.Format(Template, $"Foo, {@interface}");
		var expected =
			Verify_WithExecution
				.Diagnostic()
				.WithLocation(4, 14)
				.WithArguments("MyTestCase");

		await Verify_WithExecution.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Fact]
	public async void WithOnlyAbstractions_StillTriggersDiagnostic()
	{
		var source = "public class MyTestCase : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";
		var expected =
			Verify_WithAbstractions
				.Diagnostic()
				.WithLocation(1, 14)
				.WithArguments("MyTestCase");

		await Verify_WithAbstractions.VerifyAnalyzerAsyncV2(source, expected);
	}

	internal class Analyzer_WithAbstractions : TestCaseMustBeLongLivedMarshalByRefObject
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Abstractions(compilation);
	}

	internal class Analyzer_WithExecution : TestCaseMustBeLongLivedMarshalByRefObject
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Execution(compilation);
	}
}
