using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	public static class IOperationExtensions
	{
		public static IOperation WalkDownImplicitConversions(this IOperation operation)
		{
			var current = operation;
			while (current is IConversionOperation conversion && conversion.Conversion.IsImplicit)
				current = conversion.Operand;

			return current;
		}
	}
}
