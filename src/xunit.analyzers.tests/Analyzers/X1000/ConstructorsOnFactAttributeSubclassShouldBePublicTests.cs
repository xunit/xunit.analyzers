using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ConstructorsOnFactAttributeSubclassShouldBePublic>;

public class ConstructorsOnFactAttributeSubclassShouldBePublicTests
{
	[Fact]
	public async void DoesNotFindError_ForPublicConstructor_InFactAttributeSubclass()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute
{
    public CustomFactAttribute()
    {
        this.Skip = ""xxx"";
    }
}

public class Tests
{
    [CustomFact]
    public void TestCustomFact()
    {
    }

    [Fact]
    public void TestFact()
    {
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindError_ForPublicConstructorWithArguments_InFactAttributeSubclass()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute
{
    public CustomFactAttribute(string blah)
    {
        this.Skip = blah;
    }
}

public class Tests
{
    [CustomFact(""blah"")]
    public void TestCustomFact()
    {
    }

    [Fact]
    public void TestFact()
    {
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindError_ForPublicConstructorWithOtherConstructors_InFactAttributeSubclass()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute
{
    public CustomFactAttribute()
    {
        this.Skip = ""xxx"";
    }

    internal CustomFactAttribute(string blah)
    {
        this.Skip = blah;
    }
}

public class Tests
{
    [CustomFact]
    public void TestCustomFact()
    {
    }

    [Fact]
    public void TestFact()
    {
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindError_ForDefaultConstructor_InFactAttributeSubclass()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute
{
}

public class Tests
{
    [CustomFact]
    public void TestCustomFact()
    {
    }

    [Fact]
    public void TestFact()
    {
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void FindsError_ForInternalConstructor_InFactAttributeSubclass()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute
{
    internal CustomFactAttribute()
    {
        this.Skip = ""xxx"";
    }
}

public class Tests
{
    [CustomFact]
    public void TestCustomFact()
    {
    }

    [Fact]
    public void TestFact()
    {
    }
}";

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1043")
				.WithSeverity(DiagnosticSeverity.Error)
				.WithSpan(16, 6, 16, 16)
				.WithArguments("CustomFactAttribute")
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindsError_ForProtectedInternalConstructor_InFactAttributeSubclass()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute
{
    protected internal CustomFactAttribute()
    {
        this.Skip = ""xxx"";
    }
}

public class Tests
{
    [CustomFact]
    public void TestCustomFact()
    {
    }

    [Fact]
    public void TestFact()
    {
    }
}";

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1043")
				.WithSeverity(DiagnosticSeverity.Error)
				.WithSpan(16, 6, 16, 16)
				.WithArguments("CustomFactAttribute")
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
