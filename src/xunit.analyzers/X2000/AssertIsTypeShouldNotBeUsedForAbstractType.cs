using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

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

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		var type = invocationOperation.TargetMethod.TypeArguments.FirstOrDefault();
		if (type is null)
			return;

		var typeKind = type.TypeKind switch
		{
			TypeKind.Class => type.IsAbstract ? abstractClass : null,
			TypeKind.Interface => @interface,
			_ => null,
		};

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
}
