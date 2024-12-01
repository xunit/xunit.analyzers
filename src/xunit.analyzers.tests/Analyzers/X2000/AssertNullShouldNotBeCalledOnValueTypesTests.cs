using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertNullShouldNotBeCalledOnValueTypes>;

public class AssertNullShouldNotBeCalledOnValueTypesTests
{
	public static TheoryData<string> Methods =
	[
		"Null",
		"NotNull",
	];

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForValueType_Triggers(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					int val = 1;
					{{|#0:Xunit.Assert.{0}(val)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", "int");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForNullableValueType_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					int? val = 1;
					Xunit.Assert.{0}(val);
				}}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForNullableReferenceType_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					string val = null;
					Xunit.Assert.{0}(val);
				}}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForClassConstrainedGenericTypes_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class Class<T> where T : class {{
				public void Method(T arg) {{
					Xunit.Assert.{0}(arg);
				}}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForInterfaceConstrainedGenericTypes_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			interface IDo {{ }}

			class Class<T> where T : IDo {{
				public void Method(System.Collections.Generic.IEnumerable<T> collection) {{
					foreach (T item in collection) {{
						Xunit.Assert.{0}(item);
					}}
				}}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForUnconstrainedGenericTypes_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class Class<T> {{
				public void Method(System.Collections.Generic.IEnumerable<T> collection) {{
					foreach (T item in collection) {{
						Xunit.Assert.{0}(item);
					}}
				}}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	// https://github.com/xunit/xunit/issues/2395
	public async Task ForUserDefinedImplicitConversion_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
				public void TestMethod() {{
					Xunit.Assert.{0}((MyBuggyInt)42);
					Xunit.Assert.{0}((MyBuggyInt)(int?)42);
					Xunit.Assert.{0}((MyBuggyIntBase)42);
					Xunit.Assert.{0}((MyBuggyIntBase)(int?)42);
				}}
			}}

			public abstract class MyBuggyIntBase {{
				public static implicit operator MyBuggyIntBase(int i) => new MyBuggyInt();
			}}

			public class MyBuggyInt : MyBuggyIntBase {{
				public MyBuggyInt() {{ }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}
}
