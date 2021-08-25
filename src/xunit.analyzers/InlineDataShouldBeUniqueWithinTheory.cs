using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class InlineDataShouldBeUniqueWithinTheory : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1025_InlineDataShouldBeUniqueWithinTheory);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext) =>
				context.RegisterSymbolAction(context => AnalyzeMethod(context, xunitContext), SymbolKind.Method);

		static void AnalyzeMethod(
			SymbolAnalysisContext context,
			XunitContext xunitContext)
		{
			var method = (IMethodSymbol)context.Symbol;

			var methodAllAttributes = method.GetAttributes();
			if (!methodAllAttributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
				return;

			var objectArrayType = TypeSymbolFactory.ObjectArray(context.Compilation);

			var wellFormedInlineDataAttributes =
				methodAllAttributes
					.Where(a => Equals(a.AttributeClass, xunitContext.Core.InlineDataAttributeType) && HasAttributeDeclarationNoCompilationErrors(a, objectArrayType));

			AnalyzeInlineDataAttributesWithinTheory(context, wellFormedInlineDataAttributes);
		}

		static void AnalyzeInlineDataAttributesWithinTheory(
			SymbolAnalysisContext context,
			IEnumerable<AttributeData> inlineDataAttributes)
		{
			var attributeRelatedMethod = (IMethodSymbol)context.Symbol;
			var uniqueAttributes = new HashSet<AttributeData>(new InlineDataUniquenessComparer(attributeRelatedMethod));

			foreach (var currentInlineData in inlineDataAttributes)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (uniqueAttributes.Contains(currentInlineData))
					ReportDuplicate(context, currentInlineData);
				else
					uniqueAttributes.Add(currentInlineData);
			}
		}

		static AttributeSyntax GetAttributeSyntax(
			SymbolAnalysisContext context,
			AttributeData attribute) =>
				(AttributeSyntax)attribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken);

		static void ReportDuplicate(
			SymbolAnalysisContext context,
			AttributeData duplicateAttribute)
		{
			var method = (IMethodSymbol)context.Symbol;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1025_InlineDataShouldBeUniqueWithinTheory,
					GetAttributeSyntax(context, duplicateAttribute).GetLocation(),
					method.Name,
					method.ContainingType.Name
				)
			);
		}

		static bool HasAttributeDeclarationNoCompilationErrors(
			AttributeData attribute,
			IArrayTypeSymbol objectArrayType) =>
				attribute.ConstructorArguments.Length == 1 &&
				objectArrayType.Equals(attribute.ConstructorArguments.FirstOrDefault().Type);

		class InlineDataUniquenessComparer : IEqualityComparer<AttributeData>
		{
			ImmutableArray<IParameterSymbol> methodParametersWithExplicitDefaults;

			public InlineDataUniquenessComparer(IMethodSymbol attributeRelatedMethod)
			{
				methodParametersWithExplicitDefaults =
					attributeRelatedMethod
						.Parameters
						.Where(p => p.HasExplicitDefaultValue)
						.ToImmutableArray();
			}

			public bool Equals(
				AttributeData x,
				AttributeData y)
			{
				var xArguments = GetEffectiveTestArguments(x);
				var yArguments = GetEffectiveTestArguments(y);

				var areBothNullEntirely =
					IsSingleNullByInlineDataOrByDefaultParamValue(xArguments)
					&& IsSingleNullByInlineDataOrByDefaultParamValue(yArguments);

				return areBothNullEntirely || AreArgumentsEqual(xArguments, yArguments);
			}

			// Since arguments can be object[] at any level we need to compare 2 sequences of trees for equality.
			// The algorithm traverses each tree in a sequence and compares with the corresponding tree in the other sequence.
			// Any difference at any stage results in inequality proved and <c>false</c> returned.
			bool AreArgumentsEqual(
				ImmutableArray<object> xArguments,
				ImmutableArray<object> yArguments)
			{
				if (xArguments.Length != yArguments.Length)
					return false;

				for (var i = 0; i < xArguments.Length; i++)
				{
					var x = xArguments[i];
					var y = yArguments[i];

					switch (x)
					{
						case TypedConstant xArgPrimitive when xArgPrimitive.Kind != TypedConstantKind.Array:
							switch (y)
							{
								case TypedConstant yArgPrimitive when yArgPrimitive.Kind != TypedConstantKind.Array:
									if (!xArgPrimitive.Equals(yArgPrimitive))
										return false;
									break;

								case IParameterSymbol yMethodParamDefault:
									if (!object.Equals(xArgPrimitive.Value, yMethodParamDefault.ExplicitDefaultValue))
										return false;
									break;

								default:
									return false;
							}
							break;

						case IParameterSymbol xMethodParamDefault:
							switch (y)
							{
								case TypedConstant yArgPrimitive when yArgPrimitive.Kind != TypedConstantKind.Array:
									if (!object.Equals(xMethodParamDefault.ExplicitDefaultValue, yArgPrimitive.Value))
										return false;
									break;

								case IParameterSymbol yMethodParamDefault:
									if (!object.Equals(xMethodParamDefault.ExplicitDefaultValue, yMethodParamDefault.ExplicitDefaultValue))
										return false;
									break;

								default:
									return false;
							}
							break;

						case TypedConstant xArgArray when xArgArray.Kind == TypedConstantKind.Array && !xArgArray.IsNull:
							switch (y)
							{
								case TypedConstant yArgArray when yArgArray.Kind == TypedConstantKind.Array:
									if (!AreArgumentsEqual(xArgArray.Values.Cast<object>().ToImmutableArray(), yArgArray.Values.Cast<object>().ToImmutableArray()))
										return false;
									break;
								default:
									return false;
							}
							break;

						default:
							return false;
					}
				}

				return true;
			}

			// A special search for a degenerated case of either:
			// 1. InlineData(null) or
			// 2. InlineData() and a single param method with default returning null.
			static bool IsSingleNullByInlineDataOrByDefaultParamValue(ImmutableArray<object> args)
			{
				if (args.Length != 1)
					return false;

				return args[0] switch
				{
					TypedConstant xSingleNull when xSingleNull.Kind == TypedConstantKind.Array && xSingleNull.IsNull => true,
					IParameterSymbol xParamDefaultNull when xParamDefaultNull.ExplicitDefaultValue == null => true,
					_ => false,
				};
			}

			/// <summary>
			/// Flattens a collection of arguments (ie. new object[] {1, new object[] {2}} becomes a collection of {1, 2}
			/// and computes accumulative hash code. Exact comparison is carried out in <see cref="Equals"/> impl.
			/// </summary>
			public int GetHashCode(AttributeData attributeData)
			{
				var arguments = GetEffectiveTestArguments(attributeData);
				var flattened = GetFlattenedArgumentPrimitives(arguments);

				var hash = 17;

				foreach (var primitive in flattened)
					hash = hash * 31 + (primitive?.GetHashCode() ?? 0);

				return hash;
			}

			ImmutableArray<object> GetFlattenedArgumentPrimitives(IEnumerable<object> arguments)
			{
				var results = new List<object>();

				foreach (var argument in arguments)
				{
					switch (argument)
					{
						case TypedConstant argPrimitive when argPrimitive.Kind != TypedConstantKind.Array:
							results.Add(argPrimitive.Value);
							break;

						case IParameterSymbol methodParameterWithDefault:
							results.Add(methodParameterWithDefault.ExplicitDefaultValue);
							break;

						case TypedConstant argArray when argArray.Kind == TypedConstantKind.Array && !argArray.IsNull:
							results.AddRange(GetFlattenedArgumentPrimitives(argArray.Values.Cast<object>()));
							break;

						case TypedConstant nullObjectArray when nullObjectArray.Kind == TypedConstantKind.Array && nullObjectArray.IsNull:
							results.Add(null);
							break;
					}
				}

				return results.ToImmutableArray();
			}

			// Effective test arguments consist of InlineData argument typed constants concatenated
			// with default parameters of a test method providing such default parameters are not covered by InlineData arguments.
			ImmutableArray<object> GetEffectiveTestArguments(AttributeData attributeData)
			{
				var inlineDataObjectArrayArgument = attributeData.ConstructorArguments.Single();

				// special case InlineData(null): the compiler will treat the whole data array as being initialized to null
				var inlineDataArguments =
					inlineDataObjectArrayArgument.IsNull
						? ImmutableArray.Create(inlineDataObjectArrayArgument)
						: inlineDataObjectArrayArgument.Values;

				var methodDefaultValuesNonCoveredByInlineData =
					methodParametersWithExplicitDefaults
						.Where(p => p.Ordinal >= inlineDataArguments.Length);

				var allMethodArguments =
					inlineDataArguments
						.Cast<object>()
						.Concat(methodDefaultValuesNonCoveredByInlineData);

				return allMethodArguments.ToImmutableArray();
			}
		}
	}
}
