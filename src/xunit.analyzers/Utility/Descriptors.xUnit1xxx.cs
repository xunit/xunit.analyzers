using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static Xunit.Analyzers.Category;

namespace Xunit.Analyzers;

public static partial class Descriptors
{
	public static DiagnosticDescriptor X1000_TestClassMustBePublic { get; } =
		Diagnostic(
			"xUnit1000",
			"Test classes must be public",
			Usage,
			Error,
			"Test classes must be public. Add or change the visibility modifier of the test class to public."
		);

	public static DiagnosticDescriptor X1001_FactMethodMustNotHaveParameters { get; } =
		Diagnostic(
			"xUnit1001",
			"Fact methods cannot have parameters",
			Usage,
			Error,
			"Fact methods cannot have parameters. Remove the parameters from the method or convert it into a Theory."
		);

	public static DiagnosticDescriptor X1002_TestMethodMustNotHaveMultipleFactAttributes { get; } =
		Diagnostic(
			"xUnit1002",
			"Test methods cannot have multiple Fact or Theory attributes",
			Usage,
			Error,
			"Test methods cannot have multiple Fact or Theory attributes. Remove all but one of the attributes."
		);

	public static DiagnosticDescriptor X1003_TheoryMethodMustHaveTestData { get; } =
		Diagnostic(
			"xUnit1003",
			"Theory methods must have test data",
			Usage,
			Error,
			"Theory methods must have test data. Use InlineData, MemberData, or ClassData to provide test data for the Theory."
		);

	public static DiagnosticDescriptor X1004_TestMethodShouldNotBeSkipped { get; } =
		Diagnostic(
			"xUnit1004",
			"Test methods should not be skipped",
			Usage,
			Info,
			"Test methods should not be skipped. Remove the Skip property to start running the test again."
		);

	public static DiagnosticDescriptor X1005_FactMethodShouldNotHaveTestData { get; } =
		Diagnostic(
			"xUnit1005",
			"Fact methods should not have test data",
			Usage,
			Warning,
			"Fact methods should not have test data. Remove the test data, or convert the Fact to a Theory."
		);

	public static DiagnosticDescriptor X1006_TheoryMethodShouldHaveParameters { get; } =
		Diagnostic(
			"xUnit1006",
			"Theory methods should have parameters",
			Usage,
			Warning,
			"Theory methods should have parameters. Add parameter(s) to the theory method."
		);

	public static DiagnosticDescriptor X1007_ClassDataAttributeMustPointAtValidClass { get; } =
		Diagnostic(
			"xUnit1007",
			"ClassData must point at a valid class",
			Usage,
			Error,
			"ClassData must point at a valid class. The class {0} must be public, not sealed, with an empty constructor, and implement IEnumerable<object[]>."
		);

	public static DiagnosticDescriptor X1008_DataAttributeShouldBeUsedOnATheory { get; } =
		Diagnostic(
			"xUnit1008",
			"Test data attribute should only be used on a Theory",
			Usage,
			Warning,
			"Test data attribute should only be used on a Theory. Remove the test data, or add the Theory attribute to the test method."
		);

	public static DiagnosticDescriptor X1009_InlineDataMustMatchTheoryParameters_TooFewValues { get; } =
		Diagnostic(
			"xUnit1009",
			"InlineData values must match the number of method parameters",
			Usage,
			Error,
			"InlineData values must match the number of method parameters. Remove unused parameters, or add more data for the missing parameters."
		);

	public static DiagnosticDescriptor X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType { get; } =
		Diagnostic(
			"xUnit1010",
			"The value is not convertible to the method parameter type",
			Usage,
			Error,
			"The value is not convertible to the method parameter '{0}' of type '{1}'. Use a compatible data value."
		);

	public static DiagnosticDescriptor X1011_InlineDataMustMatchTheoryParameters_ExtraValue { get; } =
		Diagnostic(
			"xUnit1011",
			"There is no matching method parameter",
			Usage,
			Error,
			"There is no matching method parameter for value: {0}. Remove unused value(s), or add more parameter(s)."
		);

	public static DiagnosticDescriptor X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter { get; } =
		Diagnostic(
			"xUnit1012",
			"Null should only be used for nullable parameters",
			Usage,
			Warning,
			"Null should not be used for type parameter '{0}' of type '{1}'. Use a non-null value, or convert the parameter to a nullable type."
		);

	public static DiagnosticDescriptor X1013_PublicMethodShouldBeMarkedAsTest { get; } =
		Diagnostic(
			"xUnit1013",
			"Public method should be marked as test",
			Usage,
			Warning,
			"Public method '{0}' on test class '{1}' should be marked as a {2}. Reduce the visibility of the method, or add a {2} attribute to the method."
		);

	public static DiagnosticDescriptor X1014_MemberDataShouldUseNameOfOperator { get; } =
		Diagnostic(
			"xUnit1014",
			"MemberData should use nameof operator for member name",
			Usage,
			Warning,
			"MemberData should use nameof operator to reference member '{0}' on type '{1}'. Replace the constant string with nameof."
		);

	public static DiagnosticDescriptor X1015_MemberDataMustReferenceExistingMember { get; } =
		Diagnostic(
			"xUnit1015",
			"MemberData must reference an existing member",
			Usage,
			Error,
			"MemberData must reference an existing member '{0}' on type '{1}'. Fix the member reference, or add the missing data member."
		);

	public static DiagnosticDescriptor X1016_MemberDataMustReferencePublicMember { get; } =
		Diagnostic(
			"xUnit1016",
			"MemberData must reference a public member",
			Usage,
			Error,
			"MemberData must reference a public member. Add or change the visibility of the data member to public."
		);

	public static DiagnosticDescriptor X1017_MemberDataMustReferenceStaticMember { get; } =
		Diagnostic(
			"xUnit1017",
			"MemberData must reference a static member",
			Usage,
			Error,
			"MemberData must reference a static member. Add the static modifier to the data member."
		);

	public static DiagnosticDescriptor X1018_MemberDataMustReferenceValidMemberKind { get; } =
		Diagnostic(
			"xUnit1018",
			"MemberData must reference a valid member kind",
			Usage,
			Error,
			"MemberData must reference a property, field, or method. Convert the data member to a compatible member type."
		);

	public static DiagnosticDescriptor X1019_MemberDataMustReferenceMemberOfValidType { get; } =
		Diagnostic(
			"xUnit1019",
			"MemberData must reference a member providing a valid data type",
			Usage,
			Error,
			"MemberData must reference a data type assignable to {0}. The referenced type '{1}' is not valid."
		);

	public static DiagnosticDescriptor X1020_MemberDataPropertyMustHaveGetter { get; } =
		Diagnostic(
			"xUnit1020",
			"MemberData must reference a property with a public getter",
			Usage,
			Error,
			"MemberData must reference a property with a public getter. Add a public getter to the data member, or change the visibility of the existing getter to public."
		);

	public static DiagnosticDescriptor X1021_MemberDataNonMethodShouldNotHaveParameters { get; } =
		Diagnostic(
			"xUnit1021",
			"MemberData should not have parameters if the referenced member is not a method",
			Usage,
			Warning,
			"MemberData should not have parameters if the referenced member is not a method. Remove the parameter values, or convert the data member to a method with parameters."
		);

	public static DiagnosticDescriptor X1022_TheoryMethodCannotHaveParameterArray { get; } =
		Diagnostic(
			"xUnit1022",
			"Theory methods cannot have a parameter array",
			Usage,
			Error,
			"Theory method '{0}' on test class '{1}' cannot have a parameter array '{2}'. Upgrade to xUnit.net 2.2 or later to enable this feature."
		);

	public static DiagnosticDescriptor X1023_TheoryMethodCannotHaveDefaultParameter { get; } =
		Diagnostic(
			"xUnit1023",
			"Theory methods cannot have default parameter values",
			Usage,
			Error,
			"Theory method '{0}' on test class '{1}' parameter '{2}' cannot have a default value. Upgrade to xUnit.net 2.2 or later to enable this feature."
		);

	public static DiagnosticDescriptor X1024_TestMethodCannotHaveOverloads { get; } =
		Diagnostic(
			"xUnit1024",
			"Test methods cannot have overloads",
			Usage,
			Error,
			"Test method '{0}' on test class '{1}' has the same name as another method declared on class '{2}'. Rename method(s) so that there are no overloaded names."
		);

	public static DiagnosticDescriptor X1025_InlineDataShouldBeUniqueWithinTheory { get; } =
		Diagnostic(
			"xUnit1025",
			"InlineData should be unique within the Theory it belongs to",
			Usage,
			Warning,
			"Theory method '{0}' on test class '{1}' has InlineData duplicate(s). Remove redundant attribute(s) from the theory method."
		);

	public static DiagnosticDescriptor X1026_TheoryMethodShouldUseAllParameters { get; } =
		Diagnostic(
			"xUnit1026",
			"Theory methods should use all of their parameters",
			Usage,
			Warning,
			"Theory method '{0}' on test class '{1}' does not use parameter '{2}'. Use the parameter, or remove the parameter and associated data."
		);

	public static DiagnosticDescriptor X1027_CollectionDefinitionClassMustBePublic { get; } =
		Diagnostic(
			"xUnit1027",
			"Collection definition classes must be public",
			Usage,
			Error,
			"Collection definition classes must be public. Add or change the visibility modifier of the collection definition class to public."
		);

	public static DiagnosticDescriptor X1028_TestMethodHasInvalidReturnType { get; } =
		Diagnostic(
			"xUnit1028",
			"Test method must have valid return type",
			Usage,
			Error,
			"Test methods must have a supported return type. Valid types are: {0}. Change the return type to one of the compatible types."
		);

	public static DiagnosticDescriptor X1029_LocalFunctionsCannotBeTestFunctions { get; } =
		Diagnostic(
			"xUnit1029",
			"Local functions cannot be test functions",
			Usage,
			Error,
			"Local functions cannot be test functions. Remove '{0}'."
		);

	public static DiagnosticDescriptor X1030_DoNotUseConfigureAwait { get; } =
		Diagnostic(
			"xUnit1030",
			"Do not call ConfigureAwait(false) in test method",
			Usage,
			Warning,
			"Test methods should not call ConfigureAwait({0}), as it may bypass parallelization limits. {1}"
		);

	public static DiagnosticDescriptor X1031_DoNotUseBlockingTaskOperations { get; } =
		Diagnostic(
			"xUnit1031",
			"Do not use blocking task operations in test method",
			Usage,
			Warning,
			"Test methods should not use blocking task operations, as they can cause deadlocks. Use an async test method and await instead."
		);

	public static DiagnosticDescriptor X1032_TestClassCannotBeNestedInGenericClass { get; } =
		Diagnostic(
			"xUnit1032",
			"Test classes cannot be nested within a generic class",
			Usage,
			Error,
			"Test classes cannot be nested within a generic class. Move the test class out of the class it is nested in."
		);

	public static DiagnosticDescriptor X1033_TestClassShouldHaveTFixtureArgument { get; } =
		Diagnostic(
			"xUnit1033",
			"Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture",
			Usage,
			Info,
			"Test class '{0}' does not contain constructor argument of type '{1}'. Add a constructor argument to consume the fixture data."
		);

	public static DiagnosticDescriptor X1034_MemberDataArgumentsMustMatchMethodParameters_NullShouldNotBeUsedForIncompatibleParameter { get; } =
		Diagnostic(
			"xUnit1034",
			"Null should only be used for nullable parameters",
			Usage,
			Warning,
			"Null should not be used for type parameter '{0}' of type '{1}'. Use a non-null value, or convert the parameter to a nullable type."
		);

	public static DiagnosticDescriptor X1035_MemberDataArgumentsMustMatchMethodParameters_IncompatibleValueType { get; } =
		Diagnostic(
			"xUnit1035",
			"The value is not convertible to the method parameter type",
			Usage,
			Error,
			"The value is not convertible to the method parameter '{0}' of type '{1}'. Use a compatible data value."
		);

	public static DiagnosticDescriptor X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue { get; } =
		Diagnostic(
			"xUnit1036",
			"There is no matching method parameter",
			Usage,
			Error,
			"There is no matching method parameter for value: {0}. Remove unused value(s), or add more parameter(s)."
		);

	public static DiagnosticDescriptor X1037_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters { get; } =
		Diagnostic(
			"xUnit1037",
			"There are fewer TheoryData type arguments than required by the parameters of the test method",
			Usage,
			Error,
			"There are fewer TheoryData type arguments than required by the parameters of the test method. Add more type parameters to match the method signature, or remove parameters from the test method."
		);

	public static DiagnosticDescriptor X1038_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters { get; } =
		Diagnostic(
			"xUnit1038",
			"There are more TheoryData type arguments than allowed by the parameters of the test method",
			Usage,
			Error,
			"There are more TheoryData type arguments than allowed by the parameters of the test method. Remove unused type arguments, or add more parameters."
		);

	public static DiagnosticDescriptor X1039_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes { get; } =
		Diagnostic(
			"xUnit1039",
			"The type argument to TheoryData is not compatible with the type of the corresponding test method parameter",
			Usage,
			Error,
			"The type argument {0} from {1}.{2} is not compatible with the type of the corresponding test method parameter {3}."
		);

	public static DiagnosticDescriptor X1040_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability { get; } =
		Diagnostic(
			"xUnit1040",
			"The type argument to TheoryData is nullable, while the type of the corresponding test method parameter is not",
			Usage,
			Warning,
			"The type argument {0} from {1}.{2} is nullable, while the type of the corresponding test method parameter {3} is not. Make the TheoryData type non-nullable, or make the test method parameter nullable."
		);

	public static DiagnosticDescriptor X1041_EnsureFixturesHaveASource { get; } =
		Diagnostic(
			"xUnit1041",
			"Fixture arguments to test classes must have fixture sources",
			Usage,
			Warning,
			"Fixture argument '{0}' does not have a fixture source (if it comes from a collection definition, ensure the definition is in the same assembly as the test)"
		);

	public static DiagnosticDescriptor X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis { get; } =
		Diagnostic(
			"xUnit1042",
			"The member referenced by the MemberData attribute returns untyped data rows",
			Usage,
			Info,
			"The member referenced by the MemberData attribute returns untyped data rows, such as object[]. Consider using TheoryData<> as the return type to provide better type safety."
		);

	public static DiagnosticDescriptor X1043_ConstructorOnFactAttributeSubclassShouldBePublic { get; } =
		Diagnostic(
			"xUnit1043",
			"Constructors on classes derived from FactAttribute must be public when used on test methods",
			Usage,
			Error,
			"Constructor '{0}' must be public to be used on a test method."
		);

	// Placeholder for rule X1044

	// Placeholder for rule X1045

	// Placeholder for rule X1046

	// Placeholder for rule X1047

	// Placeholder for rule X1048

	// Placeholder for rule X1049
}
