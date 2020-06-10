using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TestCaseMustBeLongLivedMarshalByRefObject : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext context, XunitContext xunitContext)
		{
			context.RegisterSyntaxNodeAction(context =>
			{
				var classDeclaration = (ClassDeclarationSyntax)context.Node;
				if (classDeclaration.BaseList == null)
					return;

				var semanticModel = context.SemanticModel;
				var isTestCase = false;
				var hasMBRO = false;

				foreach (var baseType in classDeclaration.BaseList.Types)
				{
					var type = semanticModel.GetTypeInfo(baseType.Type, context.CancellationToken).Type;
					if (xunitContext.Abstractions.ITestCaseType?.IsAssignableFrom(type) == true)
						isTestCase = true;
					if (xunitContext.Execution.LongLivedMarshalByRefObjectType?.IsAssignableFrom(type) == true)
						hasMBRO = true;
				}

				if (isTestCase && !hasMBRO)
				{
					context.ReportDiagnostic(Diagnostic.Create(
						Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject,
						classDeclaration.Identifier.GetLocation(),
						classDeclaration.Identifier.ValueText));
				}
			}, SyntaxKind.ClassDeclaration);
		}

		protected override bool ShouldAnalyze(XunitContext xunitContext)
			=> xunitContext.HasAbstractionsReference && xunitContext.HasExecutionReference;
	}
}
