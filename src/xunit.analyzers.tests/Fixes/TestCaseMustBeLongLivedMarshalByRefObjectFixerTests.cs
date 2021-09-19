using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestCaseMustBeLongLivedMarshalByRefObject>;

public class TestCaseMustBeLongLivedMarshalByRefObjectFixerTests
{
	[Fact]
	public async void WithNoBaseClass_WithoutUsing_AddsBaseClass()
	{
		var before = "public class [|MyTestCase|]: {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";
		var after = "public class MyTestCase: Xunit.LongLivedMarshalByRefObject, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void WithNoBaseClass_WithUsing_AddsBaseClass()
	{
		var before = @"
using Xunit;
using Xunit.Abstractions;

public class [|MyTestCase|]: {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestCase|}|}|}|}|}|}|}|}|} { }";

		var after = @"
using Xunit;
using Xunit.Abstractions;

public class MyTestCase: LongLivedMarshalByRefObject, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestCase|}|}|}|}|}|}|}|}|} { }";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void WithBadBaseClass_WithoutUsing_ReplacesBaseClass()
	{
		var before = "public class Foo { } public class [|MyTestCase|]: Foo, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";
		var after = "public class Foo { } public class MyTestCase: Xunit.LongLivedMarshalByRefObject, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void WithBadBaseClass_WithUsing_ReplacesBaseClass()
	{
		var before = @"
using Xunit;
using Xunit.Abstractions;

public class Foo { }

public class [|MyTestCase|]: Foo, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestCase|}|}|}|}|}|}|}|}|} { }";

		var after = @"
using Xunit;
using Xunit.Abstractions;

public class Foo { }

public class MyTestCase: LongLivedMarshalByRefObject, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestCase|}|}|}|}|}|}|}|}|} { }";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
