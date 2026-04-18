using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestCaseMustBeSerializable>;

public class X3006_TestCaseMustBeSerializableTests
{
	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = """
			using Xunit.Sdk;

			[assembly: RegisterXunitSerializer(typeof(MySerializer), typeof(ExternalSerializedTestCase))]

			public class NonTestCase { }

			public abstract class AbstractTestCase : ITestCase { }

			public sealed class SelfSerializedTestCase : ITestCase, IXunitSerializable { }

			public sealed class ExternalSerializedTestCase : ITestCase { }

			public sealed class {|#0:UnserializedTestCase|} : ITestCase { }

			public class MySerializer : IXunitSerializer { }
			""";

		var expected = Verify.Diagnostic("xUnit3006")
			.WithLocation(0)
			.WithArguments("UnserializedTestCase", "Xunit.Sdk.ITestCase", "Xunit.Sdk.IXunitSerializable");

		await Verify.VerifyAnalyzerV3NonAot(CompilerDiagnostics.None, source, expected);
	}
}
