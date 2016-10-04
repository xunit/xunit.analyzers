using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberDataShouldUseNameOfOperator : DiagnosticAnalyzer
    {
        internal const string MemberType = "MemberType";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Constants.Descriptors.X1014_MemberDataShouldUseNameOfOperator);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var cSharpCompilation = compilationStartContext.Compilation as CSharpCompilation;
                if (cSharpCompilation == null || cSharpCompilation.LanguageVersion < LanguageVersion.CSharp6)
                    return;

                var memberDataType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitMemberDataAttribute);
                if (memberDataType == null)
                    return;

                compilationStartContext.RegisterSyntaxNodeAction(symbolContext =>
                {
                    var attribute = symbolContext.Node as AttributeSyntax;
                    var semanticModel = symbolContext.SemanticModel;
                    if (semanticModel.GetTypeInfo(attribute, symbolContext.CancellationToken).Type != memberDataType)
                        return;

                    var memberNameArgument = attribute.ArgumentList.Arguments.FirstOrDefault();
                    if (memberNameArgument == null || !memberNameArgument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                        return;

                    var memberName = ((LiteralExpressionSyntax)memberNameArgument.Expression).Token.ValueText;

                    var memberTypeArgument = attribute.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.ValueText == "MemberType");
                    ITypeSymbol memberTypeSymbol = null;
                    if (memberTypeArgument?.Expression is TypeOfExpressionSyntax)
                    {
                        var typeSyntax = ((TypeOfExpressionSyntax)memberTypeArgument.Expression).Type;
                        memberTypeSymbol = semanticModel.GetTypeInfo(typeSyntax, symbolContext.CancellationToken).Type;
                    }

                    var testClassTypeSymbol = semanticModel.GetDeclaredSymbol(attribute.FirstAncestorOrSelf<ClassDeclarationSyntax>());
                    var declaredMemberTypeSymbol = memberTypeSymbol ?? testClassTypeSymbol;
                    ISymbol memberSymbol = FindMemberSymbol(memberName, declaredMemberTypeSymbol);

                    if (memberSymbol != null)
                    {
                        var builder = ImmutableDictionary.CreateBuilder<string, string>();
                        if (memberSymbol.ContainingType != declaredMemberTypeSymbol)
                        {
                            builder.Add(MemberType, memberSymbol.ContainingType.ToDisplayString());
                        }
                        symbolContext.ReportDiagnostic(Diagnostic.Create(
                            Constants.Descriptors.X1014_MemberDataShouldUseNameOfOperator,
                            memberNameArgument.Expression.GetLocation(),
                            builder.ToImmutable(),
                            memberName,
                            memberSymbol.ContainingType.ToDisplayString()
                            ));
                    }
                }, SyntaxKind.Attribute);
            });
        }

        static ISymbol FindMemberSymbol(string memberName, ITypeSymbol type)
        {
            while (type != null)
            {
                var memberSymbol = type.GetMembers(memberName).FirstOrDefault();
                if (memberSymbol != null)
                    return memberSymbol;

                type = type.BaseType;
            }
            return null;
        }
    }
}
