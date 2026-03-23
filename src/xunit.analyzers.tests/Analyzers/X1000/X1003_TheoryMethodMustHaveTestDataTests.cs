using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;

public class X1003_TheoryMethodMustHaveTestDataTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void FactMethod_DoesNotTrigger() { }

				[Theory]
				[InlineData]
				public void TheoryMethodWithInlineData_DoesNotTrigger() { }

				[Theory]
				[MemberData("")]
				public void TheoryMethodWithMemberData_DoesNotTrigger() { }

				[Theory]
				[ClassData(typeof(string))]
				public void TheoryMethodWithClassData_DoesNotTrigger() { }

				[Theory]
				public void [|TheoryMethodWithoutData_Triggers|]() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[CulturedFact(new[] { "en-US" })]
				public void FactMethod_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				public void TheoryMethodWithInlineData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[MemberData("")]
				public void TheoryMethodWithMemberData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[ClassData(typeof(string))]
				public void TheoryMethodWithClassData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				public void [|TheoryMethodWithoutData_Triggers|]() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async ValueTask V3_only_NonAOT()
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
				public void TheoryMethodWithCustomData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[MyData]
				public void CulturedTheoryMethodWithCustomData_DoesNotTrigger() { }
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

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source);
	}

#if NETCOREAPP && ROSLYN_LATEST

	[Fact]
	public async ValueTask V3_only_AOT()
	{
		var source = /* lang=c#-test */ """
			using Xunit;
			using Xunit.v3;

			public class TestClass {
				[Theory]
				[MyData]
				public void TheoryMethodWithCustomData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[MyData]
				public void CulturedTheoryMethodWithCustomData_DoesNotTrigger() { }
			}

			public class MyData : DataAttribute { }
			""";

		await Verify.VerifyAnalyzerV3Aot(LanguageVersion.CSharp8, source);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST
}
