using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static Xunit.Analyzers.Category;

namespace Xunit.Analyzers;

public enum Category
{
	// 1xxx
	Usage,

	// 2xxx
	Assertions,

	// 3xxx
	Extensibility,
}

public static class Descriptors
{
	static readonly ConcurrentDictionary<Category, string> categoryMapping = new();

	static DiagnosticDescriptor Rule(
		string id,
		string title,
		Category category,
		DiagnosticSeverity defaultSeverity,
		string messageFormat)
	{
		var helpLink = $"https://xunit.net/xunit.analyzers/rules/{id}";
		var categoryString = categoryMapping.GetOrAdd(category, c => c.ToString());

		return new DiagnosticDescriptor(id, title, messageFormat, categoryString, defaultSeverity, isEnabledByDefault: true, helpLinkUri: helpLink);
	}

	public static DiagnosticDescriptor X1000_TestClassMustBePublic { get; } =
		Rule(
			"xUnit1000",
			"Test classes must be public",
			Usage,
			Error,
			"Test classes must be public. Add or change the visibility modifier of the test class to public."
		);

	public static DiagnosticDescriptor X1001_FactMethodMustNotHaveParameters { get; } =
		Rule(
			"xUnit1001",
			"Fact methods cannot have parameters",
			Usage,
			Error,
			"Fact methods cannot have parameters. Remove the parameters from the method or convert it into a Theory."
		);

	public static DiagnosticDescriptor X1002_TestMethodMustNotHaveMultipleFactAttributes { get; } =
		Rule(
			"xUnit1002",
			"Test methods cannot have multiple Fact or Theory attributes",
			Usage,
			Error,
			"Test methods cannot have multiple Fact or Theory attributes. Remove all but one of the attributes."
		);

	public static DiagnosticDescriptor X1003_TheoryMethodMustHaveTestData { get; } =
		Rule(
			"xUnit1003",
			"Theory methods must have test data",
			Usage,
			Error,
			"Theory methods must have test data. Use InlineData, MemberData, or ClassData to provide test data for the Theory."
		);

	public static DiagnosticDescriptor X1004_TestMethodShouldNotBeSkipped { get; } =
		Rule(
			"xUnit1004",
			"Test methods should not be skipped",
			Usage,
			Info,
			"Test methods should not be skipped. Remove the Skip property to start running the test again."
		);

	public static DiagnosticDescriptor X1005_FactMethodShouldNotHaveTestData { get; } =
		Rule(
			"xUnit1005",
			"Fact methods should not have test data",
			Usage,
			Warning,
			"Fact methods should not have test data. Remove the test data, or convert the Fact to a Theory."
		);

	public static DiagnosticDescriptor X1006_TheoryMethodShouldHaveParameters { get; } =
		Rule(
			"xUnit1006",
			"Theory methods should have parameters",
			Usage,
			Warning,
			"Theory methods should have parameters. Add parameter(s) to the theory method."
		);

	public static DiagnosticDescriptor X1007_ClassDataAttributeMustPointAtValidClass { get; } =
		Rule(
			"xUnit1007",
			"ClassData must point at a valid class",
			Usage,
			Error,
			"ClassData must point at a valid class. The class {0} must be public, not sealed, with an empty constructor, and implement IEnumerable<object[]>."
		);

	public static DiagnosticDescriptor X1008_DataAttributeShouldBeUsedOnATheory { get; } =
		Rule(
			"xUnit1008",
			"Test data attribute should only be used on a Theory",
			Usage,
			Warning,
			"Test data attribute should only be used on a Theory. Remove the test data, or add the Theory attribute to the test method."
		);

	public static DiagnosticDescriptor X1009_InlineDataMustMatchTheoryParameters_TooFewValues { get; } =
		Rule(
			"xUnit1009",
			"InlineData values must match the number of method parameters",
			Usage,
			Error,
			"InlineData values must match the number of method parameters. Remove unused parameters, or add more data for the missing parameters."
		);

	public static DiagnosticDescriptor X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType { get; } =
		Rule(
			"xUnit1010",
			"The value is not convertible to the method parameter type",
			Usage,
			Error,
			"The value is not convertible to the method parameter '{0}' of type '{1}'. Use a compatible data value."
		);

	public static DiagnosticDescriptor X1011_InlineDataMustMatchTheoryParameters_ExtraValue { get; } =
		Rule(
			"xUnit1011",
			"There is no matching method parameter",
			Usage,
			Error,
			"There is no matching method parameter for value: {0}. Remove unused value(s), or add more parameter(s)."
		);

	public static DiagnosticDescriptor X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter { get; } =
		Rule(
			"xUnit1012",
			"Null should only be used for nullable parameters",
			Usage,
			Warning,
			"Null should not be used for type parameter '{0}' of type '{1}'. Use a non-null value, or convert the parameter to a nullable type."
		);

	public static DiagnosticDescriptor X1013_PublicMethodShouldBeMarkedAsTest { get; } =
		Rule(
			"xUnit1013",
			"Public method should be marked as test",
			Usage,
			Warning,
			"Public method '{0}' on test class '{1}' should be marked as a {2}. Reduce the visibility of the method, or add a {2} attribute to the method."
		);

	public static DiagnosticDescriptor X1014_MemberDataShouldUseNameOfOperator { get; } =
		Rule(
			"xUnit1014",
			"MemberData should use nameof operator for member name",
			Usage,
			Warning,
			"MemberData should use nameof operator to reference member '{0}' on type '{1}'. Replace the constant string with nameof."
		);

	public static DiagnosticDescriptor X1015_MemberDataMustReferenceExistingMember { get; } =
		Rule(
			"xUnit1015",
			"MemberData must reference an existing member",
			Usage,
			Error,
			"MemberData must reference an existing member '{0}' on type '{1}'. Fix the member reference, or add the missing data member."
		);

	public static DiagnosticDescriptor X1016_MemberDataMustReferencePublicMember { get; } =
		Rule(
			"xUnit1016",
			"MemberData must reference a public member",
			Usage,
			Error,
			"MemberData must reference a public member. Add or change the visibility of the data member to public."
		);

	public static DiagnosticDescriptor X1017_MemberDataMustReferenceStaticMember { get; } =
		Rule(
			"xUnit1017",
			"MemberData must reference a static member",
			Usage,
			Error,
			"MemberData must reference a static member. Add the static modifier to the data member."
		);

	public static DiagnosticDescriptor X1018_MemberDataMustReferenceValidMemberKind { get; } =
		Rule(
			"xUnit1018",
			"MemberData must reference a valid member kind",
			Usage,
			Error,
			"MemberData must reference a property, field, or method. Convert the data member to a compatible member type."
		);

	public static DiagnosticDescriptor X1019_MemberDataMustReferenceMemberOfValidType { get; } =
		Rule(
			"xUnit1019",
			"MemberData must reference a member providing a valid data type",
			Usage,
			Error,
			"MemberData must reference a data type assignable to {0}. The referenced type '{1}' is not valid."
		);

	public static DiagnosticDescriptor X1020_MemberDataPropertyMustHaveGetter { get; } =
		Rule(
			"xUnit1020",
			"MemberData must reference a property with a public getter",
			Usage,
			Error,
			"MemberData must reference a property with a public getter. Add a public getter to the data member, or change the visibility of the existing getter to public."
		);

	public static DiagnosticDescriptor X1021_MemberDataNonMethodShouldNotHaveParameters { get; } =
		Rule(
			"xUnit1021",
			"MemberData should not have parameters if the referenced member is not a method",
			Usage,
			Warning,
			"MemberData should not have parameters if the referenced member is not a method. Remove the parameter values, or convert the data member to a method with parameters."
		);

	public static DiagnosticDescriptor X1022_TheoryMethodCannotHaveParameterArray { get; } =
		Rule(
			"xUnit1022",
			"Theory methods cannot have a parameter array",
			Usage,
			Error,
			"Theory method '{0}' on test class '{1}' cannot have a parameter array '{2}'. Upgrade to xUnit.net 2.2 or later to enable this feature."
		);

	public static DiagnosticDescriptor X1023_TheoryMethodCannotHaveDefaultParameter { get; } =
		Rule(
			"xUnit1023",
			"Theory methods cannot have default parameter values",
			Usage,
			Error,
			"Theory method '{0}' on test class '{1}' parameter '{2}' cannot have a default value. Upgrade to xUnit.net 2.2 or later to enable this feature."
		);

	public static DiagnosticDescriptor X1024_TestMethodCannotHaveOverloads { get; } =
		Rule(
			"xUnit1024",
			"Test methods cannot have overloads",
			Usage,
			Error,
			"Test method '{0}' on test class '{1}' has the same name as another method declared on class '{2}'. Rename method(s) so that there are no overloaded names."
		);

	public static DiagnosticDescriptor X1025_InlineDataShouldBeUniqueWithinTheory { get; } =
		Rule(
			"xUnit1025",
			"InlineData should be unique within the Theory it belongs to",
			Usage,
			Warning,
			"Theory method '{0}' on test class '{1}' has InlineData duplicate(s). Remove redundant attribute(s) from the theory method."
		);

	public static DiagnosticDescriptor X1026_TheoryMethodShouldUseAllParameters { get; } =
		Rule(
			"xUnit1026",
			"Theory methods should use all of their parameters",
			Usage,
			Warning,
			"Theory method '{0}' on test class '{1}' does not use parameter '{2}'. Use the parameter, or remove the parameter and associated data."
		);

	public static DiagnosticDescriptor X1027_CollectionDefinitionClassMustBePublic { get; } =
		Rule(
			"xUnit1027",
			"Collection definition classes must be public",
			Usage,
			Error,
			"Collection definition classes must be public. Add or change the visibility modifier of the collection definition class to public."
		);

	public static DiagnosticDescriptor X1028_TestMethodHasInvalidReturnType { get; } =
		Rule(
			"xUnit1028",
			"Test method must have valid return type",
			Usage,
			Error,
			"Test methods must have a supported return type. Valid types are: {0}. Change the return type to one of the compatible types."
		);

	public static DiagnosticDescriptor X1029_LocalFunctionsCannotBeTestFunctions { get; } =
		Rule(
			"xUnit1029",
			"Local functions cannot be test functions",
			Usage,
			Error,
			"Local functions cannot be test functions. Remove '{0}'."
		);

	public static DiagnosticDescriptor X1030_DoNotUseConfigureAwait { get; } =
		Rule(
			"xUnit1030",
			"Do not call ConfigureAwait(false) in test method",
			Usage,
			Warning,
			"Test methods should not call ConfigureAwait({0}), as it may bypass parallelization limits. {1}"
		);

	public static DiagnosticDescriptor X1031_DoNotUseBlockingTaskOperations { get; } =
		Rule(
			"xUnit1031",
			"Do not use blocking task operations in test method",
			Usage,
			Warning,
			"Test methods should not use blocking task operations, as they can cause deadlocks. Use an async test method and await instead."
		);

	public static DiagnosticDescriptor X1032_TestClassCannotBeNestedInGenericClass { get; } =
		Rule(
			"xUnit1032",
			"Test classes cannot be nested within a generic class",
			Usage,
			Error,
			"Test classes cannot be nested within a generic class. Move the test class out of the class it is nested in."
		);

	public static DiagnosticDescriptor X1033_TestClassShouldHaveTFixtureArgument { get; } =
		Rule(
			"xUnit1033",
			"Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture",
			Usage,
			Info,
			"Test class '{0}' does not contain constructor argument of type '{1}'. Add a constructor argument to consume the fixture data."
		);

	public static DiagnosticDescriptor X1034_MemberDataArgumentsMustMatchMethodParameters_NullShouldNotBeUsedForIncompatibleParameter { get; } =
		Rule(
			"xUnit1034",
			"Null should only be used for nullable parameters",
			Usage,
			Warning,
			"Null should not be used for type parameter '{0}' of type '{1}'. Use a non-null value, or convert the parameter to a nullable type."
		);

	public static DiagnosticDescriptor X1035_MemberDataArgumentsMustMatchMethodParameters_IncompatibleValueType { get; } =
		Rule(
			"xUnit1035",
			"The value is not convertible to the method parameter type",
			Usage,
			Error,
			"The value is not convertible to the method parameter '{0}' of type '{1}'. Use a compatible data value."
		);

	public static DiagnosticDescriptor X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue { get; } =
		Rule(
			"xUnit1036",
			"There is no matching method parameter",
			Usage,
			Error,
			"There is no matching method parameter for value: {0}. Remove unused value(s), or add more parameter(s)."
		);

	public static DiagnosticDescriptor X1037_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters { get; } =
		Rule(
			"xUnit1037",
			"There are fewer TheoryData type arguments than required by the parameters of the test method",
			Usage,
			Error,
			"There are fewer TheoryData type arguments than required by the parameters of the test method. Add more type parameters to match the method signature, or remove parameters from the test method."
		);

	public static DiagnosticDescriptor X1038_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters { get; } =
		Rule(
			"xUnit1038",
			"There are more TheoryData type arguments than allowed by the parameters of the test method",
			Usage,
			Error,
			"There are more TheoryData type arguments than allowed by the parameters of the test method. Remove unused type arguments, or add more parameters."
		);

	public static DiagnosticDescriptor X1039_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes { get; } =
		Rule(
			"xUnit1039",
			"The type argument to TheoryData is not compatible with the type of the corresponding test method parameter",
			Usage,
			Error,
			"The type argument {0} from {1}.{2} is not compatible with the type of the corresponding test method parameter {3}."
		);

	public static DiagnosticDescriptor X1040_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability { get; } =
		Rule(
			"xUnit1040",
			"The type argument to TheoryData is nullable, while the type of the corresponding test method parameter is not",
			Usage,
			Warning,
			"The type argument {0} from {1}.{2} is nullable, while the type of the corresponding test method parameter {3} is not. Make the TheoryData type non-nullable, or make the test method parameter nullable."
		);

	public static DiagnosticDescriptor X1041_EnsureFixturesHaveASource { get; } =
		Rule(
			"xUnit1041",
			"Fixture arguments to test classes must have fixture sources",
			Usage,
			Warning,
			"Fixture argument '{0}' does not have a fixture source (if it comes from a collection definition, ensure the definition is in the same assembly as the test)"
		);

	public static DiagnosticDescriptor X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis { get; } =
		Rule(
			"xUnit1042",
			"The member referenced by the MemberData attribute returns untyped data rows",
			Usage,
			Info,
			"The member referenced by the MemberData attribute returns untyped data rows, such as object[]. Consider using TheoryData<> as the return type to provide better type safety."
		);

	// Placeholder for rule X1043

	// Placeholder for rule X1044

	// Placeholder for rule X1045

	// Placeholder for rule X1046

	// Placeholder for rule X1047

	// Placeholder for rule X1048

	// Placeholder for rule X1049

	public static DiagnosticDescriptor X2000_AssertEqualLiteralValueShouldBeFirst { get; } =
		Rule(
			"xUnit2000",
			"Constants and literals should be the expected argument",
			Assertions,
			Warning,
			"The literal or constant value {0} should be passed as the 'expected' argument in the call to '{1}' in method '{2}' on type '{3}'. Swap the parameter values."
		);

	public static DiagnosticDescriptor X2001_AssertEqualsShouldNotBeUsed { get; } =
		Rule(
			"xUnit2001",
			"Do not use invalid equality check",
			Assertions,
			Hidden,
			"Do not use {0}. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2002_AssertNullShouldNotBeCalledOnValueTypes { get; } =
		Rule(
			"xUnit2002",
			"Do not use null check on value type",
			Assertions,
			Warning,
			"Do not use {0} on value type '{1}'. Remove this assert."
		);

	public static DiagnosticDescriptor X2003_AssertEqualShouldNotUsedForNullCheck { get; } =
		Rule(
			"xUnit2003",
			"Do not use equality check to test for null value",
			Assertions,
			Warning,
			"Do not use {0} to check for null value. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2004_AssertEqualShouldNotUsedForBoolLiteralCheck { get; } =
		Rule(
			"xUnit2004",
			"Do not use equality check to test for boolean conditions",
			Assertions,
			Warning,
			"Do not use {0} to check for boolean conditions. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2005_AssertSameShouldNotBeCalledOnValueTypes { get; } =
		Rule(
			"xUnit2005",
			"Do not use identity check on value type",
			Assertions,
			Warning,
			"Do not use {0} on value type '{1}'. Value types do not have identity. Use Assert.{2} instead."
		);

	public static DiagnosticDescriptor X2006_AssertEqualGenericShouldNotBeUsedForStringValue { get; } =
		Rule(
			"xUnit2006",
			"Do not use invalid string equality check",
			Assertions,
			Warning,
			"Do not use {0} to test for string equality. Use {1} instead."
		);

	public static DiagnosticDescriptor X2007_AssertIsTypeShouldUseGenericOverload { get; } =
		Rule(
			"xUnit2007",
			"Do not use typeof expression to check the type",
			Assertions,
			Warning,
			"Do not use typeof({0}) expression to check the type. Use Assert.IsType<{0}> instead."
		);

	public static DiagnosticDescriptor X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck { get; } =
		Rule(
			"xUnit2008",
			"Do not use boolean check to match on regular expressions",
			Assertions,
			Warning,
			"Do not use {0} to match on regular expressions. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2009_AssertSubstringCheckShouldNotUseBoolCheck { get; } =
		Rule(
			"xUnit2009",
			"Do not use boolean check to check for substrings",
			Assertions,
			Warning,
			"Do not use {0} to check for substrings. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer { get; } =
		Rule(
			"xUnit2010",
			"Do not use boolean check to check for string equality",
			Assertions,
			Warning,
			"Do not use {0} to check for string equality. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2011_AssertEmptyCollectionCheckShouldNotBeUsed { get; } =
		Rule(
			"xUnit2011",
			"Do not use empty collection check",
			Assertions,
			Warning,
			"Do not use {0} to check for empty collections. Add element inspectors (for non-empty collections), or use Assert.Empty (for empty collections) instead."
		);

	public static DiagnosticDescriptor X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck { get; } =
		Rule(
			"xUnit2012",
			"Do not use boolean check to check if a value exists in a collection",
			Assertions,
			Warning,
			"Do not use {0} to check if a value exists in a collection. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck { get; } =
		Rule(
			"xUnit2013",
			"Do not use equality check to check for collection size.",
			Assertions,
			Warning,
			"Do not use {0} to check for collection size. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck { get; } =
		Rule(
			"xUnit2014",
			"Do not use throws check to check for asynchronously thrown exception",
			Assertions,
			Error,
			"Do not use {0} to check for asynchronously thrown exceptions. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2015_AssertThrowsShouldUseGenericOverload { get; } =
		Rule(
			"xUnit2015",
			"Do not use typeof expression to check the exception type",
			Assertions,
			Warning,
			"Do not use typeof({1}) expression to check the exception type. Use Assert.{0}<{1}> instead."
		);

	public static DiagnosticDescriptor X2016_AssertEqualPrecisionShouldBeInRange { get; } =
		Rule(
			"xUnit2016",
			"Keep precision in the allowed range when asserting equality of doubles or decimals.",
			Assertions,
			Error,
			"Keep precision in range {0} when asserting equality of {1} typed actual value."
		);

	public static DiagnosticDescriptor X2017_AssertCollectionContainsShouldNotUseBoolCheck { get; } =
		Rule(
			"xUnit2017",
			"Do not use Contains() to check if a value exists in a collection",
			Assertions,
			Warning,
			"Do not use {0} to check if a value exists in a collection. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2018_AssertIsTypeShouldNotBeUsedForAbstractType { get; } =
		Rule(
			"xUnit2018",
			"Do not compare an object's exact type to an abstract class or interface",
			Assertions,
			Warning,
			"Do not compare an object's exact type to the {0} '{1}'. Use Assert.{2} instead."
		);

	// Note: X2019 was already covered by X2014, and should not be reused

	public static DiagnosticDescriptor X2020_UseAssertFailInsteadOfBooleanAssert { get; } =
		Rule(
			"xUnit2020",
			"Do not use always-failing boolean assertions",
			Assertions,
			Warning,
			"Do not use Assert.{0}({1}, message) to fail a test. Use Assert.Fail(message) instead."
		);

	public static DiagnosticDescriptor X2021_AsyncAssertionsShouldBeAwaited { get; } =
		Rule(
			"xUnit2021",
			"Async assertions should be awaited",
			Assertions,
			Error,
			"Assert.{0} is async. The resulting task should be awaited (or stored for later awaiting)."
		);

	public static DiagnosticDescriptor X2022_BooleanAssertionsShouldNotBeNegated { get; } =
		Rule(
			"xUnit2022",
			"Boolean assertions should not be negated",
			Assertions,
			Info,
			"Do not negate your value when calling Assert.{0}. Call Assert.{1} without the negation instead."
		);

	public static DiagnosticDescriptor X2023_AssertSingleShouldBeUsedForSingleParameter { get; } =
		Rule(
			"xUnit2023",
			"Do not use collection methods for single-item collections",
			Assertions,
			Info,
			"Do not use Assert.{0} if there is one element in the collection. Use Assert.Single instead."
		);

	public static DiagnosticDescriptor X2024_BooleanAssertionsShouldNotBeUsedForSimpleEqualityCheck { get; } =
		Rule(
			"xUnit2024",
			"Do not use boolean asserts for simple equality tests",
			Assertions,
			Info,
			"Do not use Assert.{0} to test equality against null, numeric, string, or enum literals. Use Assert.{1} instead."
		);


	public static DiagnosticDescriptor X2025_BooleanAssertionCanBeSimplified { get; } =
		Rule(
			"xUnit2025",
			"The boolean assertion statement can be simplified",
			Assertions,
			Info,
			"The use of Assert.{0} can be simplified to avoid using a boolean literal value in an equality test."
		);

	public static DiagnosticDescriptor X2026_SetsMustBeComparedWithEqualityComparer { get; } =
		Rule(
			"xUnit2026",
			"Comparison of sets must be done with IEqualityComparer",
			Assertions,
			Warning,
			"Comparison of two sets may produce inconsistent results if GetHashCode() is not overriden. Consider using Assert.{0}(IEnumerable<T>, IEnumerable<T>, IEqualityComparer<T>) instead."
		);

	public static DiagnosticDescriptor X2027_SetsShouldNotBeComparedToLinearContainers { get; } =
		Rule(
			"xUnit2027",
			"Comparison of sets to linear containers have undefined results",
			Assertions,
			Warning,
			"Comparing an instance of {0} with an instance of {1} has undefined results, because the order of items in the set is not predictable. Create a stable order for the set (i.e., by using OrderBy from Linq)."
		);

	// Placeholder for rule X2028

	// Placeholder for rule X2029

	public static DiagnosticDescriptor X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObject { get; } =
		Rule(
			"xUnit3000",
			"Classes which cross AppDomain boundaries must derive directly or indirectly from LongLivedMarshalByRefObject",
			Extensibility,
			Error,
			"Class {0} must derive directly or indirectly from LongLivedMarshalByRefObject."
		);

	public static DiagnosticDescriptor X3001_SerializableClassMustHaveParameterlessConstructor { get; } =
		Rule(
			"xUnit3001",
			"Classes that implement Xunit.Abstractions.IXunitSerializable must have a public parameterless constructor",
			Extensibility,
			Error,
			"Class {0} must have a public parameterless constructor to support Xunit.Abstractions.IXunitSerializable."
		);

	// Placeholder for rule X3002

	// Placeholder for rule X3003

	// Placeholder for rule X3004

	// Placeholder for rule X3005

	// Placeholder for rule X3006

	// Placeholder for rule X3007

	// Placeholder for rule X3008

	// Placeholder for rule X3009
}
