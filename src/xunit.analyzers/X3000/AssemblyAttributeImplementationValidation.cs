using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssemblyAttributeImplementationValidation() :
	XunitV3DiagnosticAnalyzer(
		Descriptors.X3004_TypeDoesNotImplementInterface,
		Descriptors.X3005_TypeMustHaveCorrectPublicConstructor)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var obsoleteAttributeType = TypeSymbolFactory.ObsoleteAttribute(context.Compilation);
		var stringType = TypeSymbolFactory.String(context.Compilation);
		var assemblyFixtureArgTypes = new[] {
			TypeSymbolFactory.IMessageSink_V3(context.Compilation),
			TypeSymbolFactory.ITestContextAccessor_V3(context.Compilation),
			TypeSymbolFactory.ITestOutputHelper_V3(context.Compilation),
		}.WhereNotNull().ToImmutableHashSet(SymbolEqualityComparer.Default);

		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node is not AttributeSyntax attributeSyntax)
				return;

			if (context.SemanticModel.GetTypeInfo(attributeSyntax, context.CancellationToken).Type is not INamedTypeSymbol attributeType)
				return;

			var _ =  // Throw away the result, we just want to short circuit processing when we've identified our attribute
				validateEmptyCtor(
					xunitContext.V3Common?.RegisterXunitSerializerAttributeType,
					null,
					[xunitContext.V3Common?.IXunitSerializerType]) ||
				validateEmptyCtor(
					xunitContext.V3Core?.TestCaseOrdererAttributeType,
					null,
					[xunitContext.V3Core?.ITestCaseOrdererType]) ||
				validateEmptyCtor(
					xunitContext.V3Core?.TestClassOrdererAttributeType,
					null,
					[xunitContext.V3Core?.ITestClassOrdererType]) ||
				validateEmptyCtor(
					xunitContext.V3Core?.TestCollectionOrdererAttributeType,
					null,
					[xunitContext.V3Core?.ITestCollectionOrdererType]) ||
				validateEmptyCtor(
					xunitContext.V3Core?.TestMethodOrdererAttributeType,
					null,
					[xunitContext.V3Core?.ITestMethodOrdererType]) ||
				validateEmptyCtor(
					xunitContext.V3Core?.TestPipelineStartupAttributeType,
					null,
					[xunitContext.V3Core?.ITestPipelineStartupType]) ||
				validateEmptyCtor(
					xunitContext.V3RunnerCommon?.RegisterConsoleResultWriterAttributeType,
					xunitContext.V3RunnerCommon?.RegisterConsoleResultWriterAttributeOfTType,
					[xunitContext.V3RunnerCommon?.IConsoleResultWriterType], 1) ||
				validateEmptyCtor(
					xunitContext.V3RunnerCommon?.RegisterMicrosoftTestingPlatformResultWriterAttributeType,
					xunitContext.V3RunnerCommon?.RegisterMicrosoftTestingPlatformResultWriterAttributeOfTType,
					[xunitContext.V3RunnerCommon?.IMicrosoftTestingPlatformResultWriterType], 1) ||
				validateEmptyCtor(
					xunitContext.V3RunnerCommon?.RegisterResultWriterAttributeType,
					xunitContext.V3RunnerCommon?.RegisterResultWriterAttributeOfTType,
					[xunitContext.V3RunnerCommon?.IConsoleResultWriterType, xunitContext.V3RunnerCommon?.IMicrosoftTestingPlatformResultWriterType], 1) ||
				validateEmptyCtor(
					xunitContext.V3RunnerCommon?.RegisterRunnerReporterAttributeType,
					xunitContext.V3RunnerCommon?.RegisterRunnerReporterAttributeOfTType,
					[xunitContext.V3RunnerCommon?.IRunnerReporterType]) ||
				validateAssemblyFixture() ||
				validateTestCollectionFactory() ||
				validateTestFramework();

			INamedTypeSymbol? getImplementationType(
				INamedTypeSymbol? registrationAttributeType,
				INamedTypeSymbol? genericRegistrationAttributeType = null,
				int typeArgumentIdx = 0,
				int genericTypeArgumentIdx = 0)
			{
				if (attributeType.IsGenericType)
				{
					if (genericRegistrationAttributeType is null ||
							!SymbolEqualityComparer.Default.Equals(attributeType.ConstructUnboundGenericType(), genericRegistrationAttributeType.ConstructUnboundGenericType()) ||
							attributeType.TypeArguments.Length <= genericTypeArgumentIdx)
						return null;

					return attributeType.TypeArguments[genericTypeArgumentIdx] as INamedTypeSymbol;
				}

				if (registrationAttributeType is null ||
						!SymbolEqualityComparer.Default.Equals(attributeType, registrationAttributeType) ||
						attributeSyntax.ArgumentList is null ||
						attributeSyntax.ArgumentList.Arguments.Count <= typeArgumentIdx ||
						attributeSyntax.ArgumentList.Arguments[typeArgumentIdx].Expression is not TypeOfExpressionSyntax typeOfExpressionSyntax)
					return null;

				return context.SemanticModel.GetTypeInfo(typeOfExpressionSyntax.Type, context.CancellationToken).Type as INamedTypeSymbol;
			}

			bool hasEmptyCtorOrInstanceProperty(INamedTypeSymbol implementationType)
			{
				var targetCtor =
					implementationType
						.Constructors
						.FirstOrDefault(
							c => !c.IsStatic &&
							c.DeclaredAccessibility == Accessibility.Public &&
							c.Parameters.All(p => p.IsOptional || p.IsParams)
						);

				if (targetCtor is null)
					return false;
				if (obsoleteAttributeType is null || !targetCtor.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, obsoleteAttributeType)))
					return true;

				return
					implementationType.GetMembers("Instance").FirstOrDefault() is IPropertySymbol propertySymbol &&
					propertySymbol.IsStatic &&
					SymbolEqualityComparer.Default.Equals(propertySymbol.Type, implementationType);
			}

			void reportX3004(
				INamedTypeSymbol implementationType,
				INamedTypeSymbol interfaceType) =>
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X3004_TypeDoesNotImplementInterface,
							attributeSyntax.GetLocation(),
							implementationType,
							interfaceType
						)
					);

			void reportX3005(
				INamedTypeSymbol implementationType,
				string parameters = "") =>
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X3005_TypeMustHaveCorrectPublicConstructor,
							attributeSyntax.GetLocation(),
							implementationType,
							parameters
						)
					);

			bool validateAssemblyFixture()
			{
				if (getImplementationType(xunitContext.V3Core?.AssemblyFixtureAttributeType, xunitContext.V3Core?.AssemblyFixtureAttributeOfTType) is not { } implementationType)
					return false;

				var targetCtors =
					implementationType
						.Constructors
						.Where(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public)
						.ToArray();

				if (targetCtors.Length != 1 ||
						(obsoleteAttributeType is not null && targetCtors[0].GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, obsoleteAttributeType))) ||
						!targetCtors[0].Parameters.All(p => p.IsParams || p.IsOptional || assemblyFixtureArgTypes.Contains(p.Type)))
					reportX3005(implementationType, "[ITestContextAccessor accessor], [ITestOutputHelper helper], [IMessageSink diagnosticMessageSink]");

				return true;
			}

			bool validateEmptyCtor(
				INamedTypeSymbol? registrationAttributeType,
				INamedTypeSymbol? genericRegistrationAttributeType,
				INamedTypeSymbol?[] interfaceTypes,
				int typeArgumentIdx = 0)
			{
				if (getImplementationType(registrationAttributeType, genericRegistrationAttributeType, typeArgumentIdx) is not { } implementationType)
					return false;

				validateInterfaces(implementationType, interfaceTypes);

				if (!hasEmptyCtorOrInstanceProperty(implementationType))
					reportX3005(implementationType);

				return true;
			}

			void validateInterfaces(
				INamedTypeSymbol implementationType,
				INamedTypeSymbol?[] interfaceTypes)
			{
				if (interfaceTypes.Length == 0)
					return;

				var interfacesHash = new HashSet<INamedTypeSymbol>(implementationType.AllInterfaces, SymbolEqualityComparer.Default);

				foreach (var interfaceType in interfaceTypes.WhereNotNull())
					if (!interfacesHash.Contains(interfaceType))
						reportX3004(implementationType, interfaceType);
			}

			bool validateTestCollectionFactory()
			{
				if (xunitContext.V3Core?.IXunitTestAssemblyType is not { } testAssemblyType)
					return false;
				if (getImplementationType(xunitContext.V3Core?.CollectionBehaviorAttributeType) is not { } implementationType)
					return false;

				validateInterfaces(implementationType, [xunitContext.V3Core?.ITestCollectionFactoryType]);

				var assemblyCtor =
					implementationType
						.Constructors
						.FirstOrDefault(
							c => !c.IsStatic &&
							c.DeclaredAccessibility == Accessibility.Public &&
							c.Parameters.Length >= 1 &&
							SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, testAssemblyType) &&
							c.Parameters.Skip(1).All(p => p.IsOptional || p.IsParams) &&
							(obsoleteAttributeType is null || !c.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, obsoleteAttributeType)))
						);

				if (assemblyCtor is null)
					reportX3005(implementationType, xunitContext.HasV3AotReferences ? "ICodeGenTestAssembly testAssembly" : "IXunitTestAssembly testAssembly");

				return true;
			}

			bool validateTestFramework()
			{
				if (getImplementationType(xunitContext.V3Core?.TestFrameworkAttributeType) is not { } implementationType)
					return false;

				validateInterfaces(implementationType, [xunitContext.V3Core?.ITestFrameworkType]);

				var stringCtor =
					implementationType
						.Constructors
						.FirstOrDefault(
							c => !c.IsStatic &&
							c.DeclaredAccessibility == Accessibility.Public &&
							c.Parameters.Length >= 1 &&
							SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, stringType) &&
							c.Parameters.Skip(1).All(p => p.IsOptional || p.IsParams) &&
							(obsoleteAttributeType is null || !c.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, obsoleteAttributeType)))
						);

				if (stringCtor is not null)
					return true;

				if (!hasEmptyCtorOrInstanceProperty(implementationType))
					reportX3005(implementationType, "string? configFileName");

				return true;
			}
		}, SyntaxKind.Attribute);
	}
}
