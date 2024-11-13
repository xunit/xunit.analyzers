using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Xunit.Analyzers.Fixes;

public abstract class BatchedMemberFixProvider(params string[] diagnostics) :
	BatchedCodeFixProvider(diagnostics)
{
	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.DeclaringType, out var declaringTypeName))
			return;
		if (declaringTypeName is null)
			return;

		if (!diagnostic.Properties.TryGetValue(Constants.Properties.MemberName, out var memberName))
			return;
		if (memberName is null)
			return;

		var declaringType = semanticModel.Compilation.GetTypeByMetadataName(declaringTypeName);
		if (declaringType is null)
			return;

		var member = declaringType.GetMembers(memberName).FirstOrDefault();
		if (member is null)
			return;

		if (member.Locations.FirstOrDefault()?.IsInMetadata ?? true)
			return;

		await RegisterCodeFixesAsync(context, member).ConfigureAwait(false);
	}

	public abstract Task RegisterCodeFixesAsync(
		CodeFixContext context,
		ISymbol member);
}
