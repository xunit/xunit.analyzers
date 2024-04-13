using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ConstructorsOnFactAttributeSubclassShouldBePublic>;

public class ConstructorsOnFactAttributeSubclassShouldBePublicTests
{
	[Fact]
	public async Task DefaultConstructor_DoesNotTrigger()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute { }

public class Tests {
    [CustomFact]
    public void TestCustomFact() { }

    [Fact]
    public void TestFact() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ParameterlessPublicConstructor_DoesNotTrigger()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute {
    public CustomFactAttribute() {
        this.Skip = ""xxx"";
    }
}

public class Tests {
    [CustomFact]
    public void TestCustomFact() { }

    [Fact]
    public void TestFact() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task PublicConstructorWithParameters_DoesNotTrigger()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute {
    public CustomFactAttribute(string skip) {
        this.Skip = skip;
    }
}

public class Tests {
    [CustomFact(""blah"")]
    public void TestCustomFact() { }

    [Fact]
    public void TestFact() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task PublicConstructorWithOtherConstructors_DoesNotTrigger()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute {
    public CustomFactAttribute() {
        this.Skip = ""xxx"";
    }

    internal CustomFactAttribute(string skip) {
        this.Skip = skip;
    }
}

public class Tests {
    [CustomFact]
    public void TestCustomFact() { }

    [Fact]
    public void TestFact() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task InternalConstructor_Triggers()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute {
    internal CustomFactAttribute(string skip, params int[] values) { }
}

public class Tests {
    [CustomFact(""Skip"", 42)]
    public void TestCustomFact() { }

    [Fact]
    public void TestFact() { }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSeverity(DiagnosticSeverity.Error)
				.WithSpan(11, 6, 11, 28)
				.WithArguments("CustomFactAttribute.CustomFactAttribute(string, params int[])");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ProtectedInternalConstructor_Triggers()
	{
		var source = @"
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class CustomFactAttribute : FactAttribute {
    protected internal CustomFactAttribute() {
        this.Skip = ""xxx"";
    }
}

public class Tests {
    [CustomFact]
    public void TestCustomFact() { }

    [Fact]
    public void TestFact() { }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSeverity(DiagnosticSeverity.Error)
				.WithSpan(13, 6, 13, 16)
				.WithArguments("CustomFactAttribute.CustomFactAttribute()");

		await Verify.VerifyAnalyzer(source, expected);
	}
}
