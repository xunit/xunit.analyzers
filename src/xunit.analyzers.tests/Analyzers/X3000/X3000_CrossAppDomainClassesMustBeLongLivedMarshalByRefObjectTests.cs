using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify_WithAbstractions = CSharpVerifier<X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.AbstractionsAnalyzer>;
using Verify_WithExecution = CSharpVerifier<X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.ExecutionAnalyzer>;
using Verify_WithRunnerUtility = CSharpVerifier<X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.RunnerUtilityAnalyzer>;

public class X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests
{
	[Fact]
	public async ValueTask V2_only_WithAbstractions()
	{
		var source = /* lang=c#-test */ $$"""
			using Xunit;
			using Xunit.Abstractions;

			// ----- No base class -----

			class NoInterfaces_DoesNotTrigger { }

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

		await Verify_WithAbstractions.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async ValueTask V2_only_WithExecution()
	{
		var source = /* lang=c#-test */ $$"""
			using Xunit;
			using Xunit.Abstractions;
			using Xunit.Sdk;

			class Foo { }
			class MyLLMBRO : Xunit.LongLivedMarshalByRefObject { }

			// ----- No base class -----

			class NoInterfaces_DoesNotTrigger { }

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

			// ----- Incompatible base class -----

			// Discovery and execution messages
			class [|IMessageSink_Foo_Triggers|] : Foo, {{MemberCount("IMessageSink", 1)}} { }
			class [|IMessageSinkMessage_Foo_Triggers|] : Foo, {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class [|IAssemblyInfo_Foo_Triggers|] : Foo, {{MemberCount("IAssemblyInfo", 5)}} { }
			class [|IAttributeInfo_Foo_Triggers|] : Foo, {{MemberCount("IAttributeInfo", 3)}} { }
			class [|IMethodInfo_Foo_Triggers|] : Foo, {{MemberCount("IMethodInfo", 11)}} { }
			class [|IParameterInfo_Foo_Triggers|] : Foo, {{MemberCount("IParameterInfo", 2)}} { }
			class [|ITypeInfo_Foo_Triggers|] : Foo, {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class [|ITest_Foo_Triggers|] : Foo, {{MemberCount("ITest", 2)}} { }
			class [|ITestAssembly_Foo_Triggers|] : Foo, {{MemberCount("ITestAssembly", 4)}} { }
			class [|ITestCase_Foo_Triggers|] : Foo, {{MemberCount("ITestCase", 9)}} { }
			class [|ITestClass_Foo_Triggers|] : Foo, {{MemberCount("ITestClass", 4)}} { }
			class [|ITestCollection_Foo_Triggers|] : Foo, {{MemberCount("ITestCollection", 6)}} { }
			class [|ITestMethod_Foo_Triggers|] : Foo, {{MemberCount("ITestMethod", 4)}} { }
			class [|IXunitTestCase_Foo_Triggers|] : Foo, {{MemberCount("IXunitTestCase", 13)}} { }

			// Test framework
			class [|ISourceInformation_Foo_Triggers|] : Foo, {{MemberCount("ISourceInformation", 4)}} { }
			class [|ISourceInformationProvider_Foo_Triggers|] : Foo, {{MemberCount("ISourceInformationProvider", 2)}} { }
			class [|ITestFramework_Foo_Triggers|] : Foo, {{MemberCount("ITestFramework", 4)}} { }
			class [|ITestFrameworkDiscoverer_Foo_Triggers|] : Foo, {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class [|ITestFrameworkExecutor_Foo_Triggers|] : Foo, {{MemberCount("ITestFrameworkExecutor", 4)}} { }

			// ----- With LongLivedMarshalByRefObject -----

			// Discovery and execution messages
			class IMessageSink_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IMessageSink", 1)}} { }
			class IMessageSinkMessage_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class IAssemblyInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IAssemblyInfo", 5)}} { }
			class IAttributeInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IAttributeInfo", 3)}} { }
			class IMethodInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IMethodInfo", 11)}} { }
			class IParameterInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IParameterInfo", 2)}} { }
			class ITypeInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class ITest_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITest", 2)}} { }
			class ITestAssembly_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestAssembly", 4)}} { }
			class ITestCase_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestCase", 9)}} { }
			class ITestClass_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestClass", 4)}} { }
			class ITestCollection_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestCollection", 6)}} { }
			class ITestMethod_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestMethod", 4)}} { }
			class IXunitTestCase_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("IXunitTestCase", 13)}} { }

			// Test framework
			class ISourceInformation_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ISourceInformation", 4)}} { }
			class ISourceInformationProvider_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ISourceInformationProvider", 2)}} { }
			class ITestFramework_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestFramework", 4)}} { }
			class ITestFrameworkDiscoverer_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class ITestFrameworkExecutor_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, {{MemberCount("ITestFrameworkExecutor", 4)}} { }

			// ----- With MyLLMBRO -----

			// Discovery and execution messages
			class IMessageSink_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IMessageSink", 1)}} { }
			class IMessageSinkMessage_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class IAssemblyInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IAssemblyInfo", 5)}} { }
			class IAttributeInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IAttributeInfo", 3)}} { }
			class IMethodInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IMethodInfo", 11)}} { }
			class IParameterInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IParameterInfo", 2)}} { }
			class ITypeInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class ITest_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITest", 2)}} { }
			class ITestAssembly_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestAssembly", 4)}} { }
			class ITestCase_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestCase", 9)}} { }
			class ITestClass_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestClass", 4)}} { }
			class ITestCollection_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestCollection", 6)}} { }
			class ITestMethod_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestMethod", 4)}} { }
			class IXunitTestCase_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IXunitTestCase", 13)}} { }

			// Test framework
			class ISourceInformation_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ISourceInformation", 4)}} { }
			class ISourceInformationProvider_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ISourceInformationProvider", 2)}} { }
			class ITestFramework_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestFramework", 4)}} { }
			class ITestFrameworkDiscoverer_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class ITestFrameworkExecutor_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestFrameworkExecutor", 4)}} { }

			// ----- Concrete base class that already derives from LLMBRO -----

			class ConcreteTestCase_DoesNotTrigger : XunitTestCase { }
			""";

		await Verify_WithExecution.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async ValueTask V2_only_WithRunnerUtility()
	{
		var source = /* lang=c#-test */ $$"""
			using Xunit;
			using Xunit.Abstractions;
			using Xunit.Sdk;

			class Foo { }
			class MyLLMBRO : LongLivedMarshalByRefObject { }

			// ----- No base class -----

			class NoInterfaces_DoesNotTrigger { }

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

			// ----- Incompatible base class -----

			// Discovery and execution messages
			class [|IMessageSink_Foo_Triggers|] : Foo, {{MemberCount("IMessageSink", 1)}} { }
			class [|IMessageSinkMessage_Foo_Triggers|] : Foo, {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class [|IAssemblyInfo_Foo_Triggers|] : Foo, {{MemberCount("IAssemblyInfo", 5)}} { }
			class [|IAttributeInfo_Foo_Triggers|] : Foo, {{MemberCount("IAttributeInfo", 3)}} { }
			class [|IMethodInfo_Foo_Triggers|] : Foo, {{MemberCount("IMethodInfo", 11)}} { }
			class [|IParameterInfo_Foo_Triggers|] : Foo, {{MemberCount("IParameterInfo", 2)}} { }
			class [|ITypeInfo_Foo_Triggers|] : Foo, {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class [|ITest_Foo_Triggers|] : Foo, {{MemberCount("ITest", 2)}} { }
			class [|ITestAssembly_Foo_Triggers|] : Foo, {{MemberCount("ITestAssembly", 4)}} { }
			class [|ITestCase_Foo_Triggers|] : Foo, {{MemberCount("ITestCase", 9)}} { }
			class [|ITestClass_Foo_Triggers|] : Foo, {{MemberCount("ITestClass", 4)}} { }
			class [|ITestCollection_Foo_Triggers|] : Foo, {{MemberCount("ITestCollection", 6)}} { }
			class [|ITestMethod_Foo_Triggers|] : Foo, {{MemberCount("ITestMethod", 4)}} { }

			// Test framework
			class [|ISourceInformation_Foo_Triggers|] : Foo, {{MemberCount("ISourceInformation", 4)}} { }
			class [|ISourceInformationProvider_Foo_Triggers|] : Foo, {{MemberCount("ISourceInformationProvider", 2)}} { }
			class [|ITestFramework_Foo_Triggers|] : Foo, {{MemberCount("ITestFramework", 4)}} { }
			class [|ITestFrameworkDiscoverer_Foo_Triggers|] : Foo, {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class [|ITestFrameworkExecutor_Foo_Triggers|] : Foo, {{MemberCount("ITestFrameworkExecutor", 4)}} { }

			// ----- With LongLivedMarshalByRefObject -----

			// Discovery and execution messages
			class IMessageSink_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("IMessageSink", 1)}} { }
			class IMessageSinkMessage_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class IAssemblyInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("IAssemblyInfo", 5)}} { }
			class IAttributeInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("IAttributeInfo", 3)}} { }
			class IMethodInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("IMethodInfo", 11)}} { }
			class IParameterInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("IParameterInfo", 2)}} { }
			class ITypeInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class ITest_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITest", 2)}} { }
			class ITestAssembly_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITestAssembly", 4)}} { }
			class ITestCase_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITestCase", 9)}} { }
			class ITestClass_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITestClass", 4)}} { }
			class ITestCollection_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITestCollection", 6)}} { }
			class ITestMethod_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITestMethod", 4)}} { }

			// Test framework
			class ISourceInformation_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ISourceInformation", 4)}} { }
			class ISourceInformationProvider_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ISourceInformationProvider", 2)}} { }
			class ITestFramework_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITestFramework", 4)}} { }
			class ITestFrameworkDiscoverer_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class ITestFrameworkExecutor_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, {{MemberCount("ITestFrameworkExecutor", 4)}} { }

			// ----- With MyLLMBRO -----

			// Discovery and execution messages
			class IMessageSink_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IMessageSink", 1)}} { }
			class IMessageSinkMessage_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IMessageSinkMessage", 0)}} { }

			// Reflection
			class IAssemblyInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IAssemblyInfo", 5)}} { }
			class IAttributeInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IAttributeInfo", 3)}} { }
			class IMethodInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IMethodInfo", 11)}} { }
			class IParameterInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("IParameterInfo", 2)}} { }
			class ITypeInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITypeInfo", 13)}} { }

			// Object model
			class ITest_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITest", 2)}} { }
			class ITestAssembly_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestAssembly", 4)}} { }
			class ITestCase_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestCase", 9)}} { }
			class ITestClass_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestClass", 4)}} { }
			class ITestCollection_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestCollection", 6)}} { }
			class ITestMethod_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestMethod", 4)}} { }

			// Test framework
			class ISourceInformation_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ISourceInformation", 4)}} { }
			class ISourceInformationProvider_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ISourceInformationProvider", 2)}} { }
			class ITestFramework_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestFramework", 4)}} { }
			class ITestFrameworkDiscoverer_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestFrameworkDiscoverer", 6)}} { }
			class ITestFrameworkExecutor_MyLLMBRO_DoesNotTrigger : MyLLMBRO, {{MemberCount("ITestFrameworkExecutor", 4)}} { }
			""";

		await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(source);
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
