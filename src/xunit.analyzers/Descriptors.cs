using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static Xunit.Analyzers.Category;

namespace Xunit.Analyzers
{
    internal enum Category
    {
        Usage,
        Assertions,
    }

    internal static class Descriptors
    {
        static ConcurrentDictionary<Category, string> categoryMapping = new ConcurrentDictionary<Category, string>();

        static DiagnosticDescriptor Rule(string id, string title, Category category, DiagnosticSeverity defaultSeverity, string messageFormat, string description = null)
        {
            var helpLink = $"https://xunit.github.io/xunit.analyzers/rules/{id}";
            var isEnabledByDefault = true;
            return new DiagnosticDescriptor(id, title, messageFormat, categoryMapping.GetOrAdd(category, c => c.ToString()), defaultSeverity, isEnabledByDefault, description, helpLink);
        }

        internal static DiagnosticDescriptor X1000_TestClassMustBePublic { get; } =
            Rule("xUnit1000", "Test classes must be public", Usage, Error,
                "Test classes must be public");

        internal static DiagnosticDescriptor X1001_FactMethodMustNotHaveParameters { get; } =
            Rule("xUnit1001", "Fact methods cannot have parameters", Usage, Error,
                "Fact methods cannot have parameters",
            description: "Remove the parameters from the method or convert it into a Theory.");

        internal static DiagnosticDescriptor X1002_TestMethodMustNotHaveMultipleFactAttributes { get; } =
            Rule("xUnit1002", "Test methods cannot have multiple Fact or Theory attributes", Usage, Error,
                "Test methods cannot have multiple Fact or Theory attributes");

        internal static DiagnosticDescriptor X1003_TheoryMethodMustHaveTestData { get; } =
            Rule("xUnit1003", "Theory methods must have test data", Usage, Error,
                "Theory methods must have test data",
            description: "Use InlineData, MemberData, or ClassData to provide test data for the Theory");

        internal static DiagnosticDescriptor X1004_TestMethodShouldNotBeSkipped { get; } =
            Rule("xUnit1004", "Test methods should not be skipped", Usage, Info,
                "Test methods should not be skipped");

        internal static DiagnosticDescriptor X1005_FactMethodShouldNotHaveTestData { get; } =
            Rule("xUnit1005", "Fact methods should not have test data", Usage, Warning,
                "Fact methods should not have test data");

        internal static DiagnosticDescriptor X1006_TheoryMethodShouldHaveParameters { get; } =
            Rule("xUnit1006", "Theory methods should have parameters", Usage, Warning,
                "Theory methods should have parameters");

        internal static DiagnosticDescriptor X1007_ClassDataAttributeMustPointAtValidClass { get; } =
            Rule("xUnit1007", "ClassData must point at a valid class", Usage, Error,
                "ClassData must point at a valid class",
            description: "The class {0} must be public, not sealed, with an empty constructor, and implement IEnumerable<object[]>.");

        internal static DiagnosticDescriptor X1008_DataAttributeShouldBeUsedOnATheory { get; } =
            Rule("xUnit1008", "Test data attribute should only be used on a Theory", Usage, Warning,
                "Test data attribute should only be used on a Theory");

        internal static DiagnosticDescriptor X1009_InlineDataMustMatchTheoryParameters_TooFewValues { get; } =
            Rule("xUnit1009", "InlineData values must match the number of method parameters", Usage, Error,
                "InlineData values must match the number of method parameters");

        internal static DiagnosticDescriptor X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType { get; } =
            Rule("xUnit1010", "The value is not convertible to the method parameter type", Usage, Error,
                "The value is not convertible to the method parameter '{0}' of type '{1}'.");

        internal static DiagnosticDescriptor X1011_InlineDataMustMatchTheoryParameters_ExtraValue { get; } =
            Rule("xUnit1011", "There is no matching method parameter", Usage, Error,
                "There is no matching method parameter for value: {0}.");

        internal static DiagnosticDescriptor X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter { get; } =
            Rule("xUnit1012", "Null should not be used for value type parameters", Usage, Warning,
                "Null should not be used for value type parameter '{0}' of type '{1}'.",
            description: "XUnit will execute the theory initializing the parameter to the default value of the type, which might not be the desired behavior");

        internal static DiagnosticDescriptor X1013_PublicMethodShouldBeMarkedAsTest { get; } =
            Rule("xUnit1013", "Public method should be marked as test", Usage, Warning,
                "Public method '{0}' on test class '{1}' should be marked as a {2}.",
            description: "Public methods on a test class that return void or Task should be marked as tests or have their accessibility reduced. While test methods do not have to be public "
            + " having public non-test methods might indicate that a method was intended to be a test but the annotation was not applied.");

        internal static DiagnosticDescriptor X1014_MemberDataShouldUseNameOfOperator { get; } =
            Rule("xUnit1014", "MemberData should use nameof operator for member name", Usage, Warning,
                "MemberData should use nameof operator to reference member '{0}' on type '{1}'.");

        internal static DiagnosticDescriptor X1015_MemberDataMustReferenceExistingMember { get; } =
            Rule("xUnit1015", "MemberData must reference an existing member", Usage, Error,
                "MemberData must reference an existing member '{0}' on type '{1}'.");

        internal static DiagnosticDescriptor X1016_MemberDataMustReferencePublicMember { get; } =
            Rule("xUnit1016", "MemberData must reference a public member", Usage, Error,
                "MemberData must reference a public member");

        internal static DiagnosticDescriptor X1017_MemberDataMustReferenceStaticMember { get; } =
            Rule("xUnit1017", "MemberData must reference a static member", Usage, Error,
                "MemberData must reference a static member");

        internal static DiagnosticDescriptor X1018_MemberDataMustReferenceValidMemberKind { get; } =
            Rule("xUnit1018", "MemberData must reference a valid member kind", Usage, Error,
                "MemberData must reference a property, field, or method");

        internal static DiagnosticDescriptor X1019_MemberDataMustReferenceMemberOfValidType { get; } =
            Rule("xUnit1019", "MemberData must reference a member providing a valid data type", Usage, Error,
                "MemberData must reference a data type assignable to '{0}'. The referenced type '{1}' is not valid.");

        internal static DiagnosticDescriptor X1020_MemberDataPropertyMustHaveGetter { get; } =
            Rule("xUnit1020", "MemberData must reference a property with a getter", Usage, Error,
                "MemberData must reference a property with a getter");

        internal static DiagnosticDescriptor X1021_MemberDataNonMethodShouldNotHaveParameters { get; } =
            Rule("xUnit1021", "MemberData should not have parameters if the referenced member is not a method", Usage, Warning,
                "MemberData should not have parameters if the referenced member is not a method",
            description: "Additional MemberData parameters are only used for methods. They are ignored for fields and properties.");

        internal static DiagnosticDescriptor X1022_TheoryMethodCannotHaveParameterArray { get; } =
            Rule("xUnit1022", "Theory methods cannot have a parameter array", Usage, Error,
                "Theory method '{0}' on test class '{1}' cannot have a parameter array '{2}'.",
            description: "Params array support was added in Xunit 2.2. Remove the parameter or upgrade the Xunit binaries.");

        internal static DiagnosticDescriptor X1023_TheoryMethodCannotHaveDefaultParameter { get; } =
            Rule("xUnit1023", "Theory methods cannot have default parameter values", Usage, Error,
                "Theory method '{0}' on test class '{1}' parameter '{2}' cannot have a default value.",
            description: "Default parameter values support was added in Xunit 2.2. Remove the default value or upgrade the Xunit binaries.");

        internal static DiagnosticDescriptor X1024_TestMethodCannotHaveOverloads { get; } =
            Rule("xUnit1024", "Test methods cannot have overloads", Usage, Error,
                "Test method '{0}' on test class '{1}' has the same name as another method declared on class '{2}'.",
            description: "Test method overloads are not supported as most test runners cannot correctly invoke the appropriate overload. " +
            "This includes any combination of static and instance methods declared with any visibility in the same class or across a " +
            "class hierarchy. Rename one of the methods.");

        internal static DiagnosticDescriptor X1025_InlineDataShouldBeUniqueWithinTheory { get; } =
            Rule("xUnit1025", "InlineData should be unique within the Theory it belongs to", Usage, Warning,
                "Theory method '{0}' on test class '{1}' has InlineData duplicate(s).",
            description: "Theory should have all InlineData elements unique. Remove redundant attribute(s) from the theory method.");

        internal static DiagnosticDescriptor X1026_TheoryMethodShouldUseAllParameters { get; } =
            Rule("xUnit1026", "Theory methods should use all of their parameters", Usage, Warning,
                "Theory method '{0}' on test class '{1}' does not use parameter '{2}'.");

        // Placeholder for rule X1027

        // Placeholder for rule X1028

        // Placeholder for rule X1029

        // Placeholder for rule X1030

        // Placeholder for rule X1031

        // Placeholder for rule X1032

        internal static DiagnosticDescriptor X2000_AssertEqualLiteralValueShouldBeFirst { get; } =
            Rule("xUnit2000", "Expected value should be first", Assertions, Warning,
                "The literal or constant value {0} should be the first argument in the call to '{1}' in method '{2}' on type '{3}'.",
            description: "The xUnit Assertion library produces the best error messages if the expected value is passed in as the first argument.");

        internal static DiagnosticDescriptor X2001_AssertEqualsShouldNotBeUsed { get; } =
            Rule("xUnit2001", "Do not use invalid equality check", Assertions, Hidden,
                "Do not use {0}.");

        internal static DiagnosticDescriptor X2002_AssertNullShouldNotBeCalledOnValueTypes { get; } =
            Rule("xUnit2002", "Do not use null check on value type", Assertions, Warning,
                "Do not use {0} on value type '{1}'.");

        internal static DiagnosticDescriptor X2003_AssertEqualShouldNotUsedForNullCheck { get; } =
            Rule("xUnit2003", "Do not use equality check to test for null value", Assertions, Warning,
                "Do not use {0} to check for null value.");

        internal static DiagnosticDescriptor X2004_AssertEqualShouldNotUsedForBoolLiteralCheck { get; } =
            Rule("xUnit2004", "Do not use equality check to test for boolean conditions", Assertions, Warning,
                "Do not use {0} to check for boolean conditions.");

        internal static DiagnosticDescriptor X2005_AssertSameShouldNotBeCalledOnValueTypes { get; } =
            Rule("xUnit2005", "Do not use identity check on value type", Assertions, Warning,
                "Do not use {0} on value type '{1}'.",
            description: "The value type will be boxed which means its identity will always be different.");

        internal static DiagnosticDescriptor X2006_AssertEqualGenericShouldNotBeUsedForStringValue { get; } =
            Rule("xUnit2006", "Do not use invalid string equality check", Assertions, Warning,
                "Do not use {0} to test for string equality.");

        internal static DiagnosticDescriptor X2007_AssertIsTypeShouldUseGenericOverload { get; } =
            Rule("xUnit2007", "Do not use typeof expression to check the type", Assertions, Warning,
                "Do not use typeof({0}) expression to check the type.");

        internal static DiagnosticDescriptor X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck { get; } =
            Rule("xUnit2008", "Do not use boolean check to match on regular expressions", Assertions, Warning,
                "Do not use {0} to match on regular expressions.");

        internal static DiagnosticDescriptor X2009_AssertSubstringCheckShouldNotUseBoolCheck { get; } =
            Rule("xUnit2009", "Do not use boolean check to check for substrings", Assertions, Warning,
                "Do not use {0} to check for substrings.");

        internal static DiagnosticDescriptor X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer { get; } =
            Rule("xUnit2010", "Do not use boolean check to check for string equality", Assertions, Warning,
                "Do not use {0} to check for string equality.");

        internal static DiagnosticDescriptor X2011_AssertEmptyCollectionCheckShouldNotBeUsed { get; } =
            Rule("xUnit2011", "Do not use empty collection check", Assertions, Warning,
                "Do not use {0} to check for empty collections.");

        internal static DiagnosticDescriptor X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck { get; } =
            Rule("xUnit2012", "Do not use Enumerable.Any() to check if a value exists in a collection", Assertions, Warning,
                "Do not use Enumerable.Any() to check if a value exists in a collection.");

        internal static DiagnosticDescriptor X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck { get; } =
            Rule("xUnit2013", "Do not use equality check to check for collection size.", Assertions, Warning,
                "Do not use {0} to check for collection size.");

        internal static DiagnosticDescriptor X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck { get; } =
            Rule("xUnit2014", "Do not use throws check to check for asynchronously thrown exception", Assertions, Error,
                "Do not use {0} to check for asynchronously thrown exceptions.");

        internal static DiagnosticDescriptor X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck_Hidden { get; } =
            Rule("xUnit2014", "Do not use throws check to check for asynchronously thrown exception", Assertions, Hidden,
            "Do not use {0} to check for asynchronously thrown exceptions.");

        internal static DiagnosticDescriptor X2015_AssertThrowsShouldUseGenericOverload { get; } =
            Rule("xUnit2015", "Do not use typeof expression to check the exception type", Assertions, Warning,
                "Do not use typeof() expression to check the exception type.");

        internal static DiagnosticDescriptor X2016_AssertEqualPrecisionShouldBeInRange { get; } =
            Rule("xUnit2016", "Keep precision in the allowed range when asserting equality of doubles or decimals.", Assertions, Error,
                "Keep precision in range {0} when asserting equality of {1} typed actual value.");

        internal static DiagnosticDescriptor X2017_AssertCollectionContainsShouldNotUseBoolCheck { get; } =
            Rule("xUnit2017", "Do not use Contains() to check if a value exists in a collection", Assertions, Warning,
                "Do not use Contains() to check if a value exists in a collection.");

        internal static DiagnosticDescriptor X2018_AssertIsTypeShouldNotBeUsedForAbstractType { get; } =
            Rule("xUnit2018", "Do not compare an object's exact type to an abstract class or interface", Assertions, Warning,
                "Do not compare an object's exact type to the {0} '{1}'.");

        // Placeholder for rule X2019

        // Placeholder for rule X2020

        // Placeholder for rule X2021

        // Placeholder for rule X2022

        // Placeholder for rule X2023

        // Placeholder for rule X2024

        // Placeholder for rule X2025

        // Placeholder for rule X2026
    }
}
