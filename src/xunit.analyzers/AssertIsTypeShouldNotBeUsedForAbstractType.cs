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
		private const string AbstractClass = "abstract class";
		private const string Interface = "interface";

		private static HashSet<string> IsTypeMethods { get; } = new HashSet<string>(new[] { "IsType", "IsNotType" });

		public AssertIsTypeShouldNotBeUsedForAbstractType()
			: base(Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType, IsTypeMethods)
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var type = invocationOperation.TargetMethod.TypeArguments.FirstOrDefault();
			var typeKind = GetAbstractTypeKind(type);
			if (typeKind == null)
				return;

			var typeName = SymbolDisplay.ToDisplayString(type);

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType,
					invocationOperation.Syntax.GetLocation(),
					typeKind,
					typeName));
		}

		private static string GetAbstractTypeKind(ITypeSymbol typeSymbol)
		{
			switch (typeSymbol.TypeKind)
			{
				case TypeKind.Class:
					if (typeSymbol.IsAbstract)
						return AbstractClass;
					break;

				case TypeKind.Interface:
					return Interface;
			}

			return null;
		}
	}
}
