using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
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
			class [|IMessageSink_Triggers|] : IMessageSink { }
			class [|IMessageSinkMessage_Triggers|] : IMessageSinkMessage { }

			// Reflection
			class [|IAssemblyInfo_Triggers|] : IAssemblyInfo { }
			class [|IAttributeInfo_Triggers|] : IAttributeInfo { }
			class [|IMethodInfo_Triggers|] : IMethodInfo { }
			class [|IParameterInfo_Triggers|] : IParameterInfo { }
			class [|ITypeInfo_Triggers|] : ITypeInfo { }

			// Object model
			class [|ITest_Triggers|] : ITest { }
			class [|ITestAssembly_Triggers|] : ITestAssembly { }
			class [|ITestCase_Triggers|] : ITestCase { }
			class [|ITestClass_Triggers|] : ITestClass { }
			class [|ITestCollection_Triggers|] : ITestCollection { }
			class [|ITestMethod_Triggers|] : ITestMethod { }

			// Test framework
			class [|ISourceInformation_Triggers|] : ISourceInformation { }
			class [|ISourceInformationProvider_Triggers|] : ISourceInformationProvider { }
			class [|ITestFramework_Triggers|] : ITestFramework { }
			class [|ITestFrameworkDiscoverer_Triggers|] : ITestFrameworkDiscoverer { }
			class [|ITestFrameworkExecutor_Triggers|] : ITestFrameworkExecutor { }
			""";

		await Verify_WithAbstractions.VerifyAnalyzerV2(CompilerDiagnostics.None, source);
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
			class [|IMessageSink_Triggers|] : IMessageSink { }
			class [|IMessageSinkMessage_Triggers|] : IMessageSinkMessage { }

			// Reflection
			class [|IAssemblyInfo_Triggers|] : IAssemblyInfo { }
			class [|IAttributeInfo_Triggers|] : IAttributeInfo { }
			class [|IMethodInfo_Triggers|] : IMethodInfo { }
			class [|IParameterInfo_Triggers|] : IParameterInfo { }
			class [|ITypeInfo_Triggers|] : ITypeInfo { }

			// Object model
			class [|ITest_Triggers|] : ITest { }
			class [|ITestAssembly_Triggers|] : ITestAssembly { }
			class [|ITestCase_Triggers|] : ITestCase { }
			class [|ITestClass_Triggers|] : ITestClass { }
			class [|ITestCollection_Triggers|] : ITestCollection { }
			class [|ITestMethod_Triggers|] : ITestMethod { }
			class [|IXunitTestCase_Triggers|] : IXunitTestCase { }

			// Test framework
			class [|ISourceInformation_Triggers|] : ISourceInformation { }
			class [|ISourceInformationProvider_Triggers|] : ISourceInformationProvider { }
			class [|ITestFramework_Triggers|] : ITestFramework { }
			class [|ITestFrameworkDiscoverer_Triggers|] : ITestFrameworkDiscoverer { }
			class [|ITestFrameworkExecutor_Triggers|] : ITestFrameworkExecutor { }

			// ----- Incompatible base class -----

			// Discovery and execution messages
			class [|IMessageSink_Foo_Triggers|] : Foo, IMessageSink { }
			class [|IMessageSinkMessage_Foo_Triggers|] : Foo, IMessageSinkMessage { }

			// Reflection
			class [|IAssemblyInfo_Foo_Triggers|] : Foo, IAssemblyInfo { }
			class [|IAttributeInfo_Foo_Triggers|] : Foo, IAttributeInfo { }
			class [|IMethodInfo_Foo_Triggers|] : Foo, IMethodInfo { }
			class [|IParameterInfo_Foo_Triggers|] : Foo, IParameterInfo { }
			class [|ITypeInfo_Foo_Triggers|] : Foo, ITypeInfo { }

			// Object model
			class [|ITest_Foo_Triggers|] : Foo, ITest { }
			class [|ITestAssembly_Foo_Triggers|] : Foo, ITestAssembly { }
			class [|ITestCase_Foo_Triggers|] : Foo, ITestCase { }
			class [|ITestClass_Foo_Triggers|] : Foo, ITestClass { }
			class [|ITestCollection_Foo_Triggers|] : Foo, ITestCollection { }
			class [|ITestMethod_Foo_Triggers|] : Foo, ITestMethod { }
			class [|IXunitTestCase_Foo_Triggers|] : Foo, IXunitTestCase { }

			// Test framework
			class [|ISourceInformation_Foo_Triggers|] : Foo, ISourceInformation { }
			class [|ISourceInformationProvider_Foo_Triggers|] : Foo, ISourceInformationProvider { }
			class [|ITestFramework_Foo_Triggers|] : Foo, ITestFramework { }
			class [|ITestFrameworkDiscoverer_Foo_Triggers|] : Foo, ITestFrameworkDiscoverer { }
			class [|ITestFrameworkExecutor_Foo_Triggers|] : Foo, ITestFrameworkExecutor { }

			// ----- With LongLivedMarshalByRefObject -----

			// Discovery and execution messages
			class IMessageSink_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, IMessageSink { }
			class IMessageSinkMessage_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, IMessageSinkMessage { }

			// Reflection
			class IAssemblyInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, IAssemblyInfo { }
			class IAttributeInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, IAttributeInfo { }
			class IMethodInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, IMethodInfo { }
			class IParameterInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, IParameterInfo { }
			class ITypeInfo_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITypeInfo { }

			// Object model
			class ITest_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITest { }
			class ITestAssembly_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITestAssembly { }
			class ITestCase_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITestCase { }
			class ITestClass_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITestClass { }
			class ITestCollection_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITestCollection { }
			class ITestMethod_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITestMethod { }
			class IXunitTestCase_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, IXunitTestCase { }

			// Test framework
			class ISourceInformation_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ISourceInformation { }
			class ISourceInformationProvider_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ISourceInformationProvider { }
			class ITestFramework_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITestFramework { }
			class ITestFrameworkDiscoverer_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITestFrameworkDiscoverer { }
			class ITestFrameworkExecutor_LLMBRO_DoesNotTrigger : Xunit.LongLivedMarshalByRefObject, ITestFrameworkExecutor { }

			// ----- With MyLLMBRO -----

			// Discovery and execution messages
			class IMessageSink_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IMessageSink { }
			class IMessageSinkMessage_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IMessageSinkMessage { }

			// Reflection
			class IAssemblyInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IAssemblyInfo { }
			class IAttributeInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IAttributeInfo { }
			class IMethodInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IMethodInfo { }
			class IParameterInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IParameterInfo { }
			class ITypeInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITypeInfo { }

			// Object model
			class ITest_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITest { }
			class ITestAssembly_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestAssembly { }
			class ITestCase_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestCase { }
			class ITestClass_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestClass { }
			class ITestCollection_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestCollection { }
			class ITestMethod_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestMethod { }
			class IXunitTestCase_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IXunitTestCase { }

			// Test framework
			class ISourceInformation_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ISourceInformation { }
			class ISourceInformationProvider_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ISourceInformationProvider { }
			class ITestFramework_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestFramework { }
			class ITestFrameworkDiscoverer_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestFrameworkDiscoverer { }
			class ITestFrameworkExecutor_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestFrameworkExecutor { }

			// ----- Concrete base class that already derives from LLMBRO -----

			class ConcreteTestCase_DoesNotTrigger : XunitTestCase { }
			""";

		await Verify_WithExecution.VerifyAnalyzerV2(CompilerDiagnostics.None, source);
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
			class [|IMessageSink_Triggers|] : IMessageSink { }
			class [|IMessageSinkMessage_Triggers|] : IMessageSinkMessage { }

			// Reflection
			class [|IAssemblyInfo_Triggers|] : IAssemblyInfo { }
			class [|IAttributeInfo_Triggers|] : IAttributeInfo { }
			class [|IMethodInfo_Triggers|] : IMethodInfo { }
			class [|IParameterInfo_Triggers|] : IParameterInfo { }
			class [|ITypeInfo_Triggers|] : ITypeInfo { }

			// Object model
			class [|ITest_Triggers|] : ITest { }
			class [|ITestAssembly_Triggers|] : ITestAssembly { }
			class [|ITestCase_Triggers|] : ITestCase { }
			class [|ITestClass_Triggers|] : ITestClass { }
			class [|ITestCollection_Triggers|] : ITestCollection { }
			class [|ITestMethod_Triggers|] : ITestMethod { }

			// Test framework
			class [|ISourceInformation_Triggers|] : ISourceInformation { }
			class [|ISourceInformationProvider_Triggers|] : ISourceInformationProvider { }
			class [|ITestFramework_Triggers|] : ITestFramework { }
			class [|ITestFrameworkDiscoverer_Triggers|] : ITestFrameworkDiscoverer { }
			class [|ITestFrameworkExecutor_Triggers|] : ITestFrameworkExecutor { }

			// ----- Incompatible base class -----

			// Discovery and execution messages
			class [|IMessageSink_Foo_Triggers|] : Foo, IMessageSink { }
			class [|IMessageSinkMessage_Foo_Triggers|] : Foo, IMessageSinkMessage { }

			// Reflection
			class [|IAssemblyInfo_Foo_Triggers|] : Foo, IAssemblyInfo { }
			class [|IAttributeInfo_Foo_Triggers|] : Foo, IAttributeInfo { }
			class [|IMethodInfo_Foo_Triggers|] : Foo, IMethodInfo { }
			class [|IParameterInfo_Foo_Triggers|] : Foo, IParameterInfo { }
			class [|ITypeInfo_Foo_Triggers|] : Foo, ITypeInfo { }

			// Object model
			class [|ITest_Foo_Triggers|] : Foo, ITest { }
			class [|ITestAssembly_Foo_Triggers|] : Foo, ITestAssembly { }
			class [|ITestCase_Foo_Triggers|] : Foo, ITestCase { }
			class [|ITestClass_Foo_Triggers|] : Foo, ITestClass { }
			class [|ITestCollection_Foo_Triggers|] : Foo, ITestCollection { }
			class [|ITestMethod_Foo_Triggers|] : Foo, ITestMethod { }

			// Test framework
			class [|ISourceInformation_Foo_Triggers|] : Foo, ISourceInformation { }
			class [|ISourceInformationProvider_Foo_Triggers|] : Foo, ISourceInformationProvider { }
			class [|ITestFramework_Foo_Triggers|] : Foo, ITestFramework { }
			class [|ITestFrameworkDiscoverer_Foo_Triggers|] : Foo, ITestFrameworkDiscoverer { }
			class [|ITestFrameworkExecutor_Foo_Triggers|] : Foo, ITestFrameworkExecutor { }

			// ----- With LongLivedMarshalByRefObject -----

			// Discovery and execution messages
			class IMessageSink_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, IMessageSink { }
			class IMessageSinkMessage_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, IMessageSinkMessage { }

			// Reflection
			class IAssemblyInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, IAssemblyInfo { }
			class IAttributeInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, IAttributeInfo { }
			class IMethodInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, IMethodInfo { }
			class IParameterInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, IParameterInfo { }
			class ITypeInfo_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITypeInfo { }

			// Object model
			class ITest_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITest { }
			class ITestAssembly_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITestAssembly { }
			class ITestCase_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITestCase { }
			class ITestClass_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITestClass { }
			class ITestCollection_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITestCollection { }
			class ITestMethod_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITestMethod { }

			// Test framework
			class ISourceInformation_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ISourceInformation { }
			class ISourceInformationProvider_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ISourceInformationProvider { }
			class ITestFramework_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITestFramework { }
			class ITestFrameworkDiscoverer_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITestFrameworkDiscoverer { }
			class ITestFrameworkExecutor_LLMBRO_DoesNotTrigger : LongLivedMarshalByRefObject, ITestFrameworkExecutor { }

			// ----- With MyLLMBRO -----

			// Discovery and execution messages
			class IMessageSink_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IMessageSink { }
			class IMessageSinkMessage_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IMessageSinkMessage { }

			// Reflection
			class IAssemblyInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IAssemblyInfo { }
			class IAttributeInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IAttributeInfo { }
			class IMethodInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IMethodInfo { }
			class IParameterInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, IParameterInfo { }
			class ITypeInfo_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITypeInfo { }

			// Object model
			class ITest_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITest { }
			class ITestAssembly_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestAssembly { }
			class ITestCase_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestCase { }
			class ITestClass_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestClass { }
			class ITestCollection_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestCollection { }
			class ITestMethod_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestMethod { }

			// Test framework
			class ISourceInformation_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ISourceInformation { }
			class ISourceInformationProvider_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ISourceInformationProvider { }
			class ITestFramework_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestFramework { }
			class ITestFrameworkDiscoverer_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestFrameworkDiscoverer { }
			class ITestFrameworkExecutor_MyLLMBRO_DoesNotTrigger : MyLLMBRO, ITestFrameworkExecutor { }
			""";

		await Verify_WithRunnerUtility.VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.None, source);
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
