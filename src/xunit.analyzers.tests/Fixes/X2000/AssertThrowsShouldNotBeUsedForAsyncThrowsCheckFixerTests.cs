using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixerTests
{
	public static TheoryData<string> Lambdas = new()
	{
		"(System.Func<System.Threading.Tasks.Task>)ThrowingMethod",
		"() => System.Threading.Tasks.Task.Delay(0)",
		"(System.Func<System.Threading.Tasks.Task>)(async () => await System.Threading.Tasks.Task.Delay(0))",
		"(System.Func<System.Threading.Tasks.Task>)(async () => await System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false))",
	};

	[Theory]
	[MemberData(nameof(Lambdas))]
	public async void WithNonArgumentException(string lambda)
	{
		var before = $@"
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    Task ThrowingMethod() {{
        throw new NotImplementedException();
    }}

    [Fact]
    public void TestMethod() {{
        {{|CS0619:[|Assert.Throws<Exception>({lambda})|]|}};
    }}
}}";

		var after = $@"
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    Task ThrowingMethod() {{
        throw new NotImplementedException();
    }}

    [Fact]
    public async Task TestMethod() {{
        await Assert.ThrowsAsync<Exception>({lambda});
    }}
}}";

		await Verify.VerifyCodeFix(before, after, AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer.Key_UseAlternateAssert);
	}

	[Theory]
	[MemberData(nameof(Lambdas))]
	public async void WithArgumentException(string lambda)
	{
		var before = $@"
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    Task ThrowingMethod() {{
        throw new NotImplementedException();
    }}

    [Fact]
    public void TestMethod() {{
        {{|CS0619:[|Assert.Throws<ArgumentException>(""param"", {lambda})|]|}};
    }}
}}";

		var after = $@"
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    Task ThrowingMethod() {{
        throw new NotImplementedException();
    }}

    [Fact]
    public async Task TestMethod() {{
        await Assert.ThrowsAsync<ArgumentException>(""param"", {lambda});
    }}
}}";

		await Verify.VerifyCodeFix(before, after, AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer.Key_UseAlternateAssert);
	}
}
