using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;

public class X1000_TestClassMustBePublicTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source1 = /* lang=c#-test */ """
			using Xunit;

			public class Fact_ForPublicClass_DoesNotTrigger {
				[Fact]
				public void TestMethod() { }
			}

			public class Theory_ForPublicClass_DoesNotTrigger {
				[Theory]
				public void TestMethod() { }
			}

			class [|Fact_ForInternalClass_Triggers|] {
				[Fact]
				public void TestMethod() { }
			}

			class [|Theory_ForInternalClass_Triggers|] {
				[Theory]
				public void TestMethod() { }
			}

			public class ParentClass {
				internal class [|Fact_ForInternalClass_Triggers|] {
					[Fact]
					public void TestMethod() { }
				}

				internal class [|Theory_ForInternalClass_Triggers|] {
					[Theory]
					public void TestMethod() { }
				}

				class [|Fact_ForPrivateClass_Triggers|] {
					[Fact]
					public void TestMethod() { }
				}

				class [|Theory_ForPrivateClass_Triggers|] {
					[Theory]
					public void TestMethod() { }
				}
			}

			// Partials in the same file

			public partial class Fact_PublicPartialInSameFile_DoesNotTrigger {
				[Fact]
				public void Test1() { }
			}

			partial class Fact_PublicPartialInSameFile_DoesNotTrigger {
				[Fact]
				public void Test2() { }
			}

			public partial class Theory_PublicPartialInSameFile_DoesNotTrigger {
				[Theory]
				public void Test1() { }
			}

			partial class Theory_PublicPartialInSameFile_DoesNotTrigger {
				[Theory]
				public void Test2() { }
			}

			// Partials spread across separate files

			public partial class Fact_PublicPartialInSeparateFile_DoesNotTrigger {
				[Fact]
				public void Test1() { }
			}

			public partial class Theory_PublicPartialInSeparateFile_DoesNotTrigger {
				[Theory]
				public void Test1() { }
			}

			partial class {|#0:Fact_InternalPartialInSeparateFile_Triggers|} {
				[Fact]
				public void Test1() { }
			}

			partial class {|#10:Theory_InternalPartialInSeparateFile_Triggers|} {
				[Theory]
				public void Test1() { }
			}
			""";
		var source2 = /* lang=c#-test */ """
			using Xunit;

			partial class Fact_PublicPartialInSeparateFile_DoesNotTrigger {
				[Fact]
				public void Test2() { }
			}

			partial class Theory_PublicPartialInSeparateFile_DoesNotTrigger {
				[Theory]
				public void Test2() { }
			}

			partial class {|#1:Fact_InternalPartialInSeparateFile_Triggers|} {
				[Fact]
				public void Test2() { }
			}

			partial class {|#11:Theory_InternalPartialInSeparateFile_Triggers|} {
				[Theory]
				public void Test2() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithLocation(1),
			Verify.Diagnostic().WithLocation(10).WithLocation(11),
		};

		await Verify.VerifyAnalyzer([source1, source2], expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source1 = /* lang=c#-test */ """
			using Xunit;

			public class Fact_ForPublicClass_DoesNotTrigger {
				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			public class Theory_ForPublicClass_DoesNotTrigger {
				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}

			class [|Fact_ForInternalClass_Triggers|] {
				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			class [|Theory_ForInternalClass_Triggers|] {
				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}

			public class ParentClass {
				internal class [|Fact_ForInternalClass_Triggers|] {
					[CulturedFact(new[] { "en-US" })]
					public void TestMethod() { }
				}

				internal class [|Theory_ForInternalClass_Triggers|] {
					[CulturedTheory(new[] { "en-US" })]
					public void TestMethod() { }
				}

				class [|Fact_ForPrivateClass_Triggers|] {
					[CulturedFact(new[] { "en-US" })]
					public void TestMethod() { }
				}

				class [|Theory_ForPrivateClass_Triggers|] {
					[CulturedTheory(new[] { "en-US" })]
					public void TestMethod() { }
				}
			}

			// Partials in the same file

			public partial class Fact_PublicPartialInSameFile_DoesNotTrigger {
				[CulturedFact(new[] { "en-US" })]
				public void Test1() { }
			}

			partial class Fact_PublicPartialInSameFile_DoesNotTrigger {
				[CulturedFact(new[] { "en-US" })]
				public void Test2() { }
			}

			public partial class Theory_PublicPartialInSameFile_DoesNotTrigger {
				[CulturedTheory(new[] { "en-US" })]
				public void Test1() { }
			}

			partial class Theory_PublicPartialInSameFile_DoesNotTrigger {
				[CulturedTheory(new[] { "en-US" })]
				public void Test2() { }
			}

			// Partials spread across separate files

			public partial class Fact_PublicPartialInSeparateFile_DoesNotTrigger {
				[CulturedFact(new[] { "en-US" })]
				public void Test1() { }
			}

			public partial class Theory_PublicPartialInSeparateFile_DoesNotTrigger {
				[CulturedTheory(new[] { "en-US" })]
				public void Test1() { }
			}

			partial class {|#0:Fact_InternalPartialInSeparateFile_Triggers|} {
				[CulturedFact(new[] { "en-US" })]
				public void Test1() { }
			}

			partial class {|#10:Theory_InternalPartialInSeparateFile_Triggers|} {
				[CulturedTheory(new[] { "en-US" })]
				public void Test1() { }
			}
			""";
		var source2 = /* lang=c#-test */ """
			using Xunit;

			partial class Fact_PublicPartialInSeparateFile_DoesNotTrigger {
				[CulturedFact(new[] { "en-US" })]
				public void Test2() { }
			}

			partial class Theory_PublicPartialInSeparateFile_DoesNotTrigger {
				[CulturedTheory(new[] { "en-US" })]
				public void Test2() { }
			}

			partial class {|#1:Fact_InternalPartialInSeparateFile_Triggers|} {
				[CulturedFact(new[] { "en-US" })]
				public void Test2() { }
			}

			partial class {|#11:Theory_InternalPartialInSeparateFile_Triggers|} {
				[CulturedTheory(new[] { "en-US" })]
				public void Test2() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithLocation(1),
			Verify.Diagnostic().WithLocation(10).WithLocation(11),
		};

		await Verify.VerifyAnalyzerV3([source1, source2], expected);
	}
}
