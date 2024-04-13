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
		readonly static string Template = "using Xunit.Abstractions; public class {0}: {1} {{ }}";

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithAbstractions.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithAbstractions))]
		public async Task DoesNotAttemptToFix(string @interface)
		{
			var source = string.Format(Template, "[|MyClass|]", @interface);

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
		static string Template_WithoutUsing = @"
using Xunit.Abstractions;

public class Foo {{ }}
public class {0}: {1} {{ }}";
		static string Template_WithUsing = @"
using Xunit;
using Xunit.Abstractions;

public class Foo {{ }}
public class {0}: {1} {{ }}";

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithExecution))]
		public async Task WithNoBaseClass_WithoutUsing_AddsBaseClass(string @interface)
		{
			var before = string.Format(Template_WithoutUsing, "[|MyClass|]", @interface);
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
			var before = string.Format(Template_WithUsing, "[|MyClass|]", @interface);
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
			var before = string.Format(Template_WithoutUsing, "[|MyClass|]", $"Foo, {@interface}");
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
			var before = string.Format(Template_WithUsing, "[|MyClass|]", $"Foo, {@interface}");
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
		static string Template_WithoutUsing = @"
using Xunit.Abstractions;

public class Foo {{ }}
public class {0}: {1} {{ }}";
		static string Template_WithUsing = @"
using Xunit.Abstractions;
using Xunit.Sdk;

public class Foo {{ }}
public class {0}: {1} {{ }}";

		[Theory]
		[MemberData(nameof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility.Interfaces), MemberType = typeof(CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectTests.WithRunnerUtility))]
		public async Task WithNoBaseClass_WithoutUsing_AddsBaseClass(string @interface)
		{
			var before = string.Format(Template_WithoutUsing, "[|MyClass|]", @interface);
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
			var before = string.Format(Template_WithUsing, "[|MyClass|]", @interface);
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
			var before = string.Format(Template_WithoutUsing, "[|MyClass|]", $"Foo, {@interface}");
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
			var before = string.Format(Template_WithUsing, "[|MyClass|]", $"Foo, {@interface}");
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
