using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Xunit.Analyzers.FixProviders
{
	public abstract class MemberFixBase : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public MemberFixBase(IEnumerable<string> diagnostics) =>
			FixableDiagnosticIds = diagnostics.ToImmutableArray();

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			var diagnostic = context.Diagnostics.FirstOrDefault();
			if (diagnostic is null)
				return;
			if (!diagnostic.Properties.TryGetValue(Constants.Properties.DeclaringType, out var declaringTypeName))
				return;
			if (!diagnostic.Properties.TryGetValue(Constants.Properties.MemberName, out var memberName))
				return;

			var declaringType = semanticModel.Compilation.GetTypeByMetadataName(declaringTypeName);
			if (declaringType is null)
				return;

			var member = declaringType.GetMembers(memberName).FirstOrDefault();
			if (member is null)
				return;

			if (member.Locations.FirstOrDefault()?.IsInMetadata ?? true)
				return;

			await RegisterCodeFixesAsync(context, member);
		}

		public abstract Task RegisterCodeFixesAsync(
			CodeFixContext context,
			ISymbol member);
	}
}
