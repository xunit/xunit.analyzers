using System;
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
    public class InlineDataMustMatchTheoryParameters : DiagnosticAnalyzer
    {
        internal static readonly string ParameterIndex = "ParameterIndex";
        internal static readonly string ParameterName = "ParameterName";
        internal static readonly string ParameterArrayStyle = "ParameterArrayStyle";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(
               Constants.Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues,
               Constants.Descriptors.X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType,
               Constants.Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue,
               Constants.Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter
               );

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                var theoryType = compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
                var inlineDataType = compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute);
                if (theoryType == null || inlineDataType == null)
                    return;

                var objectArrayType = compilation.CreateArrayTypeSymbol(compilation.ObjectType);

                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var method = (IMethodSymbol)symbolContext.Symbol;

                    var attributes = method.GetAttributes();
                    if (!attributes.ContainsAttributeType(theoryType))
                        return;

                    foreach (var attribute in attributes)
                    {
                        symbolContext.CancellationToken.ThrowIfCancellationRequested();

                        if (attribute.AttributeClass != inlineDataType)
                            continue;

                        // Check if the semantic model indicates there are no syntax/compilation errors
                        if (attribute.ConstructorArguments.Length != 1 || !objectArrayType.Equals(attribute.ConstructorArguments.FirstOrDefault().Type))
                            continue;

                        var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference.GetSyntax(symbolContext.CancellationToken);

                        var arrayStyle = ParameterArrayStyleType.Initializer;
                        var dataParameterExpressions = GetParameterExpressionsFromArrayArgument(attributeSyntax);
                        if (dataParameterExpressions == null)
                        {
                            arrayStyle = ParameterArrayStyleType.Params;
                            dataParameterExpressions = attributeSyntax.ArgumentList.Arguments.Select(a => a.Expression).ToList();
                        }

                        var dataArrayArgument = attribute.ConstructorArguments.Single();
                        // Need to special case InlineData(null) as the compiler will treat the whole data array as being initialized to null
                        var values = dataArrayArgument.IsNull ? ImmutableArray.Create(dataArrayArgument) : dataArrayArgument.Values;
                        if (values.Length < method.Parameters.Length)
                        {
                            var builder = ImmutableDictionary.CreateBuilder<string, string>();
                            builder[ParameterArrayStyle] = arrayStyle.ToString();
                            symbolContext.ReportDiagnostic(Diagnostic.Create(
                                Constants.Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues,
                                attributeSyntax.GetLocation(),
                                builder.ToImmutable()));
                        }

                        for (int i = 0; i < Math.Min(values.Length, method.Parameters.Length); i++)
                        {
                            var parameter = method.Parameters[i];
                            if (parameter.Type == compilation.ObjectType)
                                continue; // Everything is assignable to object so move one

                            var builder = ImmutableDictionary.CreateBuilder<string, string>();
                            builder[ParameterIndex] = i.ToString();
                            builder[ParameterName] = parameter.Name;
                            var properties = builder.ToImmutable();

                            var value = values[i];
                            if (!value.IsNull)
                            {
                                var isConvertible = DetermineIsConvertible(compilation, value.Type, parameter.Type);
                                if (!isConvertible)
                                {
                                    symbolContext.ReportDiagnostic(Diagnostic.Create(
                                        Constants.Descriptors.X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType,
                                        dataParameterExpressions[i].GetLocation(),
                                        properties,
                                        parameter.Name,
                                        SymbolDisplay.ToDisplayString(parameter.Type)));
                                }
                            }

                            if (value.IsNull && parameter.Type.IsValueType && parameter.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
                            {
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    Constants.Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter,
                                    dataParameterExpressions[i].GetLocation(),
                                    properties,
                                    parameter.Name,
                                    SymbolDisplay.ToDisplayString(parameter.Type)));
                            }
                        }

                        for (int i = method.Parameters.Length; i < values.Length; i++)
                        {
                            var builder = ImmutableDictionary.CreateBuilder<string, string>();
                            builder[ParameterIndex] = i.ToString();
                            symbolContext.ReportDiagnostic(Diagnostic.Create(
                                Constants.Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue,
                                dataParameterExpressions[i].GetLocation(),
                                builder.ToImmutable(),
                                values[i].ToCSharpString()));
                        }
                    }
                }, SymbolKind.Method);
            });
        }

        static bool DetermineIsConvertible(Compilation compilation, ITypeSymbol source, ITypeSymbol destination)
        {
            var conversion = compilation.ClassifyConversion(source, destination);
            if (conversion.IsNumeric)
                return true; // Allow all numeric conversions. Narrowing conversion issues will be reported at runtime.
            var isConvertible = conversion.IsImplicit || conversion.IsUnboxing || (conversion.IsExplicit && conversion.IsReference);
            return isConvertible;
        }

        static List<ExpressionSyntax> GetParameterExpressionsFromArrayArgument(AttributeSyntax attribute)
        {
            if (attribute.ArgumentList.Arguments.Count != 1)
                return null;

            var argumentExpression = attribute.ArgumentList.Arguments.Single().Expression;
            InitializerExpressionSyntax initializer = null;
            switch (argumentExpression.Kind())
            {
                case SyntaxKind.ArrayCreationExpression:
                    initializer = ((ArrayCreationExpressionSyntax)argumentExpression).Initializer;
                    break;
                case SyntaxKind.ImplicitArrayCreationExpression:
                    initializer = ((ImplicitArrayCreationExpressionSyntax)argumentExpression).Initializer;
                    break;
                default:
                    return null;
            }
            return initializer.Expressions.ToList();
        }

        internal enum ParameterArrayStyleType
        {
            /// <summary>
            /// E.g. InlineData(1, 2, 3)
            /// </summary>
            Params,
            /// <summary>
            /// E.g. InlineData(data: new object[] { 1, 2, 3 }) or InlineData(new object[] { 1, 2, 3 })
            /// </summary>
            Initializer,
        }
    }
}
