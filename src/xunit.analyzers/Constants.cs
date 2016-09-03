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
        }

        internal static class Types
        {
            internal static readonly string XunitFactAttribute = "Xunit.FactAttribute";
            internal static readonly string XunitTheoryAttribute = "Xunit.TheoryAttribute";
        }
    }
}
