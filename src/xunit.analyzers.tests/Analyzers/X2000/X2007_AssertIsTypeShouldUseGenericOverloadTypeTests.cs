using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;

#if NETCOREAPP
using Microsoft.CodeAnalysis.CSharp;
#endif

public class AssertIsTypeShouldUseGenericOverloadTypeTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void ForNonGenericCall_Triggers() {
					{|#0:Assert.IsType(typeof(int), 1)|};
					{|#1:Assert.IsNotType(typeof(int), 1)|};
					{|#2:Assert.IsAssignableFrom(typeof(int), 1)|};
				}

				void ForGenericCall_DoesNotTrigger() {
					Assert.IsType<int>(1);
					Assert.IsType<int>(1, false);
					Assert.IsType<int>(1, true);

					Assert.IsNotType<int>(1);
					Assert.IsNotType<int>(1, false);
					Assert.IsNotType<int>(1, true);

					Assert.IsAssignableFrom<int>(1);
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("int"),
			Verify.Diagnostic().WithLocation(1).WithArguments("int"),
			Verify.Diagnostic().WithLocation(2).WithArguments("int"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

#if NETCOREAPP

	[Fact]
	public async ValueTask NonStaticAbstractInterfaceMethods()
	{
		string source = /* lang=c#-test */ """
			using Xunit;

			interface IStaticMethod {
				static void Method() { }
			}

			class StaticMethodClass : IStaticMethod {
				public static void Method() { }
			}

			interface IAbstractMethod {
				abstract void Method();
			}

			class AbstractMethodClass : IAbstractMethod {
				public void Method() { }
			}

			class TestClass {
				void StaticInterfaceMethod_Triggers() {
					{|#0:Assert.IsType(typeof(IStaticMethod), new StaticMethodClass())|};
					{|#1:Assert.IsNotType(typeof(IStaticMethod), new StaticMethodClass())|};
					{|#2:Assert.IsAssignableFrom(typeof(IStaticMethod), new StaticMethodClass())|};
				}

				void AbstractInterfaceMethod_Triggers() {
					{|#10:Assert.IsType(typeof(IAbstractMethod), new AbstractMethodClass())|};
					{|#11:Assert.IsNotType(typeof(IAbstractMethod), new AbstractMethodClass())|};
					{|#12:Assert.IsAssignableFrom(typeof(IAbstractMethod), new AbstractMethodClass())|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("IStaticMethod"),
			Verify.Diagnostic().WithLocation(1).WithArguments("IStaticMethod"),
			Verify.Diagnostic().WithLocation(2).WithArguments("IStaticMethod"),

			Verify.Diagnostic().WithLocation(10).WithArguments("IAbstractMethod"),
			Verify.Diagnostic().WithLocation(11).WithArguments("IAbstractMethod"),
			Verify.Diagnostic().WithLocation(12).WithArguments("IAbstractMethod"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

#endif  // NETCOREAPP

#if NETCOREAPP && ROSLYN_LATEST

	[Fact]
	public async ValueTask StaticAbstractInterfaceMethods()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			interface IStaticAbstractMethod {
				static abstract void Method();
			}

			class StaticAbstractMethodClass : IStaticAbstractMethod {
				public static void Method() { }
			}

			interface IStaticAbstractMethodChild : IStaticAbstractMethod { }

			class StaticAbstractMethodChildClass : IStaticAbstractMethodChild {
				public static void Method() { }
			}

			class TestClass {
				void TestMethod() {
					Assert.IsAssignableFrom(typeof(IStaticAbstractMethod), new StaticAbstractMethodClass());
					Assert.IsAssignableFrom(typeof(IStaticAbstractMethod), new StaticAbstractMethodChildClass());
					Assert.IsAssignableFrom(typeof(IStaticAbstractMethodChild), new StaticAbstractMethodChildClass());
				}
			}
			""";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp11, source);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST
}
