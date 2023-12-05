using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.ConstructorsOnFactAttributeSubclassShouldBePublic>;

public class ConstructorsOnFactAttributeSubclassShouldBePublicFixerTests
{
	const string before = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute
{{
    {0} [|CustomFactAttribute|]()
    {{
        this.Skip = ""xxx"";
    }}
}}

public class Tests
{{
    [CustomFact]
    public void TestCustomFact()
    {{
    }}

    [Fact]
    public void TestFact()
    {{
    }}
}}";

	[Theory]
	[InlineData("internal")]
	[InlineData("protected internal")]
	public async void ChangeConstructorVisibilityParameters(string visibility)
	{
		await Verify.VerifyCodeFix(
			string.Format(before, visibility),
			string.Format(before, "public").Replace("[|", "").Replace("|]", ""),
			ConstructorsOnFactAttributeSubclassShouldBePublicFixer.Key_MakeConstructorPublic);
	}
}
