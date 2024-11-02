using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify_WithAbstractions = CSharpVerifier<CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithAbstractions.Analyzer>;
using Verify_WithExecution = CSharpVerifier<CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution.Analyzer>;
using Verify_WithRunnerUtility = CSharpVerifier<CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility.Analyzer>;

public class CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests
{
	public class WithAbstractions
	{
		readonly static string Template = /* lang=c#-test */ """
			using Xunit.Abstractions;

			public class {{|#0:MyClass|}}: {0} {{ }}
			""";

		public static TheoryData<string> Interfaces =
		[
			// Discovery and execution messages
			MemberCount("IMessageSink", 1),
			MemberCount("IMessageSinkMessage", 0),

			// Reflection
			MemberCount("IAssemblyInfo", 5),
			MemberCount("IAttributeInfo", 3),
			MemberCount("IMethodInfo", 11),
			MemberCount("IParameterInfo", 2),
			MemberCount("ITypeInfo", 13),

			// Test cases
			MemberCount("ITest", 2),
			MemberCount("ITestAssembly", 4),
			MemberCount("ITestCase", 9),
			MemberCount("ITestClass", 4),
			MemberCount("ITestCollection", 6),
			MemberCount("ITestMethod", 4),

			// Test frameworks
			MemberCount("ISourceInformation", 4),
			MemberCount("ISourceInformationProvider", 2),
			MemberCount("ITestFramework", 4),
			MemberCount("ITestFrameworkDiscoverer", 6),
			MemberCount("ITestFrameworkExecutor", 4),
		];

		[Fact]
		public async Task NoInterfaces_DoesNotTrigger()
		{
			var source = "public class Foo { }";

			await Verify_WithAbstractions.VerifyAnalyzerV2(source);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task InterfaceWithoutBaseClass_Triggers(string @interface)
		{
			var source = string.Format(Template, @interface);
			var expected = Verify_WithAbstractions.Diagnostic().WithLocation(0).WithArguments("MyClass");

			await Verify_WithAbstractions.VerifyAnalyzerV2(source, expected);
		}

		internal class Analyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
		{
			protected override XunitContext CreateXunitContext(Compilation compilation) =>
				XunitContext.ForV2Abstractions(compilation);
		}
	}

	public class WithExecution
	{
		readonly static string Template = /* lang=c#-test */ """
			using Xunit.Abstractions;

			public class Foo {{ }}
			public class MyLLMBRO: Xunit.LongLivedMarshalByRefObject {{ }}
			public class {{|#0:MyClass|}}: {0} {{ }}
			""";

		public static TheoryData<string> Interfaces = new(WithAbstractions.Interfaces) { MemberCount("Xunit.Sdk.IXunitTestCase", 13) };

		public static TheoryData<string, string> InterfacesWithBaseClasses
		{
			get
			{
				var result = new TheoryData<string, string>();

				foreach (var @interface in Interfaces)
				{
					result.Add(@interface.Data, "MyLLMBRO");
					result.Add(@interface.Data, "Xunit.LongLivedMarshalByRefObject");
				}

				return result;
			}
		}

		[Fact]
		public async Task NoInterfaces_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ "public class Foo { }";

			await Verify_WithExecution.VerifyAnalyzerV2(source);
		}

		[Fact]
		public async Task WithXunitTestCase_DoesNotTrigger()
		{
			var source = string.Format(Template, "Xunit.Sdk.XunitTestCase");

			await Verify_WithExecution.VerifyAnalyzerV2(source);
		}

		[Theory]
		[MemberData(nameof(InterfacesWithBaseClasses))]
		public async Task CompatibleBaseClass_DoesNotTrigger(
			string @interface,
			string baseClass)
		{
			var source = string.Format(Template, $"{baseClass}, {@interface}");

			await Verify_WithExecution.VerifyAnalyzerV2(source);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task InterfaceWithoutBaseClass_Triggers(string @interface)
		{
			var source = string.Format(Template, @interface);
			var expected = Verify_WithExecution.Diagnostic().WithLocation(0).WithArguments("MyClass");

			await Verify_WithExecution.VerifyAnalyzerV2(source, expected);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task IncompatibleBaseClass_Triggers(string @interface)
		{
			var source = string.Format(Template, $"Foo, {@interface}");
			var expected = Verify_WithExecution.Diagnostic().WithLocation(0).WithArguments("MyClass");

			await Verify_WithExecution.VerifyAnalyzerV2(source, expected);
		}

		internal class Analyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
		{
			protected override XunitContext CreateXunitContext(Compilation compilation) =>
				XunitContext.ForV2Execution(compilation);
		}
	}

	public class WithRunnerUtility
	{
		readonly static string Template = /* lang=c#-test */ """
			using Xunit.Abstractions;

			public class Foo {{ }}
			public class MyLLMBRO: Xunit.Sdk.LongLivedMarshalByRefObject {{ }}
			public class {{|#0:MyClass|}}: {0} {{ }}
			""";

		public static TheoryData<string> Interfaces =
			WithAbstractions.Interfaces;

		public static TheoryData<string, string> InterfacesWithBaseClasses
		{
			get
			{
				var result = new TheoryData<string, string>();

				foreach (var @interface in Interfaces)
				{
					result.Add(@interface.Data, "MyLLMBRO");
					result.Add(@interface.Data, "Xunit.Sdk.LongLivedMarshalByRefObject");
				}

				return result;
			}
		}

		[Fact]
		public async Task NoInterfaces_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ "public class Foo { }";

			await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(source);
		}

		[Theory]
		[MemberData(nameof(InterfacesWithBaseClasses))]
		public async Task CompatibleBaseClass_DoesNotTrigger(
			string @interface,
			string baseClass)
		{
			var source = string.Format(Template, $"{baseClass}, {@interface}");

			await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(source);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task InterfaceWithoutBaseClass_Triggers(string @interface)
		{
			var source = string.Format(Template, @interface);
			var expected = Verify_WithRunnerUtility.Diagnostic().WithLocation(0).WithArguments("MyClass");

			await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(source, expected);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task IncompatibleBaseClass_Triggers(string @interface)
		{
			var source = string.Format(Template, $"Foo, {@interface}");
			var expected = Verify_WithRunnerUtility.Diagnostic().WithLocation(0).WithArguments("MyClass");

			await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(source, expected);
		}

		internal class Analyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
		{
			protected override XunitContext CreateXunitContext(Compilation compilation) =>
				XunitContext.ForV2RunnerUtility(compilation);
		}
	}

	public static string MemberCount(
		string memberName,
		int memberCount)
	{
		var result = memberName;

		while (memberCount-- > 0)
			result = $"{{|CS0535:{result}|}}";

		return result;
	}
}
