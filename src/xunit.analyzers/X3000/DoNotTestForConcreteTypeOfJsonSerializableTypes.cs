using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotTestForConcreteTypeOfJsonSerializableTypes : XunitV3DiagnosticAnalyzer
{
	readonly static HashSet<string> matchingAssertions = [
		Constants.Asserts.IsAssignableFrom,
		Constants.Asserts.IsNotAssignableFrom,
		Constants.Asserts.IsNotType,
		Constants.Asserts.IsType,
	];

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

			reportIfMessageType(context, semanticModel, isTypeOperation.TypeOperand, isTypeOperation.Syntax);
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

			if (negatedPatternOperation.Children.FirstOrDefault() is not ITypePatternOperation typePatternOperation)
				return;

			reportIfMessageType(context, semanticModel, typePatternOperation.MatchedType, isPatternOperation.Syntax);
		}, OperationKind.IsPattern);

		// "value as Type"
		// "(Type)value"
		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IConversionOperation conversionOperation)
				return;

			var semanticModel = conversionOperation.SemanticModel;
			if (semanticModel is null)
				return;

			reportIfMessageType(context, semanticModel, conversionOperation.Operand.Type, conversionOperation.Syntax);
		}, OperationKind.Conversion);

		// "collection.OfType<Type>()"
		// "Assert.IsType<Type>(value)"
		// "Assert.IsNotType<Type>(value)"
		// "Assert.IsAssignableFrom<Type>(value)"
		// "Assert.IsNotAssignableFrom<Type>(value)"
		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IInvocationOperation invocationOperation)
				return;

			var semanticModel = invocationOperation.SemanticModel;
			if (semanticModel is null)
				return;

			// We will match "OfType<>()" by convention; that is, no args, single type. This may be overly
			// broad, but it seems like the best way to ensure every extension method gets convered.
			var method = invocationOperation.TargetMethod;
			if (method.Name == "OfType"
					&& method.IsGenericMethod
					&& method.TypeArguments.Length == 1
					&& method.Parameters.Length == (method.IsExtensionMethod ? 1 : 0))
				reportIfMessageType(context, semanticModel, method.TypeArguments[0], invocationOperation.Syntax);

			// We also look for type-related calls to Assert
			if (assertType is not null
					&& matchingAssertions.Contains(method.Name)
					&& SymbolEqualityComparer.Default.Equals(assertType, method.ContainingType))
			{
				var testType = default(ITypeSymbol);
				if (method.IsGenericMethod && method.TypeArguments.Length == 1)
					testType = method.TypeArguments[0];
				else if (invocationOperation.Arguments.FirstOrDefault() is IArgumentOperation typeArgumentOperation
						&& typeArgumentOperation.Value is ITypeOfOperation typeOfArgumentOperation)
					testType = typeOfArgumentOperation.TypeOperand;

				if (testType is not null)
					reportIfMessageType(context, semanticModel, testType, invocationOperation.Syntax);
			}
		}, OperationKind.Invocation);

		void reportIfMessageType(
			OperationAnalysisContext context,
			SemanticModel semanticModel,
			ITypeSymbol? typeSymbol,
			SyntaxNode syntax)
		{
			if (typeSymbol is null)
				return;

			foreach (var attribute in typeSymbol.GetAttributes())
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, jsonTypeIDAttributeType))
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X3002_DoNotTestForConcreteTypeOfJsonSerializableTypes,
							syntax.GetLocation(),
							typeSymbol.ToMinimalDisplayString(semanticModel, syntax.SpanStart)
						)
					);
		}
	}
}
