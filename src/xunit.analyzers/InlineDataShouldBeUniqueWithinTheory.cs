using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers.Utilities;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InlineDataShouldBeUniqueWithinTheory : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.X1025_InlineDataShouldBeUniqueWithinTheory);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSymbolAction(
                symbolContext => AnalyzeMethod(symbolContext, xunitContext), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, XunitContext xunitContext)
        {
            var method = (IMethodSymbol)context.Symbol;

            var methodAllAttributes = method.GetAttributes();
            if (!methodAllAttributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
                return;

            var objectArrayType = TypeSymbolFactory.GetObjectArrayType(context.Compilation);

            var wellFormedInlineDataAttributes = methodAllAttributes
                .Where(a => a.AttributeClass == xunitContext.Core.InlineDataAttributeType
                            && HasAttributeDeclarationNoCompilationErrors(a, objectArrayType));

            AnalyzeInlineDataAttributesWithinTheory(context, wellFormedInlineDataAttributes);
        }

        private static void AnalyzeInlineDataAttributesWithinTheory(SymbolAnalysisContext context,
            IEnumerable<AttributeData> inlineDataAttributes)
        {
            var attributeRelatedMethod = (IMethodSymbol)context.Symbol;
            var uniqueAttributes = new HashSet<AttributeData>(new InlineDataUniquenessComparer(attributeRelatedMethod));

            foreach (var currentInlineData in inlineDataAttributes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (uniqueAttributes.Contains(currentInlineData))
                {
                    ReportDuplicate(context, currentInlineData);
                }
                else
                {
                    uniqueAttributes.Add(currentInlineData);
                }
            }
        }

        private static AttributeSyntax GetAttributeSyntax(SymbolAnalysisContext context, AttributeData attribute)
        {
            return (AttributeSyntax)attribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken);
        }

        private static void ReportDuplicate(SymbolAnalysisContext context, AttributeData duplicateAttribute)
        {
            var method = (IMethodSymbol)context.Symbol;

            var diagnostic = Diagnostic.Create(
                Descriptors.X1025_InlineDataShouldBeUniqueWithinTheory,
                GetAttributeSyntax(context, duplicateAttribute).GetLocation(),
                method.Name,
                method.ContainingType.Name
            );

            context.ReportDiagnostic(diagnostic);
        }

        private static bool HasAttributeDeclarationNoCompilationErrors(AttributeData attribute,
            IArrayTypeSymbol objectArrayType)
        {
            return attribute.ConstructorArguments.Length == 1 &&
                   objectArrayType.Equals(attribute.ConstructorArguments.FirstOrDefault().Type);
        }

        private class InlineDataUniquenessComparer : IEqualityComparer<AttributeData>
        {
            private readonly IMethodSymbol _attributeRelatedMethod;

            public InlineDataUniquenessComparer(IMethodSymbol attributeRelatedMethod)
            {
                _attributeRelatedMethod = attributeRelatedMethod;
            }

            public bool Equals(AttributeData x, AttributeData y)
            {
                var xArguments = GetEffectiveTestArgumentsAsArgumentRoot(x);
                var yArguments = GetEffectiveTestArgumentsAsArgumentRoot(y);

                return xArguments.Equals(yArguments);
            }

            public int GetHashCode(AttributeData obj)
            {
                return GetEffectiveTestArgumentsAsArgumentRoot(obj).GetHashCode();
            }

            /// <summary>
            /// Effective test arguments consist of InlineData argument typed constants concatenated 
            /// with default parameters of a test method providing such default parameters are not covered 
            /// by InlineData arguments. The result is represented as an argument value root to facilitate reqursive
            /// algorithm use. See the constructor of <see cref="ArgumentValue"/> accepting a collection of argument 
            /// values.
            /// </summary>
            private ArgumentValue GetEffectiveTestArgumentsAsArgumentRoot(AttributeData attributeData)
            {
                var inlineDataObjectArrayArgument = attributeData.ConstructorArguments.Single();

                // special case InlineData(null): the compiler treats the whole data array as being initialized to null
                var inlineDataArguments = inlineDataObjectArrayArgument.IsNull
                    ? ImmutableArray.Create(new ArgumentValue(inlineDataObjectArrayArgument))
                    : ArgumentValue.CreateMany(inlineDataObjectArrayArgument.Values, attributeData);

                var methodDefaultValuesNonCoveredByInlineData = ArgumentValue.CreateMany(
                    _attributeRelatedMethod.Parameters
                        .Where(p => p.HasExplicitDefaultValue && p.Ordinal >= inlineDataArguments.Length));

                var argumentValues = inlineDataArguments
                    .Concat(methodDefaultValuesNonCoveredByInlineData)
                    .ToImmutableArray();

                return new ArgumentValue(argumentValues);
            }
        }
    }
}
