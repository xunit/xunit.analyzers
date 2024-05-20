using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TheoryDataRowArgumentsShouldBeSerializable : XunitDiagnosticAnalyzer
{
	public TheoryDataRowArgumentsShouldBeSerializable() :
		base(
			Descriptors.X1046_AvoidUsingTheoryDataRowArgumentsThatAreNotSerializable,
			Descriptors.X1047_AvoidUsingTheoryDataRowArgumentsThatMightNotBeSerializable
		)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var theoryDataRowTypes = TypeSymbolFactory.TheoryDataRow_ByGenericArgumentCount(context.Compilation);
		if (theoryDataRowTypes.Count == 0)
			return;

		if (SerializableTypeSymbols.Create(context.Compilation, xunitContext) is not SerializableTypeSymbols typeSymbols)
			return;

		var analyzer = new SerializabilityAnalyzer(typeSymbols);

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IObjectCreationOperation objectCreation)
				return;

			var creationType = objectCreation.Type as INamedTypeSymbol;
			if (creationType is not null && creationType.IsGenericType)
				creationType = creationType.OriginalDefinition;

			if (!theoryDataRowTypes.Values.Contains(creationType, SymbolEqualityComparer.Default))
				return;

			var argumentOperations = GetConstructorArguments(objectCreation);
			if (argumentOperations is null)
				return;

			foreach (var argumentOperation in argumentOperations)
			{
				if (analyzer.TypeShouldBeIgnored(argumentOperation.Type))
					continue;

				var serializability = analyzer.AnalayzeSerializability(argumentOperation.Type);

				if (serializability != Serializability.AlwaysSerializable)
				{
					var typeDisplayName =
						argumentOperation.SemanticModel is null
							? argumentOperation.Type.Name
							: argumentOperation.Type.ToMinimalDisplayString(argumentOperation.SemanticModel, argumentOperation.Syntax.SpanStart);

					context.ReportDiagnostic(
						Diagnostic.Create(
							serializability == Serializability.NeverSerializable
								? Descriptors.X1046_AvoidUsingTheoryDataRowArgumentsThatAreNotSerializable
								: Descriptors.X1047_AvoidUsingTheoryDataRowArgumentsThatMightNotBeSerializable,
							argumentOperation.Syntax.GetLocation(),
							argumentOperation.Syntax.ToFullString(),
							typeDisplayName
						)
					);
				}
			}

		}, OperationKind.ObjectCreation);
	}

	static IReadOnlyList<IOperation>? GetConstructorArguments(IObjectCreationOperation objectCreation)
	{
		// If this is the generic TheoryDataRow, then just return the arguments as-is
		if (objectCreation.Type is INamedTypeSymbol creationType && creationType.IsGenericType)
		{
			var result = new List<IOperation>();

			for (var idx = 0; idx < objectCreation.Arguments.Length; ++idx)
			{
#if ROSLYN_3_11
				var elementValue = objectCreation.Arguments[idx].Children.FirstOrDefault();
#else
				var elementValue = objectCreation.Arguments[idx].ChildOperations.FirstOrDefault();
#endif
				while (elementValue is IConversionOperation conversion)
					elementValue = conversion.Operand;

				if (elementValue is not null)
					result.Add(elementValue);
			}

			return result;
		}

		// Non-generic TheoryDataRow, which means we should have a single argument
		// which is the params array of values, which we need to unpack.
		if (objectCreation.Arguments.FirstOrDefault() is not IArgumentOperation argumentOperation)
			return null;

#if ROSLYN_3_11
		var firstArgument = argumentOperation.Children.FirstOrDefault();
#else
		var firstArgument = argumentOperation.ChildOperations.FirstOrDefault();
#endif
		if (firstArgument is null)
			return null;

		// Common pattern: implicit array creation for the params array
		if (firstArgument is IArrayCreationOperation arrayCreation &&
#if ROSLYN_3_11
			arrayCreation.Children.Skip(1).FirstOrDefault() is IArrayInitializerOperation arrayInitializer)
#else
			arrayCreation.ChildOperations.Skip(1).FirstOrDefault() is IArrayInitializerOperation arrayInitializer)
#endif
		{
			var result = new List<IOperation>();

			for (var idx = 0; idx < arrayInitializer.ElementValues.Length; ++idx)
			{
				var elementValue = arrayInitializer.ElementValues[idx];
				while (elementValue is IConversionOperation conversion)
					elementValue = conversion.Operand;

				result.Add(elementValue);
			}

			return result;
		}

		// TODO: Less common pattern: user created the array ahead of time, which shows up as ILocalReferenceOperation

		return null;
	}

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		Guard.ArgumentNotNull(xunitContext).HasV3References;
}
