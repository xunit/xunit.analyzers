using System.Linq;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestCaseMustBeLongLivedMarshalByRefObject>;

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

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Fact]
	public async void XunitTestCase_NoDiagnostics()
	{
		var source = "public class MyTestCase: Xunit.Sdk.XunitTestCase { }";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(InterfacesWithBaseClasses))]
	public async void InterfaceWithProperBaseClass_NoDiagnostics(
		string @interface,
		string baseClass)
	{
		var source = string.Format(Template, $"{baseClass}, {@interface}");

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async void InterfaceWithoutBaseClass_ReturnsError(string @interface)
	{
		var source = string.Format(Template, @interface);
		var expected =
			Verify
				.Diagnostic()
				.WithLocation(4, 14)
				.WithArguments("MyTestCase");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Interfaces))]
	public async void InterfaceWithBadBaseClass_ReturnsError(string @interface)
	{
		var source = string.Format(Template, $"Foo, {@interface}");
		var expected =
			Verify
				.Diagnostic()
				.WithLocation(4, 14)
				.WithArguments("MyTestCase");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}
}
