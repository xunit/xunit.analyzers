using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1053_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class Initializers_AndGetExpressions_DoesNotTrigger {
				public static TheoryData<int> Field = null;
				public static TheoryData<int> Property { get; } = null;
				public static TheoryData<int> PropertyWithGetBody { get { return null; } }
				public static TheoryData<int> PropertyWithGetExpression => null;
				public static TheoryData<int> FieldWrittenInStaticConstructor;
				public static TheoryData<int> PropertyWrittenInStaticConstructor { get; set; }

				static Initializers_AndGetExpressions_DoesNotTrigger()
				{
					Initializers_AndGetExpressions_DoesNotTrigger.FieldWrittenInStaticConstructor = null;
					PropertyWrittenInStaticConstructor = null;
				}

				[Theory]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Property))]
				[MemberData(nameof(PropertyWithGetBody))]
				[MemberData(nameof(PropertyWithGetExpression))]
				[MemberData(nameof(FieldWrittenInStaticConstructor))]
				[MemberData(nameof(PropertyWrittenInStaticConstructor))]
				public void TestCase(int _) {}
			}

			// Initializing to null is not sufficient to prevent the trigger

			public class SettingNull_Triggers {
				public static TheoryData<int> {|#0:Field|};
				public static TheoryData<int> {|#1:Property|} { get; set; }

				public SettingNull_Triggers()
				{
					SettingNull_Triggers.Field = null;
					Property = null;
				}

				[Theory]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Property))]
				public void TestCase(int _) {}
			}

			// We currently don't chase down the static => static call, so this is out of scope

			public class OutOfScopeCaseInitializer {
				public static void Initialize() {
					OutOfScopeCase_Triggers.Field = null;
					OutOfScopeCase_Triggers.Property = null;
				}
			}

			public class OutOfScopeCase_Triggers {
				public static TheoryData<int> {|#10:Field|};
				public static TheoryData<int> {|#11:Property|} { get; set; }

				static OutOfScopeCase_Triggers()
				{
					OutOfScopeCaseInitializer.Initialize();
				}

				[Theory]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Property))]
				public void TestCase(int _) {}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1053").WithLocation(0).WithArguments("Field"),
			Verify.Diagnostic("xUnit1053").WithLocation(1).WithArguments("Property"),

			Verify.Diagnostic("xUnit1053").WithLocation(10).WithArguments("Field"),
			Verify.Diagnostic("xUnit1053").WithLocation(11).WithArguments("Property"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
