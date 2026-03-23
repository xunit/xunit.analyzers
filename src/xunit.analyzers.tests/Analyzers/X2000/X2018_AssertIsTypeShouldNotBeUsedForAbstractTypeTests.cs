using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldNotBeUsedForAbstractType>;
using Verify_v2_Pre2_9_3 = CSharpVerifier<X2018_AssertIsTypeShouldNotBeUsedForAbstractTypeTests.Analyzer_v2_Pre2_9_3>;
using Verify_v3_Pre0_6_0 = CSharpVerifier<X2018_AssertIsTypeShouldNotBeUsedForAbstractTypeTests.Analyzer_v3_Pre0_6_0>;

public class X2018_AssertIsTypeShouldNotBeUsedForAbstractTypeTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.IO;
			using Xunit;
			using static Xunit.Assert;

			class TestClass {
				readonly bool flag = true;

				void Interface_Triggers() {
					{|#0:Assert.IsType<IDisposable>(new object())|};
					{|#1:Assert.IsNotType<IDisposable>(new object())|};
				}

				void AbstractClass_Triggers() {
					{|#10:Assert.IsType<Stream>(new object())|};
					{|#11:Assert.IsNotType<Stream>(new object())|};
				}

				void UsingStatic_Triggers() {
					{|#20:IsType<IDisposable>(new object())|};
					{|#21:IsNotType<IDisposable>(new object())|};
				}

				void Interface_WithExactMatchFlag_TriggersForLiteralTrue() {
					{|#30:Assert.IsType<IDisposable>(new object(), true)|};
					{|#31:Assert.IsNotType<IDisposable>(new object(), true)|};
					{|#32:Assert.IsType<IDisposable>(new object(), exactMatch: true)|};
					{|#33:Assert.IsNotType<IDisposable>(new object(), exactMatch: true)|};
					Assert.IsType<IDisposable>(new object(), flag);
					Assert.IsNotType<IDisposable>(new object(), flag);
				}

				void AbstractClass_WithExactMatchFlag_Triggers() {
					{|#40:Assert.IsType<Stream>(new object(), true)|};
					{|#41:Assert.IsNotType<Stream>(new object(), true)|};
					{|#42:Assert.IsType<Stream>(new object(), exactMatch: true)|};
					{|#43:Assert.IsNotType<Stream>(new object(), exactMatch: true)|};
					Assert.IsType<Stream>(new object(), flag);
					Assert.IsNotType<Stream>(new object(), flag);
				}

				void UsingStatic_WithExactMatchFlag_Triggers() {
					{|#50:IsType<IDisposable>(new object(), true)|};
					{|#51:IsNotType<IDisposable>(new object(), true)|};
					{|#52:IsType<IDisposable>(new object(), exactMatch: true)|};
					{|#53:IsNotType<IDisposable>(new object(), exactMatch: true)|};
					IsType<IDisposable>(new object(), flag);
					IsNotType<IDisposable>(new object(), flag);
				}

				void NonAbstractClass_DoesNotTrigger() {
					Assert.IsType<string>(new object());
					Assert.IsNotType<string>(new object());
					Assert.IsType<string>(new object(), flag);
					Assert.IsNotType<string>(new object(), flag);
					Assert.IsType<string>(new object(), exactMatch: flag);
					Assert.IsNotType<string>(new object(), exactMatch: flag);
					Assert.IsType<string>(new object(), true);
					Assert.IsNotType<string>(new object(), true);
					Assert.IsType<string>(new object(), exactMatch: true);
					Assert.IsNotType<string>(new object(), exactMatch: true);
					Assert.IsType<string>(new object(), false);
					Assert.IsNotType<string>(new object(), false);
					Assert.IsType<string>(new object(), exactMatch: false);
					Assert.IsNotType<string>(new object(), exactMatch: false);
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(1).WithArguments("interface", "System.IDisposable", "exactMatch: false"),

			Verify.Diagnostic().WithLocation(10).WithArguments("abstract class", "System.IO.Stream", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(11).WithArguments("abstract class", "System.IO.Stream", "exactMatch: false"),

			Verify.Diagnostic().WithLocation(20).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(21).WithArguments("interface", "System.IDisposable", "exactMatch: false"),

			Verify.Diagnostic().WithLocation(30).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(31).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(32).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(33).WithArguments("interface", "System.IDisposable", "exactMatch: false"),

			Verify.Diagnostic().WithLocation(40).WithArguments("abstract class", "System.IO.Stream", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(41).WithArguments("abstract class", "System.IO.Stream", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(42).WithArguments("abstract class", "System.IO.Stream", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(43).WithArguments("abstract class", "System.IO.Stream", "exactMatch: false"),

			Verify.Diagnostic().WithLocation(50).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(51).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(52).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(53).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V2_and_V3_PreInexactMatchSupport()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.IO;
			using Xunit;
			using static Xunit.Assert;

			class TestClass {
				readonly bool flag = true;

				void Interface_Triggers() {
					{|#0:Assert.IsType<IDisposable>(new object())|};
					{|#1:Assert.IsNotType<IDisposable>(new object())|};
				}

				void AbstractClass_Triggers() {
					{|#10:Assert.IsType<Stream>(new object())|};
					{|#11:Assert.IsNotType<Stream>(new object())|};
				}

				void UsingStatic_Triggers() {
					{|#20:IsType<IDisposable>(new object())|};
					{|#21:IsNotType<IDisposable>(new object())|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("interface", "System.IDisposable", "Assert.IsAssignableFrom"),
			Verify.Diagnostic().WithLocation(1).WithArguments("interface", "System.IDisposable", "Assert.IsNotAssignableFrom"),

			Verify.Diagnostic().WithLocation(10).WithArguments("abstract class", "System.IO.Stream", "Assert.IsAssignableFrom"),
			Verify.Diagnostic().WithLocation(11).WithArguments("abstract class", "System.IO.Stream", "Assert.IsNotAssignableFrom"),

			Verify.Diagnostic().WithLocation(20).WithArguments("interface", "System.IDisposable", "Assert.IsAssignableFrom"),
			Verify.Diagnostic().WithLocation(21).WithArguments("interface", "System.IDisposable", "Assert.IsNotAssignableFrom"),
		};

		await Verify_v2_Pre2_9_3.VerifyAnalyzer(source, expected);
		await Verify_v3_Pre0_6_0.VerifyAnalyzer(source, expected);
	}

	internal class Analyzer_v2_Pre2_9_3 : AssertIsTypeShouldNotBeUsedForAbstractType
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 9, 2));
	}

	internal class Analyzer_v3_Pre0_6_0 : AssertIsTypeShouldNotBeUsedForAbstractType
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(0, 5, 999));
	}
}
