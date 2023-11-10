using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify_X2007 = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;
using Verify_X2015 = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldUseGenericOverloadCheck>;

public class UseGenericOverloadFixTests
{
	[Fact]
	public async void X2007_SwitchesToGenericIsType()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var result = 123;

        [|Assert.IsType(typeof(int), result)|];
    }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var result = 123;

        Assert.IsType<int>(result);
    }
}";

		await Verify_X2007.VerifyCodeFix(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}

	[Fact]
	public async void X2007_DoesNotFindErrorForStaticAbstractInterfaceMembers()
	{
		const string source = @"
using Xunit;

public interface IClass {
    static abstract void Method();
}

public class Class : IClass {
    public static void Method() { }
}

public abstract class TestClass {
    [Fact]
    public void TestMethod() {
        var data = new Class();

        Assert.IsAssignableFrom(typeof(IClass), data);
    }
}";

#if NETFRAMEWORK
		var expected = new[]
		{
			Verify_X2007
				.CompilerError("CS8703")
				.WithSpan(5, 26, 5, 32)
				.WithArguments("abstract", "6", "preview"),

			Verify_X2007
				.CompilerError("CS8919")
				.WithSpan(5, 26, 5, 32),

			Verify_X2007
				.CompilerError("CS8929")
				.WithSpan(8, 22, 8, 28)
				.WithArguments("Class.Method()", "IClass.Method()", "Class")
		};
		await Verify_X2007.VerifyAnalyzer(source, expected);
#else
		await Verify_X2007.VerifyAnalyzer(LanguageVersion.Preview, source);
#endif
	}

	[Theory]
	[InlineData("static", "", "{ }")]
	[InlineData("", "abstract", ";")]
	public async void X2007_FindsErrorForNotStaticAbstractInterfaceMembers(string staticModifier, string abstractModifier, string methodBody)
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
			Verify_X2007
				.Diagnostic()
				.WithSpan(17, 9, 17, 54)
				.WithArguments("IClass")
		};

#if NETFRAMEWORK
		if (!string.IsNullOrWhiteSpace(staticModifier))
		{
			expected.Add(
				Verify_X2007
					.CompilerError("CS8701")
					.WithSpan(5, 18, 5, 24));
		}
#endif

		await Verify_X2007.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected.ToArray());
	}

	[Fact]
	public async void X2015_SwitchesToGenericThrows()
	{
		var before = @"
using System;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Action func = () => { };

        [|Assert.Throws(typeof(DivideByZeroException), func)|];
    }
}";

		var after = @"
using System;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Action func = () => { };

        Assert.Throws<DivideByZeroException>(func);
    }
}";

		await Verify_X2015.VerifyCodeFix(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}
}
