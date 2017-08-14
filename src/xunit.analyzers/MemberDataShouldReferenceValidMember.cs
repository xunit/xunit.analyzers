using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberDataShouldReferenceValidMember : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(
               Descriptors.X1014_MemberDataShouldUseNameOfOperator,
               Descriptors.X1015_MemberDataMustReferenceExistingMember,
               Descriptors.X1016_MemberDataMustReferencePublicMember,
               Descriptors.X1017_MemberDataMustReferenceStaticMember,
               Descriptors.X1018_MemberDataMustReferenceValidMemberKind,
               Descriptors.X1019_MemberDataMustReferenceMemberOfValidType,
               Descriptors.X1020_MemberDataPropertyMustHaveGetter,
               Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;

                var memberDataType = compilation.GetTypeByMetadataName(Constants.Types.XunitMemberDataAttribute);
                if (memberDataType == null)
                    return;

                var iEnumerableOfObjectArrayType = compilation.GetIEnumerableOfObjectArrayType();

                var supportsNameofOperator = compilation is CSharpCompilation cSharpCompilation
                    && cSharpCompilation.LanguageVersion >= LanguageVersion.CSharp6;

                compilationStartContext.RegisterSyntaxNodeAction(symbolContext =>
                {
                    var attribute = symbolContext.Node as AttributeSyntax;
                    var semanticModel = symbolContext.SemanticModel;
                    if (semanticModel.GetTypeInfo(attribute, symbolContext.CancellationToken).Type != memberDataType)
                        return;

                    var memberNameArgument = attribute.ArgumentList.Arguments.FirstOrDefault();
                    if (memberNameArgument == null)
                        return;

                    var constantValue = semanticModel.GetConstantValue(memberNameArgument.Expression, symbolContext.CancellationToken);
                    var memberName = constantValue.Value as string;
                    if (memberName == null)
                        return;

                    var memberTypeArgument = attribute.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.ValueText == "MemberType");
                    ITypeSymbol memberTypeSymbol = null;
                    if (memberTypeArgument?.Expression is TypeOfExpressionSyntax typeofExpression)
                    {
                        var typeSyntax = typeofExpression.Type;
                        memberTypeSymbol = semanticModel.GetTypeInfo(typeSyntax, symbolContext.CancellationToken).Type;
                    }

                    var testClassTypeSymbol = semanticModel.GetDeclaredSymbol(attribute.FirstAncestorOrSelf<ClassDeclarationSyntax>());
                    var declaredMemberTypeSymbol = memberTypeSymbol ?? testClassTypeSymbol;
                    var memberSymbol = FindMemberSymbol(memberName, declaredMemberTypeSymbol);

                    if (memberSymbol == null)
                    {
                        symbolContext.ReportDiagnostic(Diagnostic.Create(
                                  Descriptors.X1015_MemberDataMustReferenceExistingMember,
                                  attribute.GetLocation(),
                                  memberName,
                                  SymbolDisplay.ToDisplayString(declaredMemberTypeSymbol)));
                    }
                    else
                    {
                        if (memberSymbol.Kind != SymbolKind.Field &&
                            memberSymbol.Kind != SymbolKind.Property &&
                            memberSymbol.Kind != SymbolKind.Method)
                        {
                            symbolContext.ReportDiagnostic(Diagnostic.Create(
                                Descriptors.X1018_MemberDataMustReferenceValidMemberKind,
                                attribute.GetLocation()));
                        }
                        else
                        {
                            if (supportsNameofOperator && memberNameArgument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                var builder = ImmutableDictionary.CreateBuilder<string, string>();
                                if (memberSymbol.ContainingType != testClassTypeSymbol)
                                {
                                    builder.Add("DeclaringType", memberSymbol.ContainingType.ToDisplayString());
                                }
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    Descriptors.X1014_MemberDataShouldUseNameOfOperator,
                                    memberNameArgument.Expression.GetLocation(),
                                    builder.ToImmutable(),
                                    memberName,
                                    memberSymbol.ContainingType.ToDisplayString()
                                    ));
                            }

                            var memberProperties = new Dictionary<string, string> {
                                                    { "DeclaringType", declaredMemberTypeSymbol.ToDisplayString() },
                                                    { "MemberName", memberName }
                                                }.ToImmutableDictionary();
                            if (memberSymbol.DeclaredAccessibility != Accessibility.Public)
                            {
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    Descriptors.X1016_MemberDataMustReferencePublicMember,
                                    attribute.GetLocation(),
                                    memberProperties));
                            }
                            if (!memberSymbol.IsStatic)
                            {
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    Descriptors.X1017_MemberDataMustReferenceStaticMember,
                                    attribute.GetLocation(),
                                    memberProperties));
                            }
                            var memberType = GetMemberType(memberSymbol);
                            if (!iEnumerableOfObjectArrayType.IsAssignableFrom(memberType))
                            {
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    Descriptors.X1019_MemberDataMustReferenceMemberOfValidType,
                                    attribute.GetLocation(),
                                    memberProperties,
                                    SymbolDisplay.ToDisplayString(iEnumerableOfObjectArrayType),
                                    SymbolDisplay.ToDisplayString(memberType)));
                            }
                            if (memberSymbol.Kind == SymbolKind.Property && ((IPropertySymbol)memberSymbol).GetMethod == null)
                            {
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    Descriptors.X1020_MemberDataPropertyMustHaveGetter,
                                    attribute.GetLocation()));
                            }
                            var extraArguments = attribute.ArgumentList.Arguments
                                .Skip(1)
                                .TakeWhile(a => a.NameEquals == null)
                                .ToList();
                            if (memberSymbol.Kind == SymbolKind.Property || memberSymbol.Kind == SymbolKind.Field)
                            {
                                if (extraArguments.Any())
                                {
                                    var span = TextSpan.FromBounds(extraArguments.First().Span.Start, extraArguments.Last().Span.End);
                                    symbolContext.ReportDiagnostic(Diagnostic.Create(
                                        Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters,
                                        Location.Create(attribute.SyntaxTree, span)));
                                }
                            }
                            if (memberSymbol.Kind == SymbolKind.Method)
                            {
                                // TODO: handle method paramater type matching, model after InlineDataMustMatchTheoryParameter
                            }
                        }
                    }
                }, SyntaxKind.Attribute);
            });
        }

        static ITypeSymbol GetMemberType(ISymbol memberSymbol)
        {
            switch (memberSymbol)
            {
                case IPropertySymbol prop: return prop.Type;
                case IFieldSymbol field: return field.Type;
                case IMethodSymbol method: return method.ReturnType;
                default: return null;
            }
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
