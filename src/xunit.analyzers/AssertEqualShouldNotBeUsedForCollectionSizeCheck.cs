using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertEqualShouldNotBeUsedForCollectionSizeCheck : AssertUsageAnalyzerBase
    {
        internal static string MethodName = "MethodName";
        internal static string SizeValue = "SizeValue";

        private static readonly HashSet<string> EqualMethods = new HashSet<string>(new[] { "Equal", "NotEqual" });

        private static readonly HashSet<string> SizeMethods = new HashSet<string>
        {
            "System.Array.Length",
            "System.Collections.Generic.IEnumerable<TSource>.Count<TSource>()",
            "System.Collections.Immutable.ImmutableArray<T>.Length",
        };

        public AssertEqualShouldNotBeUsedForCollectionSizeCheck() :
            base(Descriptors.X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck, EqualMethods)
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            if (method.Parameters.Length != 2 ||
                !method.Parameters[0].Type.SpecialType.Equals(SpecialType.System_Int32) ||
                !method.Parameters[1].Type.SpecialType.Equals(SpecialType.System_Int32))
                return;

            var size = context.SemanticModel.GetConstantValue(invocation.ArgumentList.Arguments[0].Expression, context.CancellationToken);

            if (!size.HasValue || (int)size.Value < 0 || (int)size.Value > 1 || (int)size.Value == 1 && method.Name != "Equal")
                return;

            var expression =
                (ExpressionSyntax)(invocation.ArgumentList.Arguments[1].Expression as InvocationExpressionSyntax) ??
                (ExpressionSyntax)(invocation.ArgumentList.Arguments[1].Expression as MemberAccessExpressionSyntax);

            if (expression == null)
                return;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken);

            if (!IsWellKnownSizeMethod(symbolInfo) &&
                !IsICollectionCountProperty(context, symbolInfo) &&
                !IsICollectionOfTCountProperty(context, symbolInfo) &&
                !IsIReadOnlyCollectionOfTCountProperty(context, symbolInfo))
                return;

            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder[MethodName] = method.Name;
            builder[SizeValue] = size.Value.ToString();

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck,
                invocation.GetLocation(),
                builder.ToImmutable(),
                SymbolDisplay.ToDisplayString(method, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
        }

        private static bool IsWellKnownSizeMethod(SymbolInfo symbolInfo)
        {
            return SizeMethods.Contains(symbolInfo.Symbol.OriginalDefinition.ToDisplayString());
        }

        private static bool IsICollectionCountProperty(SyntaxNodeAnalysisContext context, SymbolInfo symbolInfo)
        {
            return IsCountPropertyOf(
                context.Compilation.GetTypeByMetadataName(Constants.Types.SystemCollectionsICollection),
                symbolInfo);
        }

        private static bool IsICollectionOfTCountProperty(SyntaxNodeAnalysisContext context, SymbolInfo symbolInfo)
        {
            return IsCountPropertyOfGenericType(
                context.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T),
                symbolInfo);
        }

        private static bool IsIReadOnlyCollectionOfTCountProperty(SyntaxNodeAnalysisContext context, SymbolInfo symbolInfo)
        {
            return IsCountPropertyOfGenericType(
                context.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T),
                symbolInfo);
        }

        private static bool IsCountPropertyOfGenericType(INamedTypeSymbol openCollectionType, SymbolInfo symbolInfo)
        {
            var containingType = symbolInfo.Symbol.ContainingType;
            var concreteCollectionType = containingType.GetGenericInterfaceImplementation(openCollectionType);
            return concreteCollectionType != null && IsCountPropertyOf(concreteCollectionType, symbolInfo);
        }

        private static bool IsCountPropertyOf(INamedTypeSymbol collectionType, SymbolInfo symbolInfo)
        {
            var memberSymbol = symbolInfo.Symbol;
            var containingType = memberSymbol.ContainingType;
            var countSymbol = collectionType.GetMember("Count");
            var countSymbolImplementation = containingType.FindImplementationForInterfaceMember(countSymbol);
            return countSymbolImplementation?.Equals(memberSymbol) ?? false;
        }
    }
}
