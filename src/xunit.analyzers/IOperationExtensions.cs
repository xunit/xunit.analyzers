﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	internal static class IOperationExtensions
	{
		public static IOperation WalkDownImplicitConversions(this IOperation operation)
		{
			var current = operation;
			while (current is IConversionOperation conversion && conversion.IsImplicit)
			{
				current = conversion.Operand;
			}

			return current;
		}
	}
}
