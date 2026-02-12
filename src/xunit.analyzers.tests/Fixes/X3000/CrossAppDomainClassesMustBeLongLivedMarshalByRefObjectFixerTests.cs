using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify_WithAbstractions = CSharpVerifier<CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests.WithAbstractions.Analyzer>;
using Verify_WithExecution = CSharpVerifier<CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests.WithExecution.Analyzer>;
using Verify_WithRunnerUtility = CSharpVerifier<CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests.WithRunnerUtility.Analyzer>;

public class CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests
{
	public class WithAbstractions : CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixerTests
	{
		const string Template = /* lang=c#-test */ "using Xunit.Abstractions; public class {0}: {1} {{ }}";

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithAbstractions.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithAbstractions))]
		public async Task DoesNotAttemptToFix(string @interface)
		{
			var source = string.Format(Template, /* lang=c#-test */ "[|MyClass|]", @interface);

			await Verify_WithAbstractions.VerifyCodeFixV2(
				source,
				source,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		internal class Analyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
		{
			protected override XunitContext CreateXunitContext(Compilation compilation) =>
				XunitContext.ForV2Abstractions(compilation);
		}
	}

	public class WithExecution
	{
		const string Template_WithoutUsing = /* lang=c#-test */ """
			using Xunit.Abstractions;

			public class Foo {{ }}
			public class {0}: {1} {{ }}
			""";
		const string Template_WithUsing = /* lang=c#-test */ """
			using Xunit;
			using Xunit.Abstractions;

			public class Foo {{ }}
			public class {0}: {1} {{ }}
			""";

		[Fact]
		public async Task FixAll_AddsBaseClassToMultipleClasses()
		{
			var before = /* lang=c#-test */ """
				using Xunit;
				using Xunit.Abstractions;

				public class [|MyClass1|]: IMessageSink { }
				public class [|MyClass2|]: IMessageSink { }
				""";
			var after = /* lang=c#-test */ """
				using Xunit;
				using Xunit.Abstractions;

				public class MyClass1: LongLivedMarshalByRefObject, IMessageSink { }
				public class MyClass2: LongLivedMarshalByRefObject, IMessageSink { }
				""";

			await Verify_WithExecution.VerifyCodeFixV2FixAll(before, after, CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType);
		}

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution))]
		public async Task WithNoBaseClass_WithoutUsing_AddsBaseClass(string @interface)
		{
			var before = string.Format(Template_WithoutUsing, /* lang=c#-test */ "[|MyClass|]", @interface);
			var after = string.Format(Template_WithoutUsing, "MyClass", $"Xunit.LongLivedMarshalByRefObject, {@interface}");

			await Verify_WithExecution.VerifyCodeFixV2(
				before,
				after,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution))]
		public async Task WithNoBaseClass_WithUsing_AddsBaseClass(string @interface)
		{
			var before = string.Format(Template_WithUsing, /* lang=c#-test */ "[|MyClass|]", @interface);
			var after = string.Format(Template_WithUsing, "MyClass", $"LongLivedMarshalByRefObject, {@interface}");

			await Verify_WithExecution.VerifyCodeFixV2(
				before,
				after,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution))]
		public async Task WithBadBaseClass_WithoutUsing_ReplacesBaseClass(string @interface)
		{
			var before = string.Format(Template_WithoutUsing, /* lang=c#-test */ "[|MyClass|]", $"Foo, {@interface}");
			var after = string.Format(Template_WithoutUsing, "MyClass", $"Xunit.LongLivedMarshalByRefObject, {@interface}");

			await Verify_WithExecution.VerifyCodeFixV2(
				before,
				after,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution))]
		public async Task WithBadBaseClass_WithUsing_ReplacesBaseClass(string @interface)
		{
			var before = string.Format(Template_WithUsing, /* lang=c#-test */ "[|MyClass|]", $"Foo, {@interface}");
			var after = string.Format(Template_WithUsing, "MyClass", $"LongLivedMarshalByRefObject, {@interface}");

			await Verify_WithExecution.VerifyCodeFixV2(
				before,
				after,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		internal class Analyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
		{
			protected override XunitContext CreateXunitContext(Compilation compilation) =>
				XunitContext.ForV2Execution(compilation);
		}
	}

	public class WithRunnerUtility
	{
		const string Template_WithoutUsing = /* lang=c#-test */ """
			using Xunit.Abstractions;

			public class Foo {{ }}
			public class {0}: {1} {{ }}
			""";
		const string Template_WithUsing = /* lang=c#-test */ """
			using Xunit.Abstractions;
			using Xunit.Sdk;

			public class Foo {{ }}
			public class {0}: {1} {{ }}
			""";

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility))]
		public async Task WithNoBaseClass_WithoutUsing_AddsBaseClass(string @interface)
		{
			var before = string.Format(Template_WithoutUsing, /* lang=c#-test */ "[|MyClass|]", @interface);
			var after = string.Format(Template_WithoutUsing, "MyClass", $"Xunit.Sdk.LongLivedMarshalByRefObject, {@interface}");

			await Verify_WithRunnerUtility.VerifyCodeFixV2RunnerUtility(
				before,
				after,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility))]
		public async Task WithNoBaseClass_WithUsing_AddsBaseClass(string @interface)
		{
			var before = string.Format(Template_WithUsing, /* lang=c#-test */ "[|MyClass|]", @interface);
			var after = string.Format(Template_WithUsing, "MyClass", $"LongLivedMarshalByRefObject, {@interface}");

			await Verify_WithRunnerUtility.VerifyCodeFixV2RunnerUtility(
				before,
				after,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility))]
		public async Task WithBadBaseClass_WithoutUsing_ReplacesBaseClass(string @interface)
		{
			var before = string.Format(Template_WithoutUsing, /* lang=c#-test */ "[|MyClass|]", $"Foo, {@interface}");
			var after = string.Format(Template_WithoutUsing, "MyClass", $"Xunit.Sdk.LongLivedMarshalByRefObject, {@interface}");

			await Verify_WithRunnerUtility.VerifyCodeFixV2RunnerUtility(
				before,
				after,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility))]
		public async Task WithBadBaseClass_WithUsing_ReplacesBaseClass(string @interface)
		{
			var before = string.Format(Template_WithUsing, /* lang=c#-test */ "[|MyClass|]", $"Foo, {@interface}");
			var after = string.Format(Template_WithUsing, "MyClass", $"LongLivedMarshalByRefObject, {@interface}");

			await Verify_WithRunnerUtility.VerifyCodeFixV2RunnerUtility(
				before,
				after,
				CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer.Key_SetBaseType
			);
		}

		internal class Analyzer : CrossAppDomainClassesMustBeLongLivedMarshalByRefObject
		{
			protected override XunitContext CreateXunitContext(Compilation compilation) =>
				XunitContext.ForV2RunnerUtility(compilation);
		}
	}
}
