using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    internal static class Constants
    {
        static class Categories
        {
            internal static string Usage { get; } = "Usage";
            internal static string Extensibility { get; } = "Extensibility";
        }

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
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

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
        }

        internal static class Types
        {
            internal static readonly string XunitClassDataAttribute = "Xunit.ClassDataAttribute";
            internal static readonly string XunitFactAttribute = "Xunit.FactAttribute";
            internal static readonly string XunitTheoryAttribute = "Xunit.TheoryAttribute";

            internal static readonly string XunitSdkDataAttribute = "Xunit.Sdk.DataAttribute";
        }
    }
}
