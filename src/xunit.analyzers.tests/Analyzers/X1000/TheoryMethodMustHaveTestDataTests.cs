using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;

public class TheoryMethodMustHaveTestDataTests
{
	[Fact]
	public async Task FactMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task TheoryMethodWithDataAttributes_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData]
				public void TestMethod1() { }

				[Theory]
				[MemberData("")]
				public void TestMethod2() { }

				[Theory]
				[ClassData(typeof(string))]
				public void TestMethod3() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task TheoryMethodWithCustomDataAttribute_v3_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using System.Reflection;
			using System.Threading.Tasks;
			using Xunit;
			using Xunit.Sdk;
			using Xunit.v3;

			public class TestClass {
				[Theory]
				[MyData]
				public void TestMethod() { }
			}

			public class MyData : Attribute, IDataAttribute {
				public bool? Explicit => throw new NotImplementedException();
				public string? Label => throw new NotImplementedException();
				public string? Skip => throw new NotImplementedException();
				public Type? SkipType => throw new NotImplementedException();
				public string? SkipUnless => throw new NotImplementedException();
				public string? SkipWhen => throw new NotImplementedException();
				public string? TestDisplayName => throw new NotImplementedException();
				public int? Timeout => throw new NotImplementedException();
				public string[]? Traits => throw new NotImplementedException();

				public ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
					MethodInfo testMethod,
					DisposalTracker disposalTracker) =>
						throw new NotImplementedException();

				public bool SupportsDiscoveryEnumeration() =>
					throw new NotImplementedException();
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	[Fact]
	public async Task TheoryMethodMissingData_Triggers()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				public void [|TestMethod|]() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
