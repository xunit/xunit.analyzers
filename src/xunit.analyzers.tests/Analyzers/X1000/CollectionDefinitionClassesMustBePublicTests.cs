using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionClassesMustBePublic>;

public class CollectionDefinitionClassesMustBePublicTests
{
	[Fact]
	public async Task ForPublicClass_DoesNotFindError()
	{
		var source = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public class CollectionDefinitionClass { }";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("")]
	[InlineData("internal ")]
	public async Task ForFriendOrInternalClass_FindsError(string classAccessModifier)
	{
		var source = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
{classAccessModifier}class CollectionDefinitionClass {{ }}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(3, 7 + classAccessModifier.Length, 3, 32 + classAccessModifier.Length);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData("")]
	[InlineData("public ")]
	public async Task ForPartialClassInSameFile_WhenClassIsPublic_DoesNotFindError(string otherPartAccessModifier)
	{
		var source = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
public partial class CollectionDefinitionClass {{ }}
{otherPartAccessModifier}partial class CollectionDefinitionClass {{ }}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("", "internal ")]
	[InlineData("internal ", "internal ")]
	public async Task ForPartialClassInSameFile_WhenClassIsNonPublic_FindsError(
		string part1AccessModifier,
		string part2AccessModifier)
	{
		var source = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
{part1AccessModifier}partial class CollectionDefinitionClass {{ }}
{part2AccessModifier}partial class CollectionDefinitionClass {{ }}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(3, 15 + part1AccessModifier.Length, 3, 40 + part1AccessModifier.Length)
				.WithSpan(4, 15 + part2AccessModifier.Length, 4, 40 + part2AccessModifier.Length);

		await Verify.VerifyAnalyzer(source, expected);
	}
}
