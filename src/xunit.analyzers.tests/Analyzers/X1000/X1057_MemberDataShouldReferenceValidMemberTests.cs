using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1057_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1042

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<object[]> DataMethod() => DataMethod("Hello world");
				public static IEnumerable<object[]> DataMethod(string s) => new object[][] { new object[] { s } };

				[Theory]
				[MemberData(nameof(PublicClass.Data), MemberType = typeof(PublicClass))]
				public void WithPublicClass(string _) { }

				public class PublicClass {
					public static IEnumerable<object[]> Data => new object[][] { };
				}

				[Theory]
				[MemberData(nameof(InternalClass.Data), MemberType = typeof(InternalClass))]
				public void WithInternalClass(string _) { }

				internal class InternalClass {
					public static IEnumerable<object[]> Data => new object[][] { };
				}

				[Theory]
				[{|#0:MemberData(nameof(PrivateClass.Data), MemberType = typeof(PrivateClass))|}]
				public void WithPrivateClass(string _) { }

				class PrivateClass {
					public static IEnumerable<object[]> Data => new object[][] { };
				}
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
#if NETCOREAPP && ROSLYN_LATEST
		var expectedAot = Verify.Diagnostic("xUnit1057").WithLocation(0).WithArguments("MemberData member", "TestClass.PrivateClass");

		await Verify.VerifyAnalyzerV3Aot(source, expectedAot);
#endif
	}
}
