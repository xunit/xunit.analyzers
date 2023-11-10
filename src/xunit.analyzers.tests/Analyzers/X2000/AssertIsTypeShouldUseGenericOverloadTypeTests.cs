using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;

public class AssertIsTypeShouldUseGenericOverloadTypeTests
{
	const string methodCode = "static abstract void Method();";
	const string codeTemplate = @"
using Xunit;

public interface IParentClass  {{
	{0}
}}

public interface IClass : IParentClass {{
    {1}
}}

public class Class : IClass {{
    public static void Method() {{ }}
}}

public abstract class TestClass {{
    [Fact]
    public void TestMethod() {{
        var data = new Class();

        Assert.IsAssignableFrom(typeof(IClass), data);
    }}
}}";

	[Fact]
	public async void DoesNotFindWarning_ForStaticAbstractInterfaceMembers()
	{
		string source = string.Format(codeTemplate, string.Empty, methodCode);

#if NETFRAMEWORK
		var expected = new[]
		{
			Verify
				.CompilerError("CS8703")
				.WithSpan(9, 26, 9, 32)
				.WithArguments("abstract", "6", "preview"),

			Verify
				.CompilerError("CS8919")
				.WithSpan(9, 26, 9, 32),

			Verify
				.CompilerError("CS8929")
				.WithSpan(12, 22, 12, 28)
				.WithArguments("Class.Method()", "IClass.Method()", "Class")
		};
		await Verify.VerifyAnalyzer(source, expected);
#else
		await Verify.VerifyAnalyzer(LanguageVersion.Preview, source);
#endif
	}

	[Fact]
	public async void DoesNotFindWarning_ForNestedStaticAbstractInterfaceMembers()
	{
		string source = string.Format(codeTemplate, methodCode, string.Empty);

#if NETFRAMEWORK
		var expected = new[]
		{
			Verify
				.CompilerError("CS8703")
				.WithSpan(5, 23, 5, 29)
				.WithArguments("abstract", "6", "preview"),

			Verify
				.CompilerError("CS8919")
				.WithSpan(5, 23, 5, 29),

			Verify
				.CompilerError("CS8929")
				.WithSpan(12, 22, 12, 28)
				.WithArguments("Class.Method()", "IParentClass.Method()", "Class")
		};
		await Verify.VerifyAnalyzer(source, expected);
#else
		await Verify.VerifyAnalyzer(LanguageVersion.Preview, source);
#endif
	}

	[Theory]
	[InlineData("static", "", "{ }")]
	[InlineData("", "abstract", ";")]
	public async void FindsWarning_ForNotStaticAbstractInterfaceMembers(string staticModifier, string abstractModifier, string methodBody)
	{
		string source = $@"
using Xunit;

public interface IClass {{
    {staticModifier} {abstractModifier} void Method() {methodBody}
}}

public class Class : IClass {{
    public {staticModifier} void Method() {{ }}
}}

public abstract class TestClass {{
    [Fact]
    public void TestMethod() {{
        var data = new Class();

        Assert.IsAssignableFrom(typeof(IClass), data);
    }}
}}";

		var expected = new List<DiagnosticResult>()
		{
			Verify
				.Diagnostic()
				.WithSpan(17, 9, 17, 54)
				.WithArguments("IClass")
		};

#if NETFRAMEWORK
		if (!string.IsNullOrWhiteSpace(staticModifier))
		{
			expected.Add(
				Verify
					.CompilerError("CS8701")
					.WithSpan(5, 18, 5, 24));
		}
#endif

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected.ToArray());
	}
}
