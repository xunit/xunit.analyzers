using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify_WithAbstractions = CSharpVerifier<X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests.AbstractionsAnalyzer>;
using Verify_WithExecution = CSharpVerifier<X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests.ExecutionAnalyzer>;
using Verify_WithRunnerUtility = CSharpVerifier<X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests.RunnerUtilityAnalyzer>;

public class X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests
{
	[Fact]
	public async ValueTask WithAbstractions_DoesNotAttemptToFix()
	{
		var source = /* lang=c#-test */ $$"""
			using Xunit.Abstractions;

			// Discovery and execution messages
			class [|IMessageSink_Triggers|] : {{MemberCount("IMessageSink", 1)}} { }
			class [|IMessageSinkMessage_Triggers|] : {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class [|IAssemblyInfo_Triggers|] : {{MemberCount("IAssemblyInfo", 5)}} { }
			class [|IAttributeInfo_Triggers|] : {{MemberCount("IAttributeInfo", 3)}} { }
			class [|IMethodInfo_Triggers|] : {{MemberCount("IMethodInfo", 11)}} { }
			class [|IParameterInfo_Triggers|] : {{MemberCount("IParameterInfo", 2)}} { }
			class [|ITypeInfo_Triggers|] : {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class [|ITest_Triggers|] : {{MemberCount("ITest", 2)}} { }
			class [|ITestAssembly_Triggers|] : {{MemberCount("ITestAssembly", 4)}} { }
			class [|ITestCase_Triggers|] : {{MemberCount("ITestCase", 9)}} { }
			class [|ITestClass_Triggers|] : {{MemberCount("ITestClass", 4)}} { }
			class [|ITestCollection_Triggers|] : {{MemberCount("ITestCollection", 6)}} { }
			class [|ITestMethod_Triggers|] : {{MemberCount("ITestMethod", 4)}} { }

			// Test framework
			class [|ISourceInformation_Triggers|] : {{MemberCount("ISourceInformation", 4)}} { }
			class [|ISourceInformationProvider_Triggers|] : {{MemberCount("ISourceInformationProvider", 2)}} { }
			class [|ITestFramework_Triggers|] : {{MemberCount("ITestFramework", 4)}} { }
			class [|ITestFrameworkDiscoverer_Triggers|] : {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class [|ITestFrameworkExecutor_Triggers|] : {{MemberCount("ITestFrameworkExecutor", 4)}} { }
			""";

		await Verify_WithAbstractions.VerifyCodeFixV2(source, source, CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType);
	}

	[Fact]
	public async ValueTask WithExecution_Fixes()
	{
		var before = /* lang=c#-test */ $$"""
			using Xunit.Abstractions;
			using Xunit.Sdk;

			// Discovery and execution messages
			class [|IMessageSink_Triggers|] : {{MemberCount("IMessageSink", 1)}} { }
			class [|IMessageSinkMessage_Triggers|] : {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class [|IAssemblyInfo_Triggers|] : {{MemberCount("IAssemblyInfo", 5)}} { }
			class [|IAttributeInfo_Triggers|] : {{MemberCount("IAttributeInfo", 3)}} { }
			class [|IMethodInfo_Triggers|] : {{MemberCount("IMethodInfo", 11)}} { }
			class [|IParameterInfo_Triggers|] : {{MemberCount("IParameterInfo", 2)}} { }
			class [|ITypeInfo_Triggers|] : {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class [|ITest_Triggers|] : {{MemberCount("ITest", 2)}} { }
			class [|ITestAssembly_Triggers|] : {{MemberCount("ITestAssembly", 4)}} { }
			class [|ITestCase_Triggers|] : {{MemberCount("ITestCase", 9)}} { }
			class [|ITestClass_Triggers|] : {{MemberCount("ITestClass", 4)}} { }
			class [|ITestCollection_Triggers|] : {{MemberCount("ITestCollection", 6)}} { }
			class [|ITestMethod_Triggers|] : {{MemberCount("ITestMethod", 4)}} { }
			class [|IXunitTestCase_Triggers|] : {{MemberCount("IXunitTestCase", 13)}} { }

			// Test framework
			class [|ISourceInformation_Triggers|] : {{MemberCount("ISourceInformation", 4)}} { }
			class [|ISourceInformationProvider_Triggers|] : {{MemberCount("ISourceInformationProvider", 2)}} { }
			class [|ITestFramework_Triggers|] : {{MemberCount("ITestFramework", 4)}} { }
			class [|ITestFrameworkDiscoverer_Triggers|] : {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class [|ITestFrameworkExecutor_Triggers|] : {{MemberCount("ITestFrameworkExecutor", 4)}} { }
			""";
		var after = /* lang=c#-test */ $$"""
			using Xunit.Abstractions;
			using Xunit.Sdk;

			// Discovery and execution messages
			class IMessageSink_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IMessageSink", 1)}} { }
			class IMessageSinkMessage_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class IAssemblyInfo_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IAssemblyInfo", 5)}} { }
			class IAttributeInfo_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IAttributeInfo", 3)}} { }
			class IMethodInfo_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IMethodInfo", 11)}} { }
			class IParameterInfo_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IParameterInfo", 2)}} { }
			class ITypeInfo_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class ITest_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITest", 2)}} { }
			class ITestAssembly_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestAssembly", 4)}} { }
			class ITestCase_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestCase", 9)}} { }
			class ITestClass_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestClass", 4)}} { }
			class ITestCollection_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestCollection", 6)}} { }
			class ITestMethod_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestMethod", 4)}} { }
			class IXunitTestCase_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IXunitTestCase", 13)}} { }

			// Test framework
			class ISourceInformation_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ISourceInformation", 4)}} { }
			class ISourceInformationProvider_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ISourceInformationProvider", 2)}} { }
			class ITestFramework_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestFramework", 4)}} { }
			class ITestFrameworkDiscoverer_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class ITestFrameworkExecutor_Triggers : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestFrameworkExecutor", 4)}} { }
			""";

		await Verify_WithExecution.VerifyCodeFixV2(before, after, CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType);
	}

	[Fact]
	public async ValueTask WithRunnerUtility_Fixes()
	{
		var before = /* lang=c#-test */ $$"""
			using Xunit.Abstractions;

			// Discovery and execution messages
			class [|IMessageSink_Triggers|] : {{MemberCount("IMessageSink", 1)}} { }
			class [|IMessageSinkMessage_Triggers|] : {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class [|IAssemblyInfo_Triggers|] : {{MemberCount("IAssemblyInfo", 5)}} { }
			class [|IAttributeInfo_Triggers|] : {{MemberCount("IAttributeInfo", 3)}} { }
			class [|IMethodInfo_Triggers|] : {{MemberCount("IMethodInfo", 11)}} { }
			class [|IParameterInfo_Triggers|] : {{MemberCount("IParameterInfo", 2)}} { }
			class [|ITypeInfo_Triggers|] : {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class [|ITest_Triggers|] : {{MemberCount("ITest", 2)}} { }
			class [|ITestAssembly_Triggers|] : {{MemberCount("ITestAssembly", 4)}} { }
			class [|ITestCase_Triggers|] : {{MemberCount("ITestCase", 9)}} { }
			class [|ITestClass_Triggers|] : {{MemberCount("ITestClass", 4)}} { }
			class [|ITestCollection_Triggers|] : {{MemberCount("ITestCollection", 6)}} { }
			class [|ITestMethod_Triggers|] : {{MemberCount("ITestMethod", 4)}} { }

			// Test framework
			class [|ISourceInformation_Triggers|] : {{MemberCount("ISourceInformation", 4)}} { }
			class [|ISourceInformationProvider_Triggers|] : {{MemberCount("ISourceInformationProvider", 2)}} { }
			class [|ITestFramework_Triggers|] : {{MemberCount("ITestFramework", 4)}} { }
			class [|ITestFrameworkDiscoverer_Triggers|] : {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class [|ITestFrameworkExecutor_Triggers|] : {{MemberCount("ITestFrameworkExecutor", 4)}} { }
			""";
		var after = /* lang=c#-test */ $$"""
			using Xunit.Abstractions;

			// Discovery and execution messages
			class IMessageSink_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("IMessageSink", 1)}} { }
			class IMessageSinkMessage_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class IAssemblyInfo_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("IAssemblyInfo", 5)}} { }
			class IAttributeInfo_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("IAttributeInfo", 3)}} { }
			class IMethodInfo_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("IMethodInfo", 11)}} { }
			class IParameterInfo_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("IParameterInfo", 2)}} { }
			class ITypeInfo_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class ITest_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITest", 2)}} { }
			class ITestAssembly_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITestAssembly", 4)}} { }
			class ITestCase_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITestCase", 9)}} { }
			class ITestClass_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITestClass", 4)}} { }
			class ITestCollection_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITestCollection", 6)}} { }
			class ITestMethod_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITestMethod", 4)}} { }

			// Test framework
			class ISourceInformation_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ISourceInformation", 4)}} { }
			class ISourceInformationProvider_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ISourceInformationProvider", 2)}} { }
			class ITestFramework_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITestFramework", 4)}} { }
			class ITestFrameworkDiscoverer_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class ITestFrameworkExecutor_Triggers : Xunit.Sdk.LongLivedMarshalByRefObject, {{MemberCount("ITestFrameworkExecutor", 4)}} { }
			""";

		await Verify_WithRunnerUtility.VerifyCodeFixV2RunnerUtility(before, after, CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType);
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

	internal class AbstractionsAnalyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Abstractions(compilation);
	}

	internal class ExecutionAnalyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Execution(compilation);
	}

	internal class RunnerUtilityAnalyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2RunnerUtility(compilation);
	}
}
