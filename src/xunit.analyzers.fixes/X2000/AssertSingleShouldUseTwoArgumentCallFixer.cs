using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Composition;
using System.Threading.Tasks;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertSingleShouldUseTwoArgumentCallFixer : BatchedCodeFixProvider
{
	public AssertSingleShouldUseTwoArgumentCallFixer() :
		base(Descriptors.X2031_AssertSingleShouldUseTwoArgumentCall.Id)
	{ }

	public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		return Task.CompletedTask;
	}
}
