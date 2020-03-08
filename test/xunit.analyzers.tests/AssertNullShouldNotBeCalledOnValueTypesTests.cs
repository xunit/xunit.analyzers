using Microsoft.CodeAnalysis;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertNullShouldNotBeCalledOnValueTypes>;

namespace Xunit.Analyzers
{
	public class AssertNullShouldNotBeCalledOnValueTypesTests
	{
		public static TheoryData<string> Methods
			= new TheoryData<string> { "Null", "NotNull" };

		[Theory]
		[MemberData(nameof(Methods))]
		public async void FindsWarning_ForValueType(string method)
		{
			var source =
@"class TestClass { void TestMethod() {
    int val = 1;
    Xunit.Assert." + method + @"(val);
} }";

			var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 23 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()", "int");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[MemberData(nameof(Methods))]
		public async void DoesNotFindWarning_ForNullableValueType(string method)
		{
			var source =
@"class TestClass { void TestMethod() {
    int? val = 1;
    Xunit.Assert." + method + @"(val);
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Theory]
		[MemberData(nameof(Methods))]
		public async void DoesNotFindWarning_ForNullableReferenceType(string method)
		{
			var source =
@"class TestClass { void TestMethod() {
    string val = null;
    Xunit.Assert." + method + @"(val);
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Theory]
		[MemberData(nameof(Methods))]
		public async void DoesNotFindWarning_ForClassConstrainedGenericTypes(string method)
		{
			var source =
				@"
class Class<T> where T : class
{
  public void Method(T arg)
  {
    Xunit.Assert." + method + @"(arg);
  }
}";
			await Verify.VerifyAnalyzerAsync(source);
		}

		[Theory]
		[MemberData(nameof(Methods))]
		public async void DoesNotFindWarning_ForInterfaceConstrainedGenericTypes(string method)
		{
			var source =
				@"
interface IDo {}

class Class<T> where T : IDo
{
  public void Method(System.Collections.Generic.IEnumerable<T> collection)
  {
    foreach (T item in collection)
    {
      Xunit.Assert." + method + @"(item);
    }
  }
}";
			await Verify.VerifyAnalyzerAsync(source);
		}

		[Theory]
		[MemberData(nameof(Methods))]
		public async void DoesNotFindWarning_ForUnconstrainedGenericTypes(string method)
		{
			var source =
				@"
class Class<T>
{
  public void Method(System.Collections.Generic.IEnumerable<T> collection)
  {
    foreach (T item in collection)
    {
      Xunit.Assert." + method + @"(item);
    }
  }
}";
			await Verify.VerifyAnalyzerAsync(source);
		}
	}
}
