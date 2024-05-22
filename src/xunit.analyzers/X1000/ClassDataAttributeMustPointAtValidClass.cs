using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClassDataAttributeMustPointAtValidClass : XunitDiagnosticAnalyzer
{
	const string typesV2 = "IEnumerable<object[]>";
	const string typesV3 = "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>";

	public ClassDataAttributeMustPointAtValidClass() :
		base(
			Descriptors.X1007_ClassDataAttributeMustPointAtValidClass,
			Descriptors.X1050_ClassDataTheoryDataRowIsRecommendedForStronglyTypedAnalysis
		)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var compilation = context.Compilation;
		var iEnumerableOfObjectArray = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);
		var iEnumerableOfTheoryDataRow = TypeSymbolFactory.IEnumerableOfITheoryDataRow(compilation);
		var iAsyncEnumerableOfObjectArray = TypeSymbolFactory.IAsyncEnumerableOfObjectArray(compilation);
		var iAsyncEnumerableOfTheoryDataRow = TypeSymbolFactory.IAsyncEnumerableOfITheoryDataRow(compilation);
		var theoryDataRows =
			TypeSymbolFactory
				.TheoryDataRow_ByGenericArgumentCount(compilation)
				.Where(kvp => kvp.Key != 0)
				.Select(kvp => kvp.Value.ConstructUnboundGenericType())
				.ToArray();

		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node is not AttributeSyntax attribute)
				return;
			if (attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression is not TypeOfExpressionSyntax argumentExpression)
				return;

			var semanticModel = context.SemanticModel;
			if (!SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(attribute).Type, xunitContext.Core.ClassDataAttributeType))
				return;

			if (semanticModel.GetTypeInfo(argumentExpression.Type).Type is not INamedTypeSymbol classType)
				return;
			if (classType.Kind == SymbolKind.ErrorType)
				return;

			var missingInterface = !iEnumerableOfObjectArray.IsAssignableFrom(classType);
			if (xunitContext.HasV3References)
			{
				if (missingInterface && iEnumerableOfTheoryDataRow is not null)
					missingInterface = !iEnumerableOfTheoryDataRow.IsAssignableFrom(classType);
				if (missingInterface && iAsyncEnumerableOfObjectArray is not null)
					missingInterface = !iAsyncEnumerableOfObjectArray.IsAssignableFrom(classType);
				if (missingInterface && iAsyncEnumerableOfTheoryDataRow is not null)
					missingInterface = !iAsyncEnumerableOfTheoryDataRow.IsAssignableFrom(classType);
			}

			var isAbstract = classType.IsAbstract;
			var noValidConstructor = !classType.InstanceConstructors.Any(c => c.Parameters.IsEmpty && c.DeclaredAccessibility == Accessibility.Public);

			if (missingInterface || isAbstract || noValidConstructor)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1007_ClassDataAttributeMustPointAtValidClass,
						argumentExpression.Type.GetLocation(),
						classType.Name,
						xunitContext.HasV3References ? typesV3 : typesV2
					)
				);
				return;
			}

			// For v3, recommend I(Async)Enumerable<TheoryDataRow<>> over I(Async)Enumerable<object[]>.
			// Can't make any recommendations for v2 projects, since deriving from TheoryData<> doesn't make sense.
			if (!xunitContext.HasV3References)
				return;

			var rowType = classType.UnwrapEnumerable(compilation);
			if (rowType is null || theoryDataRows.Any(tdr => tdr.IsAssignableFrom(rowType.OriginalDefinition)))
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1050_ClassDataTheoryDataRowIsRecommendedForStronglyTypedAnalysis,
					argumentExpression.Type.GetLocation()
				)
			);
		}, SyntaxKind.Attribute);
	}
}
