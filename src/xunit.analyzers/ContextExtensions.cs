using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers.Utilities;

namespace Xunit.Analyzers
{
    internal static class ContextExtensions
    {
        // The property SyntaxNodeAnalysisContext.Compilation was not added until 2.3.0.
        // Remove this extension method once we upgrade to that version of Roslyn.
        internal static Compilation Compilation(this SyntaxNodeAnalysisContext context)
        {
            return context.SemanticModel?.Compilation;
        }

        internal static RequireTypesContext RequireTypes(this AnalysisContext context, params string[] types)
        {
            return new RequireTypesContext(context, types);
        }
    }
}
