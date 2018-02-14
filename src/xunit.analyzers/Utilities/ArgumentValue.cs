using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Utilities
{
    /// <summary>
    /// A common model of test method argument value obtained either as a InlineData attribute argument 
    /// typed constant or a test method parameter default value.
    /// </summary>
    /// <remarks>
    /// This data structure should avoid allocations, hence structs (ImmutableArray with a builder) and no LINQ
    /// </remarks>
    internal struct ArgumentValue : IEquatable<ArgumentValue>
    {
        private readonly bool _isNegativeZero; // only for doubles and floats
        private readonly bool _isPositiveZero; // only for doubles and floats

        public bool IsNull { get; }

        public bool IsArray { get; }

        public object Value { get; }

        public IImmutableList<ArgumentValue> Values { get; }

        public ArgumentValue(TypedConstant typedConstant, AttributeArgumentSyntax argumentSyntax = null)
        {
            IsNull = typedConstant.IsNull;
            IsArray = typedConstant.Kind == TypedConstantKind.Array;
            Value = !IsArray ? typedConstant.Value : null;
            // This is incorrect as we should pass syntaxes to CreateMany too, but it is a compromise between 
            // complexity and the ability to catch most common usage of those corner cases that syntaxes address
            Values = IsArray && !IsNull ? CreateMany(typedConstant.Values) : ImmutableArray<ArgumentValue>.Empty;
            
            SetPositiveAndNegativeZeros(typedConstant.Type, argumentSyntax?.Expression, Value, 
                out _isNegativeZero, out _isPositiveZero);
        }

        public ArgumentValue(IParameterSymbol parameterSymbol)
        {
            IsNull = parameterSymbol.ExplicitDefaultValue == null;
            IsArray = false;
            Value = parameterSymbol.ExplicitDefaultValue;
            Values = ImmutableArray<ArgumentValue>.Empty;

            var paramSyntax = parameterSymbol.DeclaringSyntaxReferences[0].GetSyntax() as ParameterSyntax;
            var defaultValueExpressionSyntax = paramSyntax?.Default.Value;
            SetPositiveAndNegativeZeros(parameterSymbol.Type, defaultValueExpressionSyntax, Value, 
                out _isNegativeZero, out _isPositiveZero);
        }

        /// <summary>
        /// Creates an equivalent to 'new object[] { argumentValues }' parameter. This is especially useful when
        /// constructing a single root argument value representing all method argument values, i.e. 
        /// [InlineData(1,2)] Test(int x, int y) is equivalent to [InlineData(new object[] {1,2})] Test(int x, int y).
        /// This simplifies the recursive algorithm used in Equals and GetHashCode methods.
        /// </summary>
        public ArgumentValue(ImmutableArray<ArgumentValue> argumentValues)
        {
            // A special search for a degenerated case of either:
            // 1. InlineData(null) or
            // 2. InlineData() and a single param method with default returning null.
            IsNull = argumentValues == null
                     || (argumentValues.Length == 1 && argumentValues[0].IsNull);

            IsArray = true;
            Value = null;
            Values = argumentValues;
            _isPositiveZero = false;
            _isNegativeZero = false;
        }

        /// <summary>
        /// Creates an array of argument values either from inline data attribute arguments [InlineData(1, 2)] 
        /// or nested object array arguments [InlineData(1, new object[] {1, 2})]. Attribute data is needed
        /// to obtain syntax necessary for to indicate corner cases.
        /// </summary>
        public static ImmutableArray<ArgumentValue> CreateMany(IEnumerable<TypedConstant> typedConstants, 
            AttributeData attributeData = null)
        {
            var argumentSyntaxes = (attributeData?
                                       .ApplicationSyntaxReference.GetSyntax() as AttributeSyntax)?
                                       .ArgumentList?
                                       .Arguments ?? new SeparatedSyntaxList<AttributeArgumentSyntax>();
            int index = 0;
            var valuesBuilder = ImmutableArray.CreateBuilder<ArgumentValue>();
            foreach (var typedConstantValue in typedConstants)
            {
                var argumentSyntax = index < argumentSyntaxes.Count ? argumentSyntaxes[index] : null;
                valuesBuilder.Add(new ArgumentValue(typedConstantValue, argumentSyntax));
                index++;
            }

            return valuesBuilder.ToImmutable();
        }

        /// <summary>
        /// Creates an array of argument values from default parameter values found in test methods, e.g.:
        /// TestMethod(int x = 2, int y = default(int))
        /// </summary>
        public static ImmutableArray<ArgumentValue> CreateMany(IEnumerable<IParameterSymbol> parameterSymbols)
        {
            var valuesBuilder = ImmutableArray.CreateBuilder<ArgumentValue>();
            foreach (var parameterSymbol in parameterSymbols)
            {
                valuesBuilder.Add(new ArgumentValue(parameterSymbol));
            }

            return valuesBuilder.ToImmutable();
        }

        /// <summary>
        /// Since arguments can be object[] at any level we need to compare 2 sequences of trees for equality.
        /// The algorithm traverses each tree in a sequence and compares with the corresponding tree in the other sequence.
        /// Any difference at any stage results in inequality proved and <c>false</c> returned.
        /// </summary>
        public bool Equals(ArgumentValue other)
        {
            if (IsNull && other.IsNull)
                return true;

            if (!IsArray && !other.IsArray)
            {
                if (!Equals(Value, other.Value))
                    return false;

                // -0.0 and 0.0 (we treat as non-equal arguments), see: https://github.com/xunit/xunit/issues/1489
                if ((_isPositiveZero && other._isNegativeZero)
                    || (_isNegativeZero && other._isPositiveZero))
                    return false;
            }
            else if (IsArray && other.IsArray && !IsNull && !other.IsNull)
            {
                if (Values.Count != other.Values.Count)
                    return false;

                for (int i = 0; i < Values.Count; i++)
                {
                    if (!Values[i].Equals(other.Values[i]))
                        return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ArgumentValue))
                return false;

            return Equals((ArgumentValue) obj);
        }

        public override int GetHashCode()
        {
            if (IsNull)
                return 0;

            if (!IsArray)
                return Value?.GetHashCode() ?? 0;

            var hash = 17;

            foreach (var singleArgumentValue in Values)
            {
                hash = hash * 31 + singleArgumentValue.GetHashCode();
            }

            return hash;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private static void SetPositiveAndNegativeZeros(ITypeSymbol typeSymbol,
            ExpressionSyntax valueExpressionSyntax, object value, out bool isNegativeZero, 
            out bool isPositiveZero)
        {
            if (typeSymbol.SpecialType == SpecialType.System_Double && (double) value == 0.0
                || typeSymbol.SpecialType == SpecialType.System_Single && (float) value == 0.0f)
            {
                if (valueExpressionSyntax != null
                    && valueExpressionSyntax is PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax
                    && prefixUnaryExpressionSyntax.Kind() == SyntaxKind.UnaryMinusExpression)
                {
                    isNegativeZero = true;
                    isPositiveZero = false;
                }
                else
                {
                    isNegativeZero = false;
                    isPositiveZero = true;
                }
            }
            else
            {
                isNegativeZero = false;
                isPositiveZero = false;
            }
        }
    }
}
