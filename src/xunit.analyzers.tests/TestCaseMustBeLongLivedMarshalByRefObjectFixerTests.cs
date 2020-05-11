using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestCaseMustBeLongLivedMarshalByRefObject>;

namespace Xunit.Analyzers
{
	public class TestCaseMustBeLongLivedMarshalByRefObjectFixerTests
	{
		[Fact]
		public async void WithNoBaseClass_WithoutUsing_AddsBaseClass()
		{
			var source = "public class [|MyTestCase|] : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";
			var fixedSource = "public class MyTestCase : Xunit.LongLivedMarshalByRefObject, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";

			await new Verify.Test
			{
				TestState =
				{
					Sources = { source },
					AdditionalReferences = { CodeAnalyzerHelper.XunitExecutionReference },
				},
				FixedState = { Sources = { fixedSource } },
			}.RunAsync();
		}

		[Fact]
		public async void WithNoBaseClass_WithUsing_AddsBaseClass()
		{
			var source = "using Xunit; using Xunit.Abstractions; public class [|MyTestCase|] : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestCase|}|}|}|}|}|}|}|}|} { }";
			var fixedSource = "using Xunit; using Xunit.Abstractions; public class MyTestCase : LongLivedMarshalByRefObject, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestCase|}|}|}|}|}|}|}|}|} { }";

			await new Verify.Test
			{
				TestState =
				{
					Sources = { source },
					AdditionalReferences = { CodeAnalyzerHelper.XunitExecutionReference },
				},
				FixedState = { Sources = { fixedSource } },
			}.RunAsync();
		}

		[Fact]
		public async void WithBadBaseClass_WithoutUsing_ReplacesBaseClass()
		{
			var source = "public class Foo { } public class [|MyTestCase|] : Foo, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";
			var fixedSource = "public class Foo { } public class MyTestCase : Xunit.LongLivedMarshalByRefObject, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:Xunit.Abstractions.ITestCase|}|}|}|}|}|}|}|}|} { }";

			await new Verify.Test
			{
				TestState =
				{
					Sources = { source },
					AdditionalReferences = { CodeAnalyzerHelper.XunitExecutionReference },
				},
				FixedState = { Sources = { fixedSource } },
			}.RunAsync();
		}

		[Fact]
		public async void WithBadBaseClass_WithUsing_ReplacesBaseClass()
		{
			var source = "using Xunit; using Xunit.Abstractions; public class Foo { } public class [|MyTestCase|] : Foo, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestCase|}|}|}|}|}|}|}|}|} { }";
			var fixedSource = "using Xunit; using Xunit.Abstractions; public class Foo { } public class MyTestCase : LongLivedMarshalByRefObject, {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestCase|}|}|}|}|}|}|}|}|} { }";

			await new Verify.Test
			{
				TestState =
				{
					Sources = { source },
					AdditionalReferences = { CodeAnalyzerHelper.XunitExecutionReference },
				},
				FixedState = { Sources = { fixedSource } },
			}.RunAsync();
		}
	}
}
