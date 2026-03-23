using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1018_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1053

			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> FieldData;
				public static TheoryData<int> PropertyData { get; set; }
				public static TheoryData<int> MethodData() { return null; }

				public static class ClassData { }
				public delegate IEnumerable<object[]> DelegateData();
				public static event EventHandler EventData;

				[MemberData(nameof(FieldData))]
				public void TestMethod1(int _) { }

				[MemberData(nameof(PropertyData))]
				public void TestMethod2(int _) { }

				[MemberData(nameof(MethodData))]
				public void TestMethod3(int _) { }

				[{|xUnit1018:MemberData(nameof(ClassData))|}]
				public void TestMethod4(int _) { }

				[{|xUnit1018:MemberData(nameof(DelegateData))|}]
				public void TestMethod5(int _) { }

				[{|xUnit1018:MemberData(nameof(EventData))|}]
				public void TestMethod6(int _) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
