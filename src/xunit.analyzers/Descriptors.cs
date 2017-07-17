using Microsoft.CodeAnalysis;
using static Xunit.Analyzers.Constants;

namespace Xunit.Analyzers
{
    internal static class Descriptors
    {
        internal static DiagnosticDescriptor X1000_TestClassMustBePublic { get; } = new DiagnosticDescriptor("xUnit1000",
            "Test classes must be public",
            "Test classes must be public",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1001_FactMethodMustNotHaveParameters { get; } = new DiagnosticDescriptor("xUnit1001",
            "Fact methods cannot have parameters",
            "Fact methods cannot have parameters",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
            description: "Remove the parameters from the method or convert it into a Theory.");

        internal static DiagnosticDescriptor X1002_TestMethodMustNotHaveMultipleFactAttributes { get; } = new DiagnosticDescriptor("xUnit1002",
            "Test methods cannot have multiple Fact or Theory attributes",
            "Test methods cannot have multiple Fact or Theory attributes",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1003_TheoryMethodMustHaveTestData { get; } = new DiagnosticDescriptor("xUnit1003",
            "Theory methods must have test data",
            "Theory methods must have test data",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
            description: "Use InlineData, MemberData, or ClassData to provide test data for the Theory");

        internal static DiagnosticDescriptor X1004_TestMethodShouldNotBeSkipped { get; } = new DiagnosticDescriptor("xUnit1004",
            "Test methods should not be skipped",
            "Test methods should not be skipped",
            Categories.Usage, DiagnosticSeverity.Info, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1005_FactMethodShouldNotHaveTestData { get; } = new DiagnosticDescriptor("xUnit1005",
            "Fact methods should not have test data",
            "Fact methods should not have test data",
            Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1006_TheoryMethodShouldHaveParameters { get; } = new DiagnosticDescriptor("xUnit1006",
            "Theory methods should have parameters",
            "Theory methods should have parameters",
            Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1007_ClassDataAttributeMustPointAtValidClass { get; } = new DiagnosticDescriptor("xUnit1007",
            "ClassData must point at a valid class",
            "ClassData must point at a valid class",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
            description: "The class {0} must be public, not sealed, with an empty constructor, and implement IEnumerable<object[]>.");

        internal static DiagnosticDescriptor X1008_DataAttributeShouldBeUsedOnATheory { get; } = new DiagnosticDescriptor("xUnit1008",
            "Test data attribute should only be used on a Theory",
            "Test data attribute should only be used on a Theory",
            Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1009_InlineDataMustMatchTheoryParameters_TooFewValues { get; } = new DiagnosticDescriptor("xUnit1009",
            "InlineData values must match the number of method parameters",
            "InlineData values must match the number of method parameters",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType { get; } = new DiagnosticDescriptor("xUnit1010",
            "The value is not convertible to the method parameter type",
            "The value is not convertible to the method parameter '{0}' of type '{1}'.",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1011_InlineDataMustMatchTheoryParameters_ExtraValue { get; } = new DiagnosticDescriptor("xUnit1011",
            "There is no matching method parameter",
            "There is no matching method parameter for value: {0}.",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter { get; } = new DiagnosticDescriptor("xUnit1012",
            "Null should not be used for value type parameters",
            "Null should not be used for value type parameter '{0}' of type '{1}'.",
            Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: "XUnit will execute the theory initializing the parameter to the default value of the type, which might not be the desired behavior");

        internal static DiagnosticDescriptor X1013_PublicMethodShouldBeMarkedAsTest { get; } = new DiagnosticDescriptor("xUnit1013",
            "Public method should be marked as test",
            "Public method '{0}' on test class '{1}' should be marked as a {2}.",
            Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: "Public methods on a test class that return void or Task should be marked as tests or have their accessibility reduced. While test methods do not have to be public "
            + " having public non-test methods might indicate that a method was intended to be a test but the annotation was not applied.");

        internal static DiagnosticDescriptor X1014_MemberDataShouldUseNameOfOperator { get; } = new DiagnosticDescriptor("xUnit1014",
            "MemberData should use nameof operator for member name",
            "MemberData should use nameof operator to reference member '{0}' on type '{1}'.",
            Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1015_MemberDataMustReferenceExistingMember { get; } = new DiagnosticDescriptor("xUnit1015",
            "MemberData must reference an existing member",
            "MemberData must reference an existing member '{0}' on type '{1}'.",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1016_MemberDataMustReferencePublicMember { get; } = new DiagnosticDescriptor("xUnit1016",
            "MemberData must reference a public member",
            "MemberData must reference a public member",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1017_MemberDataMustReferenceStaticMember { get; } = new DiagnosticDescriptor("xUnit1017",
            "MemberData must reference a static member",
            "MemberData must reference a static member",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1018_MemberDataMustReferenceValidMemberKind { get; } = new DiagnosticDescriptor("xUnit1018",
            "MemberData must reference a valid member type",
            "MemberData must reference a property, field, or method",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1019_MemberDataMustReferenceMemberOfValidType { get; } = new DiagnosticDescriptor("xUnit1019",
            "MemberData must reference a member providing a valid data type",
            "MemberData must reference a data type assignable to '{0}'. The referenced type '{1}' is not valid.",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1020_MemberDataPropertyMustHaveGetter { get; } = new DiagnosticDescriptor("xUnit1020",
            "MemberData must reference a property with a getter",
            "MemberData must reference a property with a getter",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X1021_MemberDataNonMethodShouldNotHaveParameters { get; } = new DiagnosticDescriptor("xUnit1021",
            "MemberData should not have parameters if the referenced member is not a method",
            "MemberData should not have parameters if the referenced member is not a method",
            Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: "Additional MemberData parameters are only used for methods. They are ignored for fields and properties.");

        internal static DiagnosticDescriptor X1022_TheoryMethodCannotHaveParameterArray { get; } = new DiagnosticDescriptor("xUnit1022",
            "Theory methods cannot have a parameter array",
            "Theory method '{0}' on test class '{1}' cannot have a parameter array '{2}'.",
            Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
            description: "Params array support was added in Xunit 2.2. Remove the parameter or upgrade the Xunit binaries.");

        internal static DiagnosticDescriptor X1023_TheoryMethodCannotHaveDefaultParameter { get; } = new DiagnosticDescriptor("xUnit1023",
           "Theory methods cannot have default parameter values",
           "Theory method '{0}' on test class '{1}' parameter '{2}' cannot have a default value.",
           Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
           description: "Default parameter values support was added in Xunit 2.2. Remove the default value or upgrade the Xunit binaries.");

        internal static DiagnosticDescriptor X1024_TestMethodCannotHaveOverloads { get; } = new DiagnosticDescriptor("xUnit1024",
           "Test methods cannot have overloads",
           "Test method '{0}' on test class '{1}' has the same name as another method declared on class '{2}'.",
           Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
           description: "Test method overloads are not supported as most test runners cannot correctly invoke the appropriate overload. " +
            "This includes any combination of static and instance methods declared with any visibility in the same class or across a " +
            "class hierarchy. Rename one of the methods.");

        internal static DiagnosticDescriptor X1025_InlineDataShouldBeUniqueWithinTheory { get; } = new DiagnosticDescriptor("xUnit1025",
            "InlineData should be unique within the Theory it belongs to",
            "Theory method '{0}' on test class '{1}' has InlineData duplicate(s).",
            Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: "Theory should have all InlineData elements unique. Remove redundant attribute(s) from the theory method.");

        internal static DiagnosticDescriptor X2000_AssertEqualLiteralValueShouldBeFirst { get; } = new DiagnosticDescriptor("xUnit2000",
            "Expected value should be first",
            "The literal or constant value {0} should be the first argument in the call to '{1}' in method '{2}' on type '{3}'.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: "The xUnit Assertion library produces the best error messages if the expected value is passed in as the first argument.");

        internal static DiagnosticDescriptor X2001_AssertEqualsShouldNotBeUsed { get; } = new DiagnosticDescriptor("xUnit2001",
            "Do not use invalid equality check",
            "Do not use {0}.",
            Categories.Assertions, DiagnosticSeverity.Hidden, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2002_AssertNullShouldNotBeCalledOnValueTypes { get; } = new DiagnosticDescriptor("xUnit2002",
            "Do not use null check on value type",
            "Do not use {0} on value type '{1}'.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2003_AssertEqualShouldNotUsedForNullCheck { get; } = new DiagnosticDescriptor("xUnit2003",
            "Do not use equality check to test for null value",
            "Do not use {0} to check for null value.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2004_AssertEqualShouldNotUsedForBoolLiteralCheck { get; } = new DiagnosticDescriptor("xUnit2004",
            "Do not use equality check to test for boolean conditions",
            "Do not use {0} to check for boolean conditions.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2005_AssertSameShouldNotBeCalledOnValueTypes { get; } = new DiagnosticDescriptor("xUnit2005",
            "Do not use identity check on value type",
            "Do not use {0} on value type '{1}'.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: "The value type will be boxed which means its identity will always be different.");

        internal static DiagnosticDescriptor X2006_AssertEqualGenericShouldNotBeUsedForStringValue { get; } = new DiagnosticDescriptor("xUnit2006",
            "Do not use invalid string equality check",
            "Do not use {0} to test for string equality.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2007_AssertIsTypeShouldUseGenericOverload { get; } = new DiagnosticDescriptor("xUnit2007",
            "Do not use typeof expression to check the type",
            "Do not use typeof({0}) expression to check the type.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck { get; } = new DiagnosticDescriptor("xUnit2008",
            "Do not use boolean check to match on regular expressions",
            "Do not use {0} to match on regular expressions.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2009_AssertSubstringCheckShouldNotUseBoolCheck { get; } = new DiagnosticDescriptor("xUnit2009",
            "Do not use boolean check to check for substrings",
            "Do not use {0} to check for substrings.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer { get; } = new DiagnosticDescriptor("xUnit2010",
            "Do not use boolean check to check for string equality",
            "Do not use {0} to check for string equality.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        internal static DiagnosticDescriptor X2011_AssertEmptyCollectionCheckShouldNotBeUsed { get; } = new DiagnosticDescriptor("xUnit2011",
            "Do not use empty collection check",
            "Do not use {0} to check for empty collections.",
            Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);
    }
}
