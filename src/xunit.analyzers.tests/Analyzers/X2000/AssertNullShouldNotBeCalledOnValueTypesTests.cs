using System.Threading.Tasks;
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
	public async Task FindsWarning_ForValueType(string method)
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

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task DoesNotFindWarning_ForNullableValueType(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        int? val = 1;
        Xunit.Assert.{method}(val);
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task DoesNotFindWarning_ForNullableReferenceType(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        string val = null;
        Xunit.Assert.{method}(val);
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task DoesNotFindWarning_ForClassConstrainedGenericTypes(string method)
	{
		var source = $@"
class Class<T> where T : class {{
    public void Method(T arg) {{
        Xunit.Assert.{method}(arg);
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task DoesNotFindWarning_ForInterfaceConstrainedGenericTypes(string method)
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

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task DoesNotFindWarning_ForUnconstrainedGenericTypes(string method)
	{
		var source = $@"
class Class<T> {{
    public void Method(System.Collections.Generic.IEnumerable<T> collection) {{
        foreach (T item in collection) {{
            Xunit.Assert.{method}(item);
        }}
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	// https://github.com/xunit/xunit/issues/2395
	public async Task DoesNotFindWarning_ForUserDefinedImplicitConversion(string method)
	{
		var source = $@"
public class TestClass
{{
    public void TestMethod()
    {{
        Xunit.Assert.{method}((MyBuggyInt)42);
        Xunit.Assert.{method}((MyBuggyInt)(int?)42);
        Xunit.Assert.{method}((MyBuggyIntBase)42);
        Xunit.Assert.{method}((MyBuggyIntBase)(int?)42);
    }}
}}

public abstract class MyBuggyIntBase
{{
    public static implicit operator MyBuggyIntBase(int i) => new MyBuggyInt();
}}

public class MyBuggyInt : MyBuggyIntBase
{{
    public MyBuggyInt()
	{{
	}}
}}";

		await Verify.VerifyAnalyzer(source);
	}
}
