using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassShouldHaveTFixtureArgument>;

public class X1033_TestClassShouldHaveTFixtureArgumentFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class NonGenericFixtureData { }

			public class [|NonGenericFixtureDataTestClass|]: IClassFixture<NonGenericFixtureData> {
				[Fact]
				public void TestMethod() { }
			}

			public class GenericFixtureData<T> { }

			public class [|GenericFixtureDataTestClass|]: IClassFixture<GenericFixtureData<object>> {
				[Fact]
				public void TestMethod() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class NonGenericFixtureData { }

			public class [|NonGenericFixtureDataTestClass|]: IClassFixture<NonGenericFixtureData> {
				private readonly NonGenericFixtureData _nonGenericFixtureData;

				public NonGenericFixtureDataTestClass(NonGenericFixtureData nonGenericFixtureData)
				{
					_nonGenericFixtureData = nonGenericFixtureData;
				}

				[Fact]
				public void TestMethod() { }
			}

			public class GenericFixtureData<T> { }

			public class [|GenericFixtureDataTestClass|]: IClassFixture<GenericFixtureData<object>> {
				private readonly GenericFixtureData<object> _genericFixtureData;

				public GenericFixtureDataTestClass(GenericFixtureData<object> genericFixtureData)
				{
					_genericFixtureData = genericFixtureData;
				}

				[Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, TestClassShouldHaveTFixtureArgumentFixer.Key_GenerateConstructor);
	}
}
