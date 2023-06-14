using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertIsTypeShouldNotBeUsedForAbstractType : AssertUsageAnalyzerBase
	{
		public static readonly Dictionary<string, string> ReplacementMethods = new()
		{
			{ Constants.Asserts.IsType, Constants.Asserts.IsAssignableFrom },
			{ Constants.Asserts.IsNotType, Constants.Asserts.IsNotAssignableFrom },
		};

		const string abstractClass = "abstract class";
		const string @interface = "interface";

		public AssertIsTypeShouldNotBeUsedForAbstractType()
			: base(Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType, ReplacementMethods.Keys)
		{ }

		protected override void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			var type = invocationOperation.TargetMethod.TypeArguments.FirstOrDefault();
			if (type is null)
				return;

			var typeKind = GetAbstractTypeKind(type);
			if (typeKind is null)
				return;

			var typeName = SymbolDisplay.ToDisplayString(type);

			if (!ReplacementMethods.TryGetValue(invocationOperation.TargetMethod.Name, out var replacement))
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType,
					invocationOperation.Syntax.GetLocation(),
					typeKind,
					typeName,
					replacement
				)
			);
		}

		static string? GetAbstractTypeKind(ITypeSymbol? typeSymbol)
		{
			switch (typeSymbol?.TypeKind)
			{
				case TypeKind.Class:
					if (typeSymbol.IsAbstract)
						return abstractClass;
					break;

				case TypeKind.Interface:
					return @interface;
			}

			return null;
		}
	}
}
