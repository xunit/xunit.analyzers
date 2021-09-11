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
			var diagnostic = context.Diagnostics.First();

			var declaringType = semanticModel.Compilation.GetTypeByMetadataName(diagnostic.Properties["DeclaringType"]);
			if (declaringType == null)
				return;

			var member = declaringType.GetMembers(diagnostic.Properties["MemberName"]).FirstOrDefault();
			if (member == null)
				return;

			if (member.Locations.FirstOrDefault()?.IsInMetadata ?? true)
				return;

			await RegisterCodeFixesAsync(context, member).ConfigureAwait(false);
		}

		public abstract Task RegisterCodeFixesAsync(
			CodeFixContext context,
			ISymbol member);
	}
}
