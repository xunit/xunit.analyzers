using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1035_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public enum Foo { Bar }

			public class TestClass {
				public static TheoryData<int> StringData(string s) => new TheoryData<int> { s.Length };

				[MemberData(nameof(StringData), {|#0:1|})]
				public void TestMethod1(int _) { }

				public static TheoryData<int> ParamsIntData(params int[] n) => new TheoryData<int> { n[0] };

				[MemberData(nameof(ParamsIntData), {|#1:"bob"|})]
				public void TestMethod2(int _) { }

				// https://github.com/xunit/xunit/issues/2817
				public static TheoryData<int> EnumData(Foo foo) => new TheoryData<int> { (int)foo };

				[Theory]
				[MemberData(nameof(EnumData), Foo.Bar)]
				[MemberData(nameof(EnumData), (Foo)42)]
				public void TestMethod3(int _) { }

				// https://github.com/xunit/xunit/issues/2852
				public static TheoryData<int> IntegerSequenceData(IEnumerable<int> seq) => new TheoryData<int> { 42, 2112 };

				[Theory]
				[MemberData(nameof(IntegerSequenceData), new int[] { 1, 2 })]
				[MemberData(nameof(IntegerSequenceData), {|#2:new char[] { 'a', 'b' }|})]
				public void TestMethod4(int _) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1035").WithLocation(0).WithArguments("s", "string"),
			Verify.Diagnostic("xUnit1035").WithLocation(1).WithArguments("n", "int"),
			Verify.Diagnostic("xUnit1035").WithLocation(2).WithArguments("seq", "System.Collections.Generic.IEnumerable<int>"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
