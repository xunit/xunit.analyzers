using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertNullShouldNotBeCalledOnValueTypes>;

public class X2002_AssertNullShouldNotBeCalledOnValueTypesTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			class TestClass {
				readonly int IntValue = 1;
				readonly int? NullableIntValue = 1;
				readonly string StringValue = null;

				void ForValueType_Triggers() {
					{|#0:Assert.Null(IntValue)|};
					{|#1:Assert.NotNull(IntValue)|};
				}

				void ForNullableValueType_DoesNotTrigger() {
					Assert.Null(NullableIntValue);
					Assert.NotNull(NullableIntValue);
				}

				void ForNullableReferenceType_DoesNotTrigger() {
					Assert.Null(StringValue);
					Assert.NotNull(StringValue);
				}

				// https://github.com/xunit/xunit/issues/2395
				void ForUserDefinedImplicitConversion_DoesNotTrigger() {
					Assert.Null((MyBuggyInt)42);
					Assert.Null((MyBuggyInt)(int?)42);
					Assert.Null((MyBuggyIntBase)42);
					Assert.Null((MyBuggyIntBase)(int?)42);

					Assert.NotNull((MyBuggyInt)42);
					Assert.NotNull((MyBuggyInt)(int?)42);
					Assert.NotNull((MyBuggyIntBase)42);
					Assert.NotNull((MyBuggyIntBase)(int?)42);
				}

				abstract class MyBuggyIntBase {
					public static implicit operator MyBuggyIntBase(int i) => new MyBuggyInt();
				}

				class MyBuggyInt : MyBuggyIntBase {
					public MyBuggyInt() { }
				}
			}

			class ClassConstrained<T> where T : class {
				void ForClassConstrained_DoesNotTrigger(T arg) {
					Assert.Null(arg);
					Assert.NotNull(arg);
				}

				void ForInterfaceWithClassConstraint_DoesNotTrigger(IEnumerable<T> arg) {
					foreach (var value in arg) {
						Assert.Null(arg);
						Assert.NotNull(arg);
					}
				}
			}

			interface IDo { }

			class InterfaceConstrained<T> where T : IDo {
				void ForInterfaceConstrained_DoesNotTrigger(T arg) {
					Assert.Null(arg);
					Assert.NotNull(arg);
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Null()", "int"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.NotNull()", "int"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				unsafe void ForPointerType_DoesNotTrigger() {
					var value = 42;
					var ptr = &value;

					Assert.Null(ptr);
					Assert.NotNull(ptr);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}
}
