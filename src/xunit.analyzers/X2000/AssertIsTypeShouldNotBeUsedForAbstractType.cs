using System.Collections.Generic;
using System.Collections.Immutable;
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
		Guard.ArgumentNotNull(xunitContext);
		Guard.ArgumentNotNull(invocationOperation);
		Guard.ArgumentNotNull(method);

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

		if (invocationOperation.Arguments.Length > 1)
		{
			if (invocationOperation.Arguments[1].Value is not ILiteralOperation operation)
				return;
			if (operation.ConstantValue.Value is not bool value)
				return;
			if (value != true)
				return;
		}

		var typeName = SymbolDisplay.ToDisplayString(type);
		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		string? replacement;

		if (xunitContext.Assert.SupportsInexactTypeAssertions)
		{
			replacement = "exactMatch: false";
			builder[Constants.Properties.UseExactMatch] = bool.TrueString;
		}
		else
		{
			if (!ReplacementMethods.TryGetValue(invocationOperation.TargetMethod.Name, out replacement))
				return;

			builder[Constants.Properties.UseExactMatch] = bool.FalseString;
			replacement = "Assert." + replacement;
		}

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				typeKind,
				typeName,
				replacement
			)
		);
	}
}
