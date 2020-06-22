using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	internal static class OperationAnalysisContextExtensions
	{
		public static SemanticModel GetSemanticModel(this OperationAnalysisContext context)
		{
			return context.Compilation.GetSemanticModel(context.Operation.Syntax.SyntaxTree);
		}
	}
}
