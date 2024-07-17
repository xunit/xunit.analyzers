using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;

public class TestClassMustBePublicTests
{
	public static MatrixTheoryData<string, string> CreateFactsInNonPublicClassCases =
		new(
			/* lang=c#-test */ ["Xunit.Fact", "Xunit.Theory"],
			/* lang=c#-test */ ["", "internal"]
		);

	[Fact]
	public async Task ForPublicClass_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
			    [Xunit.Fact]
			    public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(CreateFactsInNonPublicClassCases))]
	public async Task ForFriendOrInternalClass_Triggers(
		string attribute,
		string modifier)
	{
		var source = string.Format(/* lang=c#-test */ """
			{1} class [|TestClass|] {{
			    [{0}]
			    public void TestMethod() {{ }}
			}}
			""", attribute, modifier);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData(/* lang=c#-test */ "")]
	[InlineData(/* lang=c#-test */ "public")]
	public async Task ForPartialClassInSameFile_WhenClassIsPublic_DoesNotTrigger(string modifier)
	{
		var source = string.Format(/* lang=c#-test */ """
			public partial class TestClass {{
			    [Xunit.Fact]
			    public void Test1() {{ }}
			}}

			{0} partial class TestClass {{
			    [Xunit.Fact]
			    public void Test2() {{ }}
			}}
			""", modifier);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData(/* lang=c#-test */ "")]
	[InlineData(/* lang=c#-test */ "public")]
	public async Task ForPartialClassInOtherFiles_WhenClassIsPublic_DoesNotTrigger(string modifier)
	{
		var source1 = /* lang=c#-test */ """
			public partial class TestClass {
			    [Xunit.Fact]
			    public void Test1() { }
			}
			""";
		var source2 = string.Format(/* lang=c#-test */ """
			{0} partial class TestClass {{
			    [Xunit.Fact]
			    public void Test2() {{ }}
			}}
			""", modifier);

		await Verify.VerifyAnalyzer([source1, source2]);
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("", "internal")]
	[InlineData("internal", "internal")]
	public async Task ForPartialClassInSameFile_WhenClassIsNonPublic_Triggers(
		string modifier1,
		string modifier2)
	{
		var source = string.Format(/* lang=c#-test */ """
			{0} partial class {{|#0:TestClass|}} {{
			    [Xunit.Fact]
			    public void Test1() {{ }}
			}}

			{1} partial class {{|#1:TestClass|}} {{
			    [Xunit.Fact]
			    public void Test2() {{ }}
			}}
			""", modifier1, modifier2);
		var expected = Verify.Diagnostic().WithLocation(0).WithLocation(1);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData("", "")]
	[InlineData("", "internal")]
	[InlineData("internal", "internal")]
	public async Task ForPartialClassInOtherFiles_WhenClassIsNonPublic_Triggers(
		string modifier1,
		string modifier2)
	{
		var source1 = string.Format(/* lang=c#-test */ """
			{0} partial class {{|#0:TestClass|}} {{
			    [Xunit.Fact]
			    public void Test1() {{ }}
			}}
			""", modifier1);
		var source2 = string.Format(/* lang=c#-test */ """
			{0} partial class {{|#1:TestClass|}} {{
			    [Xunit.Fact]
			    public void Test2() {{ }}
			}}
			""", modifier2);
		var expected = Verify.Diagnostic().WithLocation(0).WithLocation(1);

		await Verify.VerifyAnalyzer([source1, source2], expected);
	}
}
