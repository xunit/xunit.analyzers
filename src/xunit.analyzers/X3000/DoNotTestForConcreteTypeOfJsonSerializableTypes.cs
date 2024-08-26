using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotTestForConcreteTypeOfJsonSerializableTypes : XunitV3DiagnosticAnalyzer
{
	public DoNotTestForConcreteTypeOfJsonSerializableTypes() :
		base(Descriptors.X3002_DoNotTestForConcreteTypeOfJsonSerializableTypes)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var assertType = xunitContext.Assert.AssertType;

		var jsonTypeIDAttributeType = xunitContext.V3Core?.JsonTypeIDAttributeType;
		if (jsonTypeIDAttributeType is null)
			return;

		// "value is Type"
		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IIsTypeOperation isTypeOperation)
				return;

			var semanticModel = isTypeOperation.SemanticModel;
			if (semanticModel is null)
				return;

			reportIfMessageType(context.ReportDiagnostic, semanticModel, isTypeOperation.TypeOperand, isTypeOperation.Syntax);
		}, OperationKind.IsType);

		// "value is not Type"
		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IIsPatternOperation isPatternOperation)
				return;

			var semanticModel = isPatternOperation.SemanticModel;
			if (semanticModel is null)
				return;

			if (isPatternOperation.Pattern is not INegatedPatternOperation negatedPatternOperation)
				return;

#if ROSLYN_LATEST
			if (negatedPatternOperation.ChildOperations.FirstOrDefault() is not ITypePatternOperation typePatternOperation)
#else
			if (negatedPatternOperation.Children.FirstOrDefault() is not ITypePatternOperation typePatternOperation)
#endif
				return;

			reportIfMessageType(context.ReportDiagnostic, semanticModel, typePatternOperation.MatchedType, isPatternOperation.Syntax);
		}, OperationKind.IsPattern);

		// "value as Type"
		// "(Type)value"
		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IConversionOperation conversionOperation)
				return;

			// We don't want to prohibit conversion that comes from "new(...)"
			if (conversionOperation.Syntax is ImplicitObjectCreationExpressionSyntax)
				return;

			var semanticModel = conversionOperation.SemanticModel;
			if (semanticModel is null)
				return;

			reportIfMessageType(context.ReportDiagnostic, semanticModel, conversionOperation.Type, conversionOperation.Syntax);
		}, OperationKind.Conversion);

		// "typeof(Type)"
		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not ITypeOfOperation typeOfOperation)
				return;

			var semanticModel = typeOfOperation.SemanticModel;
			if (semanticModel is null)
				return;

			reportIfMessageType(context.ReportDiagnostic, semanticModel, typeOfOperation.TypeOperand, typeOfOperation.Syntax);
		}, OperationKind.TypeOf);

		// "SomeMethod<Type>()"
		// "GenericType<Type>"
		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node.ChildNodes().FirstOrDefault() is not TypeArgumentListSyntax typeArgumentListSyntax)
				return;

			foreach (var identifierNameSyntax in typeArgumentListSyntax.ChildNodes().OfType<IdentifierNameSyntax>())
				reportIfMessageType(context.ReportDiagnostic, context.SemanticModel, context.SemanticModel.GetTypeInfo(identifierNameSyntax).ConvertedType, context.Node);
		}, SyntaxKind.GenericName);

		void reportIfMessageType(
			Action<Diagnostic> reportDiagnostic,
			SemanticModel semanticModel,
			ITypeSymbol? typeSymbol,
			SyntaxNode syntax)
		{
			if (typeSymbol is null)
				return;

			foreach (var attribute in typeSymbol.GetAttributes())
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, jsonTypeIDAttributeType))
					reportDiagnostic(
						Diagnostic.Create(
							Descriptors.X3002_DoNotTestForConcreteTypeOfJsonSerializableTypes,
							syntax.GetLocation(),
							typeSymbol.ToMinimalDisplayString(semanticModel, syntax.SpanStart)
						)
					);
		}
	}
}
