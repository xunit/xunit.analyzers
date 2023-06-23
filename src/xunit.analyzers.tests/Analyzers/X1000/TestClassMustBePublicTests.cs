using System.Collections.Generic;
using System.Linq;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;

public class TestClassMustBePublicTests
{
	public static IEnumerable<object[]> CreateFactsInNonPublicClassCases =
		from attribute in new[] { "Xunit.Fact", "Xunit.Theory" }
		from modifier in new[] { "", "internal" }
		select new[] { attribute, modifier };

	[Fact]
	public async void ForPublicClass_DoesNotFindError()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[MemberData(nameof(CreateFactsInNonPublicClassCases))]
	public async void ForFriendOrInternalClass_FindsError(
		string attribute,
		string modifier)
	{
		var source = $@"
{modifier} class TestClass {{
    [{attribute}]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(2, 8 + modifier.Length, 2, 17 + modifier.Length);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[InlineData("")]
	[InlineData("public")]
	public async void ForPartialClassInSameFile_WhenClassIsPublic_DoesNotFindError(string modifier)
	{
		var source = $@"
public partial class TestClass {{
    [Xunit.Fact]
    public void Test1() {{ }}
}}

{modifier} partial class TestClass {{
    [Xunit.Fact]
    public void Test2() {{ }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[InlineData("")]
	[InlineData("public")]
	public async void ForPartialClassInOtherFiles_WhenClassIsPublic_DoesNotFindError(string modifier)
	{
		var source1 = @"
public partial class TestClass {
    [Xunit.Fact]
    public void Test1() { }
}";
		var source2 = $@"
{modifier} partial class TestClass {{
    [Xunit.Fact]
    public void Test2() {{ }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(new[] { source1, source2 });
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("", "internal")]
	[InlineData("internal", "internal")]
	public async void ForPartialClassInSameFile_WhenClassIsNonPublic_FindsError(
		string modifier1,
		string modifier2)
	{
		var source = $@"
{modifier1} partial class TestClass {{
    [Xunit.Fact]
    public void Test1() {{ }}
}}

{modifier2} partial class TestClass {{
    [Xunit.Fact]
    public void Test2() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(2, 16 + modifier1.Length, 2, 25 + modifier1.Length)
				.WithSpan(7, 16 + modifier2.Length, 7, 25 + modifier2.Length);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("", "internal")]
	[InlineData("internal", "internal")]
	public async void ForPartialClassInOtherFiles_WhenClassIsNonPublic_FindsError(
		string modifier1,
		string modifier2)
	{
		var source1 = $@"
{modifier1} partial class TestClass {{
    [Xunit.Fact]
    public void Test1() {{ }}
}}";
		var source2 = $@"
{modifier2} partial class TestClass {{
    [Xunit.Fact]
    public void Test2() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(2, 16 + modifier1.Length, 2, 25 + modifier1.Length)
				.WithSpan("/0/Test1.cs", 2, 16 + modifier2.Length, 2, 25 + modifier2.Length);

		await Verify.VerifyAnalyzerAsyncV2(new[] { source1, source2 }, expected);
	}
}
