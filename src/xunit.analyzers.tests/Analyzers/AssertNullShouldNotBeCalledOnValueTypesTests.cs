using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertNullShouldNotBeCalledOnValueTypes>;

public class AssertNullShouldNotBeCalledOnValueTypesTests
{
	public static TheoryData<string> Methods = new()
	{
		"Null",
		"NotNull",
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarning_ForValueType(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        int val = 1;
        Xunit.Assert.{method}(val);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 27 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", "int");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForNullableValueType(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        int? val = 1;
        Xunit.Assert.{method}(val);
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForNullableReferenceType(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        string val = null;
        Xunit.Assert.{method}(val);
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForClassConstrainedGenericTypes(string method)
	{
		var source = $@"
class Class<T> where T : class {{
    public void Method(T arg) {{
        Xunit.Assert.{method}(arg);
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForInterfaceConstrainedGenericTypes(string method)
	{
		var source = $@"
interface IDo {{ }}

class Class<T> where T : IDo {{
    public void Method(System.Collections.Generic.IEnumerable<T> collection) {{
        foreach (T item in collection) {{
            Xunit.Assert.{method}(item);
        }}
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForUnconstrainedGenericTypes(string method)
	{
		var source = $@"
class Class<T> {{
    public void Method(System.Collections.Generic.IEnumerable<T> collection) {{
        foreach (T item in collection) {{
            Xunit.Assert.{method}(item);
        }}
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}
}
