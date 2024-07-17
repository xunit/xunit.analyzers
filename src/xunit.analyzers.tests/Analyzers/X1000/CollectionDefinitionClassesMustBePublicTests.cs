using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionClassesMustBePublic>;

public class CollectionDefinitionClassesMustBePublicTests
{
	[Fact]
	public async Task ForPublicClass_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			public class CollectionDefinitionClass { }
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("")]
	[InlineData("internal ")]
	public async Task ForFriendOrInternalClass_Triggers(string classAccessModifier)
	{
		var source = string.Format(/* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			{0}class [|CollectionDefinitionClass|] {{ }}
			""", classAccessModifier);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("")]
	[InlineData("public ")]
	public async Task ForPartialClassInSameFile_WhenClassIsPublic_DoesNotTrigger(string otherPartAccessModifier)
	{
		var source = string.Format(/* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			public partial class CollectionDefinitionClass {{ }}
			{0}partial class CollectionDefinitionClass {{ }}
			""", otherPartAccessModifier);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("", "internal ")]
	[InlineData("internal ", "internal ")]
	public async Task ForPartialClassInSameFile_WhenClassIsNonPublic_Triggers(
		string part1AccessModifier,
		string part2AccessModifier)
	{
		var source = string.Format(/* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			{0}partial class {{|#0:CollectionDefinitionClass|}} {{ }}
			{1}partial class {{|#1:CollectionDefinitionClass|}} {{ }}
			""", part1AccessModifier, part2AccessModifier);
		var expected = Verify.Diagnostic().WithLocation(0).WithLocation(1);

		await Verify.VerifyAnalyzer(source, expected);
	}
}
