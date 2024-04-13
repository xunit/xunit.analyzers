using System.Collections.Generic;
using System.Linq;
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
		readonly static string Template = @"
using Xunit.Abstractions;

public class MyClass: {0} {{ }}";

		public static IEnumerable<object[]> Interfaces
		{
			get
			{
				// Discovery and execution messages
				yield return new object[] { MemberCount("IMessageSink", 1) };
				yield return new object[] { MemberCount("IMessageSinkMessage", 0) };

				// Reflection
				yield return new object[] { MemberCount("IAssemblyInfo", 5) };
				yield return new object[] { MemberCount("IAttributeInfo", 3) };
				yield return new object[] { MemberCount("IMethodInfo", 11) };
				yield return new object[] { MemberCount("IParameterInfo", 2) };
				yield return new object[] { MemberCount("ITypeInfo", 13) };

				// Test cases
				yield return new object[] { MemberCount("ITest", 2) };
				yield return new object[] { MemberCount("ITestAssembly", 4) };
				yield return new object[] { MemberCount("ITestCase", 9) };
				yield return new object[] { MemberCount("ITestClass", 4) };
				yield return new object[] { MemberCount("ITestCollection", 6) };
				yield return new object[] { MemberCount("ITestMethod", 4) };

				// Test frameworks
				yield return new object[] { MemberCount("ISourceInformation", 4) };
				yield return new object[] { MemberCount("ISourceInformationProvider", 2) };
				yield return new object[] { MemberCount("ITestFramework", 4) };
				yield return new object[] { MemberCount("ITestFrameworkDiscoverer", 6) };
				yield return new object[] { MemberCount("ITestFrameworkExecutor", 4) };
			}
		}

		[Fact]
		public async Task SuccessCase_NoInterfaces()
		{
			var source = "public class Foo { }";

			await Verify_WithAbstractions.VerifyAnalyzerV2(source);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task FailureCase_InterfaceWithoutBaseClass(string @interface)
		{
			var source = string.Format(Template, @interface);
			var expected =
				Verify_WithAbstractions
					.Diagnostic()
					.WithSpan(4, 14, 4, 21)
					.WithArguments("MyClass");

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
		readonly static string Template = @"
using Xunit.Abstractions;

public class Foo {{ }}
public class MyLLMBRO: Xunit.LongLivedMarshalByRefObject {{ }}
public class MyClass: {0} {{ }}";

		public static IEnumerable<object[]> Interfaces
		{
			get
			{
				foreach (var @interface in WithAbstractions.Interfaces)
					yield return @interface;

				yield return new object[] { MemberCount("Xunit.Sdk.IXunitTestCase", 13) };
			}
		}

		public static TheoryData<string, string> InterfacesWithBaseClasses
		{
			get
			{
				var result = new TheoryData<string, string>();

				foreach (var @interface in Interfaces.Select(x => (string)x[0]))
				{
					result.Add(@interface, "MyLLMBRO");
					result.Add(@interface, "Xunit.LongLivedMarshalByRefObject");
				}

				return result;
			}
		}

		[Fact]
		public async Task SuccessCase_NoInterfaces()
		{
			var source = "public class Foo { }";

			await Verify_WithExecution.VerifyAnalyzerV2(source);
		}

		[Fact]
		public async Task SuccessCase_WithXunitTestCase()
		{
			var source = string.Format(Template, "Xunit.Sdk.XunitTestCase");

			await Verify_WithExecution.VerifyAnalyzerV2(source);
		}

		[Theory]
		[MemberData(nameof(InterfacesWithBaseClasses))]
		public async Task SuccessCase_CompatibleBaseClass(
			string @interface,
			string baseClass)
		{
			var source = string.Format(Template, $"{baseClass}, {@interface}");

			await Verify_WithExecution.VerifyAnalyzerV2(source);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task FailureCase_InterfaceWithoutBaseClass(string @interface)
		{
			var source = string.Format(Template, @interface);
			var expected =
				Verify_WithExecution
					.Diagnostic()
					.WithSpan(6, 14, 6, 21)
					.WithArguments("MyClass");

			await Verify_WithExecution.VerifyAnalyzerV2(source, expected);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task FailureCase_IncompatibleBaseClass(string @interface)
		{
			var source = string.Format(Template, $"Foo, {@interface}");
			var expected =
				Verify_WithExecution
					.Diagnostic()
					.WithSpan(6, 14, 6, 21)
					.WithArguments("MyClass");

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
		readonly static string Template = @"
using Xunit.Abstractions;

public class Foo {{ }}
public class MyLLMBRO: Xunit.Sdk.LongLivedMarshalByRefObject {{ }}
public class MyClass: {0} {{ }}";

		public static IEnumerable<object[]> Interfaces =>
			WithAbstractions.Interfaces;

		public static TheoryData<string, string> InterfacesWithBaseClasses
		{
			get
			{
				var result = new TheoryData<string, string>();

				foreach (var @interface in Interfaces.Select(x => (string)x[0]))
				{
					result.Add(@interface, "MyLLMBRO");
					result.Add(@interface, "Xunit.Sdk.LongLivedMarshalByRefObject");
				}

				return result;
			}
		}

		[Fact]
		public async Task SuccessCase_NoInterfaces()
		{
			var source = "public class Foo { }";

			await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(source);
		}

		[Theory]
		[MemberData(nameof(InterfacesWithBaseClasses))]
		public async Task SuccessCase_CompatibleBaseClass(
			string @interface,
			string baseClass)
		{
			var source = string.Format(Template, $"{baseClass}, {@interface}");

			await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(source);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task FailureCase_InterfaceWithoutBaseClass(string @interface)
		{
			var source = string.Format(Template, @interface);
			var expected =
				Verify_WithRunnerUtility
					.Diagnostic()
					.WithSpan(6, 14, 6, 21)
					.WithArguments("MyClass");

			await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(source, expected);
		}

		[Theory]
		[MemberData(nameof(Interfaces))]
		public async Task FailureCase_IncompatibleBaseClass(string @interface)
		{
			var source = string.Format(Template, $"Foo, {@interface}");
			var expected =
				Verify_WithRunnerUtility
					.Diagnostic()
					.WithSpan(6, 14, 6, 21)
					.WithArguments("MyClass");

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
