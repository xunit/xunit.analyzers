using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodsFromReferencedAssembliesInAot>;

public class X1072_TestMethodsFromReferencedAssembliesInAotTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var referenceSource = /* lang=c#-test */ """
			using Xunit;

			namespace External.Tests {
				public abstract class FactBase {
					[Fact]
					public void Fact() { }
				}

				public abstract class TheoryBase {
					[Theory]
					[InlineData(42)]
					public void Theory(int value) { }
				}

				public abstract class FactAndTheoryBase {
					[Fact]
					public void FirstFact() { }

					[Theory]
					[InlineData(42)]
					public void SecondTheory(int value) { }
				}

				public abstract class GenericBase<T> {
					[Fact]
					public void GenericFact() { }
				}

				public abstract class IntermediateBase : FactBase { }

				public abstract class OrdinaryBase {
					public void OrdinaryMethod() { }

					[Fact]
					protected void ProtectedFact() { }

					[Theory]
					[InlineData(42)]
					internal void InternalTheory(int value) { }
				}

				public abstract class AbstractMethodBase {
					[Fact]
					public abstract void AbstractFact();
				}

				public abstract class VirtualFactBase {
					[Fact]
					public virtual void VirtualFact() { }
				}
			}
			""";

		var source = /* lang=c#-test */ """
			using External.Tests;
			using Xunit;

			public class {|#0:FactTests|} : FactBase { }
			public class {|#1:TheoryTests|} : TheoryBase { }
			public class {|#2:FactAndTheoryTests|} : FactAndTheoryBase { }
			public class {|#3:GenericTests|} : GenericBase<int> { }
			public class {|#4:ExternalMultilevelTests|} : IntermediateBase { }

			public abstract class SourceIntermediateBase : FactBase { }
			public class {|#5:SourceMultilevelTests|} : SourceIntermediateBase { }

			public partial class {|#6:PartialTests|} : FactBase { }
			public partial class PartialTests {
				[Fact]
				public void LocalFact() { }
			}

			public class {|#7:HiddenTests|} : FactBase {
				[Fact]
				public new void Fact() { }
			}

			public class Container {
				public class {|#8:NestedTests|} : FactBase { }
			}

			public class OrdinaryTests : OrdinaryBase { }
			public abstract class AbstractTests : FactBase { }
			internal class InternalTests : FactBase { }
			public class NonTestContainer {
				private class PrivateTests : FactBase { }
			}
			public class {|#10:ImplementedAbstractTests|} : AbstractMethodBase {
				public override void AbstractFact() { }
			}
			public class AnnotatedAbstractOverrideTests : AbstractMethodBase {
				[Fact]
				public override void AbstractFact() { }
			}
			public class AnnotatedOverrideTests : VirtualFactBase {
				[Fact]
				public override void VirtualFact() { }
			}
			public class {|#9:UnannotatedOverrideTests|} : VirtualFactBase {
				public override void VirtualFact() { }
			}
			public abstract class SourceAnnotatedOverrideBase : VirtualFactBase {
				[Fact]
				public override void VirtualFact() { }
			}
			public class InheritedSourceOverrideTests : SourceAnnotatedOverrideBase { }

			public abstract class SourceBase {
				[Fact]
				public void SourceFact() { }

				[Theory]
				[InlineData(42)]
				public void SourceTheory(int value) { }
			}
			public class SourceTests : SourceBase { }
			public class OwnTests {
				[Fact]
				public void OwnFact() { }
			}
			""";

		await Verify.VerifyAnalyzerNonAotWithMetadataReference(source, referenceSource);
#if NETCOREAPP && ROSLYN_LATEST
		await Verify.VerifyAnalyzerV3AotWithMetadataReference(
			source,
			referenceSource,
			Verify.Diagnostic().WithLocation(0).WithArguments("External.Tests.FactBase", "Fact"),
			Verify.Diagnostic().WithLocation(1).WithArguments("External.Tests.TheoryBase", "Theory"),
			Verify.Diagnostic().WithLocation(2).WithArguments("External.Tests.FactAndTheoryBase", "FirstFact"),
			Verify.Diagnostic().WithLocation(2).WithArguments("External.Tests.FactAndTheoryBase", "SecondTheory"),
			Verify.Diagnostic().WithLocation(3).WithArguments("External.Tests.GenericBase<int>", "GenericFact"),
			Verify.Diagnostic().WithLocation(4).WithArguments("External.Tests.FactBase", "Fact"),
			Verify.Diagnostic().WithLocation(5).WithArguments("External.Tests.FactBase", "Fact"),
			Verify.Diagnostic().WithLocation(6).WithArguments("External.Tests.FactBase", "Fact"),
			Verify.Diagnostic().WithLocation(7).WithArguments("External.Tests.FactBase", "Fact"),
			Verify.Diagnostic().WithLocation(8).WithArguments("External.Tests.FactBase", "Fact"),
			Verify.Diagnostic().WithLocation(9).WithArguments("External.Tests.VirtualFactBase", "VirtualFact"),
			Verify.Diagnostic().WithLocation(10).WithArguments("External.Tests.AbstractMethodBase", "AbstractFact")
		);
#endif
	}
}
