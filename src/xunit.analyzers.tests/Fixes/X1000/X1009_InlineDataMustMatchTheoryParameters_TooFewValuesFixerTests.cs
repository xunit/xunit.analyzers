using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class X1009_InlineDataMustMatchTheoryParameters_TooFewValuesFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public enum Color { Red, Green, Blue }

			public class TestClass {
				[Theory]
				[{|xUnit1009:InlineData|}]
				public void Character(char p) { }

				[Theory]
				[{|xUnit1009:InlineData|}]
				public void Double(double p) { }

				[Theory]
				[{|xUnit1009:InlineData|}]
				public void Float(float p) { }

				[Theory]
				[{|xUnit1009:InlineData|}]
				public void Int32(int p) { }

				[Theory]
				[{|xUnit1009:InlineData|}]
				public void Object(object p) { }

				[Theory]
				[{|xUnit1009:InlineData|}]
				public void Enum(Color p) { }

				[Theory]
				[{|xUnit1009:InlineData|}]
				public void StringAndBoolean(string a, bool b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public enum Color { Red, Green, Blue }

			public class TestClass {
				[Theory]
				[InlineData('\0')]
				public void Character(char p) { }

				[Theory]
				[InlineData(0D)]
				public void Double(double p) { }

				[Theory]
				[InlineData(0F)]
				public void Float(float p) { }

				[Theory]
				[InlineData(0)]
				public void Int32(int p) { }

				[Theory]
				[InlineData(null)]
				public void Object(object p) { }

				[Theory]
				[InlineData(default(Color))]
				public void Enum(Color p) { }

				[Theory]
				[InlineData("", false)]
				public void StringAndBoolean(string a, bool b) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, InlineDataMustMatchTheoryParameters_TooFewValuesFixer.Key_AddDefaultValues);
	}
}
