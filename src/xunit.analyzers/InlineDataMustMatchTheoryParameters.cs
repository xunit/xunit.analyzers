﻿using System;
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
    public class InlineDataMustMatchTheoryParameters : XunitDiagnosticAnalyzer
    {
        private static readonly IList<ExpressionSyntax> EmptyExpressionList = new ExpressionSyntax[0];

        internal static readonly string ParameterIndex = "ParameterIndex";
        internal static readonly string ParameterName = "ParameterName";
        internal static readonly string ParameterArrayStyle = "ParameterArrayStyle";

        public InlineDataMustMatchTheoryParameters()
        {
        }

        /// <summary>For testing purposes only.</summary>
        protected InlineDataMustMatchTheoryParameters(string assemblyVersion) : base(new Version(assemblyVersion))
        {
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(
               Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues,
               Descriptors.X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType,
               Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue,
               Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter
               );

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            var xunitSupportsParameterArrays = xunitContext.Core.TheorySupportsParameterArrays;
            var xunitSupportsDefaultParameterValues = xunitContext.Core.TheorySupportsDefaultParameterValues;

            var compilation = compilationStartContext.Compilation;
            var systemRuntimeInteropServicesOptionalAttribute =
                compilation.GetTypeByMetadataName(Constants.Types.SystemRuntimeInteropServicesOptionalAttribute);
            var objectArrayType = compilation.CreateArrayTypeSymbol(compilation.ObjectType);

            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var method = (IMethodSymbol)symbolContext.Symbol;

                var attributes = method.GetAttributes();
                if (!attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
                    return;

                foreach (var attribute in attributes)
                {
                    symbolContext.CancellationToken.ThrowIfCancellationRequested();

                    if (!attribute.AttributeClass.Equals(xunitContext.Core.InlineDataAttributeType))
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
                        dataParameterExpressions = attributeSyntax.ArgumentList?.Arguments.Select(a => a.Expression).ToList()
                            ?? new List<ExpressionSyntax>();
                    }

                    var dataArrayArgument = attribute.ConstructorArguments.Single();
                    // Need to special case InlineData(null) as the compiler will treat the whole data array as being initialized to null
                    var values = dataArrayArgument.IsNull ? ImmutableArray.Create(dataArrayArgument) : dataArrayArgument.Values;
                    if (values.Length < method.Parameters.Count(p => RequiresMatchingValue(p, 
                                                                        xunitSupportsParameterArrays, 
                                                                        xunitSupportsDefaultParameterValues, 
                                                                        systemRuntimeInteropServicesOptionalAttribute)))
                    {
                        var builder = ImmutableDictionary.CreateBuilder<string, string>();
                        builder[ParameterArrayStyle] = arrayStyle.ToString();
                        symbolContext.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues,
                            attributeSyntax.GetLocation(),
                            builder.ToImmutable()));
                    }

                    int valueIdx = 0, paramIdx = 0;
                    for (; valueIdx < values.Length && paramIdx < method.Parameters.Length; valueIdx++)
                    {
                        var parameter = method.Parameters[paramIdx];

                        // unwrap parameter type when the argument is a parameter list
                        var parameterType = xunitSupportsParameterArrays && parameter.IsParams && parameter.Type is IArrayTypeSymbol arrayParam
                        ? arrayParam.ElementType
                        : parameter.Type;

                        if (Equals(parameterType, compilation.ObjectType))
                        {
                            // Everything is assignable to object and 'params object[]' so move on
                            if (xunitSupportsParameterArrays && parameter.IsParams)
                            {
                                valueIdx = values.Length;
                                break;
                            }
                            else
                            {
                                paramIdx++;
                                continue;
                            }
                        }

                        var builder = ImmutableDictionary.CreateBuilder<string, string>();
                        builder[ParameterIndex] = paramIdx.ToString();
                        builder[ParameterName] = parameter.Name;
                        var properties = builder.ToImmutable();

                        var value = values[valueIdx];
                        if (!value.IsNull)
                        {
                            var isConvertible = ConversionChecker.IsConvertible(compilation, value.Type, parameterType, xunitContext);
                            if (!isConvertible)
                            {
                                symbolContext.ReportDiagnostic(Diagnostic.Create(
                                    Descriptors.X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType,
                                    dataParameterExpressions[valueIdx].GetLocation(),
                                    properties,
                                    parameter.Name,
                                    SymbolDisplay.ToDisplayString(parameterType)));
                            }
                        }

                        if (value.IsNull
                            && parameterType.IsValueType
                            && parameterType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
                        {
                            symbolContext.ReportDiagnostic(Diagnostic.Create(
                                Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter,
                                dataParameterExpressions[valueIdx].GetLocation(),
                                properties,
                                parameter.Name,
                                SymbolDisplay.ToDisplayString(parameterType)));
                        }

                        if (!parameter.IsParams)
                        {
                            // Stop moving paramIdx forward if the argument is a parameter array, regardless of xunit's support for it
                            paramIdx++;
                        }
                    }

                    for (; valueIdx < values.Length; valueIdx++)
                    {
                        var builder = ImmutableDictionary.CreateBuilder<string, string>();
                        builder[ParameterIndex] = valueIdx.ToString();
                        symbolContext.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue,
                            dataParameterExpressions[valueIdx].GetLocation(),
                            builder.ToImmutable(),
                            values[valueIdx].ToCSharpString()));
                    }
                }
            }, SymbolKind.Method);
        }

        private static bool RequiresMatchingValue(IParameterSymbol parameter, bool supportsParamsArray,
            bool supportsDefaultValue, INamedTypeSymbol optionalAttribute)
        {
            return !(parameter.HasExplicitDefaultValue && supportsDefaultValue)
                && !(parameter.IsParams && supportsParamsArray)
                && !parameter.GetAttributes().Any(a => a.AttributeClass.Equals(optionalAttribute));
        }

        static IList<ExpressionSyntax> GetParameterExpressionsFromArrayArgument(AttributeSyntax attribute)
        {
            if (attribute.ArgumentList?.Arguments.Count != 1)
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

            if (initializer == null)
            {
                return EmptyExpressionList;
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

        private static class ConversionChecker
        {
            public static bool IsConvertible(Compilation compilation, ITypeSymbol source, ITypeSymbol destination,
                XunitContext xunitContext)
            {
                if (destination.TypeKind == TypeKind.TypeParameter)
                    return IsConvertibleTypeParameter(source, (ITypeParameterSymbol) destination);

                var conversion = compilation.ClassifyConversion(source, destination);

                if (conversion.IsNumeric)
                    return IsConvertibleNumeric(source, destination);

                if (destination.SpecialType == SpecialType.System_DateTime
                    || (xunitContext.Core.TheorySupportsConversionFromStringToDateTimeOffsetAndGuid
                        && IsDateTimeOffsetOrGuid(destination)))
                {
                    // Allow all conversions from strings. All parsing issues will be reported at runtime.
                    return source.SpecialType == SpecialType.System_String;
                }

                // Rules of last resort
                return conversion.IsImplicit 
                       || conversion.IsUnboxing 
                       || (conversion.IsExplicit && conversion.IsNullable);
            }

            private static bool IsConvertibleTypeParameter(ITypeSymbol source, ITypeParameterSymbol destination)
            {
                if (destination.HasValueTypeConstraint && !source.IsValueType)
                    return false;
                if (destination.HasReferenceTypeConstraint && source.IsValueType)
                    return false;

                return destination.ConstraintTypes.All(c => c.IsAssignableFrom(source));
            }

            private static bool IsConvertibleNumeric(ITypeSymbol source, ITypeSymbol destination)
            {
                if (destination.SpecialType == SpecialType.System_Char
                    && (source.SpecialType == SpecialType.System_Double 
                        || source.SpecialType == SpecialType.System_Single))
                {
                    // Conversions from float to char (though numeric) do not actually work at runtime, so report them
                    return false;
                }

                return true; // Allow all numeric conversions. Narrowing conversion issues will be reported at runtime.
            }

            private static bool IsDateTimeOffsetOrGuid(ITypeSymbol destination)
            {
                if (destination.ContainingNamespace?.Name != "System")
                    return false;

                return destination.MetadataName == "DateTimeOffset" || destination.MetadataName == "Guid";
            }
        }
    }
}
