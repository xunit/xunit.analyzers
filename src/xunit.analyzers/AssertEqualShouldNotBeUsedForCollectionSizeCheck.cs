using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            "System.Collections.Generic.IEnumerable<TSource>.Count<TSource>()"
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
                !IsCollectionCountProperty(context, symbolInfo) &&
                !IsGenericCountProperty(context, symbolInfo))
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
            return SizeMethods.Contains(SymbolDisplay.ToDisplayString(symbolInfo.Symbol.OriginalDefinition));
        }

        private static bool IsCollectionCountProperty(SyntaxNodeAnalysisContext context, SymbolInfo symbolInfo)
        {
            var collectionCountSymbol = context.SemanticModel.Compilation
                .GetTypeByMetadataName(typeof(ICollection).FullName)
                .GetMembers(nameof(ICollection.Count))
                .Single();

            var collectionSymbolImplementation = symbolInfo.Symbol.ContainingType.FindImplementationForInterfaceMember(collectionCountSymbol);
            return collectionSymbolImplementation != null && collectionSymbolImplementation.Equals(symbolInfo.Symbol);
        }

        private static bool IsGenericCountProperty(SyntaxNodeAnalysisContext context, SymbolInfo symbolInfo)
        {
            if (symbolInfo.Symbol.ContainingType.TypeArguments.IsEmpty)
                return false;

            var genericCollectionCountSymbol = context.SemanticModel.Compilation
                .GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T)
                .Construct(symbolInfo.Symbol.ContainingType.TypeArguments.ToArray())
                .GetMembers(nameof(ICollection<int>.Count))
                .Single();

            var genericCollectionSymbolImplementation = symbolInfo.Symbol.ContainingType.FindImplementationForInterfaceMember(genericCollectionCountSymbol);
            return genericCollectionSymbolImplementation != null && genericCollectionSymbolImplementation.Equals(symbolInfo.Symbol);
        }
    }
}