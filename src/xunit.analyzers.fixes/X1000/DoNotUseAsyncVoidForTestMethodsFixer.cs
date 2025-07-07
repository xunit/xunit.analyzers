using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class DoNotUseAsyncVoidForTestMethodsFixer : XunitMemberFixProvider
{
	public const string Key_ConvertToTask = "xUnit1048_xUnit1049_ConvertToTask";
	public const string Key_ConvertToValueTask = "xUnit1049_ConvertToValueTask";

	public DoNotUseAsyncVoidForTestMethodsFixer() :
		base(
			Descriptors.X1048_DoNotUseAsyncVoidForTestMethods_V2.Id,
			Descriptors.X1049_DoNotUseAsyncVoidForTestMethods_V3.Id
		)
	{ }

	public override async Task RegisterCodeFixesAsync(
		CodeFixContext context,
		ISymbol member)
	{
		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return;

		var taskReturnType = TypeSymbolFactory.Task(semanticModel.Compilation);
		if (taskReturnType is null)
			return;

		var valueTaskReturnType = TypeSymbolFactory.ValueTask(semanticModel.Compilation);

		foreach (var diagnostic in context.Diagnostics)
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					"Change return type to Task",
					ct => context.Document.Project.Solution.ChangeMemberType(member, taskReturnType, ct),
					Key_ConvertToTask
				),
				diagnostic
			);

			if (valueTaskReturnType is not null && diagnostic.Id == Descriptors.X1049_DoNotUseAsyncVoidForTestMethods_V3.Id)
				context.RegisterCodeFix(
					CodeAction.Create(
						"Change return type to ValueTask",
						ct => context.Document.Project.Solution.ChangeMemberType(member, valueTaskReturnType, ct),
						Key_ConvertToValueTask
					),
					diagnostic
				);
		}
	}
}
