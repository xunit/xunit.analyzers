using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1068_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1042

			using System.Collections.Generic;
			using Xunit;

			public abstract class AbstractBase<T1, T2>
			{
				public static IEnumerable<object[]> Foo => new[] { new object[] { 42 } };

				[Theory]
				[{|#0:MemberData(nameof(Foo))|}]
				public void TestMethod1(int _) { }

				[Theory]
				[MemberData(nameof(OtherClass.Bar), MemberType = typeof(OtherClass))]
				public void TestMethod2(int _) { }
			}

			public class TestClass : AbstractBase<int, string>
			{
				[Theory]
				[MemberData(nameof(Foo))]
				public void TestMethod3(int _) { }
			}

			public class OtherClass {
				public static IEnumerable<object[]> Bar => new[] { new object[] { 2112 } };
			}
			""";

		await Verify.VerifyAnalyzerV3NonAot(source);
#if NETCOREAPP && ROSLYN_LATEST
		var expectedAot = Verify.Diagnostic("xUnit1068").WithLocation(0);

		await Verify.VerifyAnalyzerV3Aot(source, expectedAot);
#endif
	}
}
