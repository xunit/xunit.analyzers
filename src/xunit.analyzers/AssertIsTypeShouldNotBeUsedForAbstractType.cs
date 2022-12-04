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
		const string abstractClass = "abstract class";
		const string @interface = "interface";
		static readonly string[] targetMethods =
		{
			Constants.Asserts.IsType,
			Constants.Asserts.IsNotType
		};

		public AssertIsTypeShouldNotBeUsedForAbstractType()
			: base(Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType, targetMethods)
		{ }

		protected override void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			var type = invocationOperation.TargetMethod.TypeArguments.FirstOrDefault();
			var typeKind = GetAbstractTypeKind(type);
			if (typeKind is null)
				return;

			var typeName = SymbolDisplay.ToDisplayString(type);

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType,
					invocationOperation.Syntax.GetLocation(),
					typeKind,
					typeName
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
