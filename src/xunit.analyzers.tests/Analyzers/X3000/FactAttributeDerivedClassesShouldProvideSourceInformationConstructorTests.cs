using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.FactAttributeDerivedClassesShouldProvideSourceInformationConstructor>;
using Verify_v3_Pre300 = CSharpVerifier<FactAttributeDerivedClassesShouldProvideSourceInformationConstructorTests.Analyzer_v3_Pre300>;

public class FactAttributeDerivedClassesShouldProvideSourceInformationConstructorTests
{
	[Fact]
	public async Task v2_OlderV3_DoesNotTrigger()
	{
		var code = /* lang=c#-test */ """
			using Xunit;

			public class MyFactAttribute : FactAttribute { }

			public class MyTheoryAttribute : TheoryAttribute { }
			""";

		await Verify.VerifyAnalyzerV2(code);
		await Verify_v3_Pre300.VerifyAnalyzerV3(code);
	}


	[Fact]
	public async Task v3_Triggers()
	{
#if ROSLYN_LATEST
		var code = /* lang=c#-test */ """
			using System.Runtime.CompilerServices;
			using Xunit;

			public class {|xUnit3003:MyFactAttribute|} : FactAttribute { }

			public class MyFactWithCtorArgs([CallerFilePath] string? foo = null, [CallerLineNumber] int bar = -1)
				: FactAttribute(foo, bar)
			{ }

			public class {|xUnit3003:MyTheoryAttribute|} : TheoryAttribute { }

			public class MyTheoryWithCtorArgs(
				int x,
				string y,
				[CallerFilePath] string? sourceFilePath = null,
				[CallerLineNumber] int sourceLineNumber = -1)
					: TheoryAttribute(sourceFilePath, sourceLineNumber)
			{ }
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp12, code);
#else
		var code = /* lang=c#-test */ """
			using System.Runtime.CompilerServices;
			using Xunit;

			public class {|xUnit3003:MyFactAttribute|} : FactAttribute { }

			public class MyFactWithCtorArgs : FactAttribute
			{
				public MyFactWithCtorArgs([CallerFilePath] string? foo = null, [CallerLineNumber] int bar = -1)
					: base(foo, bar)
				{ }
			}

			public class {|xUnit3003:MyTheoryAttribute|} : TheoryAttribute { }

			public class MyTheoryWithCtorArgs : TheoryAttribute
			{
				public MyTheoryWithCtorArgs(int x, string y, [CallerFilePath] string? foo = null, [CallerLineNumber] int bar = -1)
					: base(foo, bar)
				{ }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, code);
#endif
	}

	internal class Analyzer_v3_Pre300 : FactAttributeDerivedClassesShouldProvideSourceInformationConstructor
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(2, 999, 999));
	}
}
