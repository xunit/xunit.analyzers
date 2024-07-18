#if NETCOREAPP

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;

public class AssertIsTypeShouldUseGenericOverloadTypeTests
{
	public class StaticAbstractInterfaceMethods
	{
#if ROSLYN_4_4_OR_GREATER  // C# 11 is required for static abstract methods

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
}

#endif
