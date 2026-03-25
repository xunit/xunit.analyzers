using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class X1008_DataAttributeShouldBeUsedOnATheoryTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void Fact_NoData_DoesNotTrigger() { }

				[Fact]
				[InlineData]
				public void Fact_InlineData_DoesNotTrigger() { }

				[Fact]
				[MemberData("")]
				public void Fact_MemberData_DoesNotTrigger() { }

				[Fact]
				[ClassData(typeof(string))]
				public void Fact_ClassData_DoesNotTrigger() { }

				[Theory]
				[InlineData]
				public void Theory_InlineData_DoesNotTrigger() { }

				[Theory]
				[MemberData("")]
				public void Theory_MemberData_DoesNotTrigger() { }

				[Theory]
				[ClassData(typeof(string))]
				public void Theory_ClassData_DoesNotTrigger() { }

				[InlineData]
				public void [|NonFact_InlineData_Triggers|]() { }

				[MemberData("")]
				public void [|NonFact_MemberData_Triggers|]() { }

				[ClassData(typeof(string))]
				public void [|NonFact_ClassData_Triggers|]() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[CustomFactViaInheritance]
				[InlineData(1)]
				public void CustomFactViaInheritance_DoesNotTrigger(int i) { }

				[CustomTheoryViaInheritance]
				[InlineData(1)]
				public void CustomTheoryViaInheritance_DoesNotTrigger(int i) { }
			}

			public class CustomFactViaInheritance : FactAttribute { }

			public class CustomTheoryViaInheritance : TheoryAttribute { }
			""";

		await Verify.VerifyAnalyzerNonAot(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[CulturedFact(new[] { "en-US" })]
				[InlineData]
				public void CulturedFact_InlineData_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" })]
				[MemberData("")]
				public void CulturedFact_MemberData_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" })]
				[ClassData(typeof(string))]
				public void CulturedFact_ClassData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				public void CulturedTheory_InlineData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[MemberData("")]
				public void CulturedTheory_MemberData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[ClassData(typeof(string))]
				public void CulturedTheory_ClassData_DoesNotTrigger() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;
			using Xunit.v3;

			public class TestClass {
				// https://github.com/xunit/xunit/issues/3518
				[CustomFactViaInterface]
				[InlineData(1)]
				public void CustomFactViaInterface_DoesNotTrigger(int i) { }

				// https://github.com/xunit/xunit/issues/3518
				[CustomTheoryViaInterface]
				[InlineData(1)]
				public void CustomTheoryViaInterface_DoesNotTrigger(int i) { }
			}

			public class CustomFactViaInterface : Attribute, IFactAttribute
			{
				public string? DisplayName => throw new NotImplementedException();
				public bool Explicit => throw new NotImplementedException();
				public string? Skip => throw new NotImplementedException();
				public Type[]? SkipExceptions => throw new NotImplementedException();
				public Type? SkipType => throw new NotImplementedException();
				public string? SkipUnless => throw new NotImplementedException();
				public string? SkipWhen => throw new NotImplementedException();
				public string? SourceFilePath => throw new NotImplementedException();
				public int? SourceLineNumber => throw new NotImplementedException();
				public int Timeout => throw new NotImplementedException();
			}

			public class CustomTheoryViaInterface : Attribute, ITheoryAttribute
			{
				public bool DisableDiscoveryEnumeration => throw new NotImplementedException();
				public string? DisplayName => throw new NotImplementedException();
				public bool Explicit => throw new NotImplementedException();
				public string? Skip => throw new NotImplementedException();
				public Type[]? SkipExceptions => throw new NotImplementedException();
				public bool SkipTestWithoutData => throw new NotImplementedException();
				public Type? SkipType => throw new NotImplementedException();
				public string? SkipUnless => throw new NotImplementedException();
				public string? SkipWhen => throw new NotImplementedException();
				public string? SourceFilePath => throw new NotImplementedException();
				public int? SourceLineNumber => throw new NotImplementedException();
				public int Timeout => throw new NotImplementedException();
			}
			""";

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source);
	}
}
