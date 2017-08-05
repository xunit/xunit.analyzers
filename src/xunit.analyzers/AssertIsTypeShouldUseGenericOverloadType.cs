using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertIsTypeShouldUseGenericOverloadType : AssertUsageAnalyzerBase
    {
        internal const string MethodName = "MethodName";
        internal const string TypeName = "TypeName";
        internal static HashSet<string> IsTypeMethods = new HashSet<string>(new[] { "IsType", "IsNotType", "IsAssignableFrom" });

        public AssertIsTypeShouldUseGenericOverloadType() :
            base(Descriptors.X2007_AssertIsTypeShouldUseGenericOverload, IsTypeMethods)
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            var arguments = invocation.ArgumentList.Arguments;
            if (arguments.Count != 2)
                return;

            var typeOfExpression = arguments[0].Expression as TypeOfExpressionSyntax;
            if (typeOfExpression == null)
                return;

            var typeInfo = context.SemanticModel.GetTypeInfo(typeOfExpression.Type);
            var typeName = SymbolDisplay.ToDisplayString(typeInfo.Type);

            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder[MethodName] = method.Name;
            builder[TypeName] = typeName;
            
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.X2007_AssertIsTypeShouldUseGenericOverload,
                invocation.GetLocation(),
                builder.ToImmutable(),
                typeName));
        }
    }
}