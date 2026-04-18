using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestCaseMustBeSerializable>;

public class X3007_TestCaseMustBeSerializableTests
{
	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = """
			using Xunit.Sdk;

			[assembly: RegisterXunitSerializer(typeof(MySerializer), typeof(ExternalSerializedTestCase))]

			public class NonTestCase { }

			public abstract class AbstractTestCase : ITestCase { }

			public class SelfSerializedTestCase : ITestCase, IXunitSerializable { }

			public class ExternalSerializedTestCase : ITestCase { }

			public class DerivedTestCase : ExternalSerializedTestCase { }

			public class {|#0:UnserializedTestCase|} : ITestCase { }

			public class MySerializer : IXunitSerializer { }
			""";

		var expected = Verify.Diagnostic("xUnit3007")
			.WithLocation(0)
			.WithArguments("UnserializedTestCase", "Xunit.Sdk.ITestCase", "Xunit.Sdk.IXunitSerializable");

		await Verify.VerifyAnalyzerV3NonAot(CompilerDiagnostics.None, source, expected);
	}
}
