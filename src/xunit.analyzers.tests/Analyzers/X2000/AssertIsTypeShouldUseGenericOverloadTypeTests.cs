using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;

#if NETCOREAPP
using Microsoft.CodeAnalysis.CSharp;
#endif

public class AssertIsTypeShouldUseGenericOverloadTypeTests
{
	public static TheoryData<string> Methods =
	[
		"IsType",
		"IsNotType",
		"IsAssignableFrom",
	];

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForNonGenericCall_Triggers(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.{0}(typeof(int), 1)|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("int");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForGenericCall_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}<int>(1);
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("IsType")]
	[InlineData("IsNotType")]
	public async Task ForGenericCall_WithExactMatchFlag_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}<int>(1, false);
			        Xunit.Assert.{0}<System.Type>(typeof(int), true);
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

#if NETCOREAPP

	public class StaticAbstractInterfaceMethods
	{
#if ROSLYN_LATEST  // C# 11 is required for static abstract methods

		const string methodCode = /* lang=c#-test */ "static abstract void Method();";
		const string codeTemplate = /* lang=c#-test */ """
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
			}}
			""";

		[Fact]
		public async Task ForStaticAbstractInterfaceMembers_DoesNotTrigger()
		{
			string source = string.Format(codeTemplate, string.Empty, methodCode);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp11, source);
		}

		[Fact]
		public async Task ForNestedStaticAbstractInterfaceMembers_DoesNotTrigger()
		{
			string source = string.Format(codeTemplate, methodCode, string.Empty);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp11, source);
		}

#endif

		[Theory]
		[InlineData("static", "", "{ }")]
		[InlineData("", "abstract", ";")]
		public async Task ForNotStaticAbstractInterfaceMembers_Triggers(
			string staticModifier,
			string abstractModifier,
			string methodBody)
		{
			string source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public interface IClass {{
				    {0} {1} void Method() {2}
				}}

				public class Class : IClass {{
				    public {0} void Method() {{ }}
				}}

				public abstract class TestClass {{
				    [Fact]
				    public void TestMethod() {{
				        var data = new Class();

				        {{|#0:Assert.IsAssignableFrom(typeof(IClass), data)|}};
				    }}
				}}
				""", staticModifier, abstractModifier, methodBody);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments("IClass");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}
	}

#endif
}
