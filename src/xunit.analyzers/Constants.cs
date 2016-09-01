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
                "Types with test methods must be public",
                "Make the type {0} public so that test methods on it can be discovered and executed",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);
        }
    }
}
