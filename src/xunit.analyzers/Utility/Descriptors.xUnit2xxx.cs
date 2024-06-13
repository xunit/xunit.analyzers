using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static Xunit.Analyzers.Category;

namespace Xunit.Analyzers;

public static partial class Descriptors
{
	public static DiagnosticDescriptor X2000_AssertEqualLiteralValueShouldBeFirst { get; } =
		Diagnostic(
			"xUnit2000",
			"Constants and literals should be the expected argument",
			Assertions,
			Warning,
			"The literal or constant value {0} should be passed as the 'expected' argument in the call to '{1}' in method '{2}' on type '{3}'. Swap the parameter values."
		);

	public static DiagnosticDescriptor X2001_AssertEqualsShouldNotBeUsed { get; } =
		Diagnostic(
			"xUnit2001",
			"Do not use invalid equality check",
			Assertions,
			Hidden,
			"Do not use {0}. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2002_AssertNullShouldNotBeCalledOnValueTypes { get; } =
		Diagnostic(
			"xUnit2002",
			"Do not use null check on value type",
			Assertions,
			Warning,
			"Do not use {0} on value type '{1}'. Remove this assert."
		);

	public static DiagnosticDescriptor X2003_AssertEqualShouldNotUsedForNullCheck { get; } =
		Diagnostic(
			"xUnit2003",
			"Do not use equality check to test for null value",
			Assertions,
			Warning,
			"Do not use {0} to check for null value. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2004_AssertEqualShouldNotUsedForBoolLiteralCheck { get; } =
		Diagnostic(
			"xUnit2004",
			"Do not use equality check to test for boolean conditions",
			Assertions,
			Warning,
			"Do not use {0} to check for boolean conditions. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2005_AssertSameShouldNotBeCalledOnValueTypes { get; } =
		Diagnostic(
			"xUnit2005",
			"Do not use identity check on value type",
			Assertions,
			Warning,
			"Do not use {0} on value type '{1}'. Value types do not have identity. Use Assert.{2} instead."
		);

	public static DiagnosticDescriptor X2006_AssertEqualGenericShouldNotBeUsedForStringValue { get; } =
		Diagnostic(
			"xUnit2006",
			"Do not use invalid string equality check",
			Assertions,
			Warning,
			"Do not use {0} to test for string equality. Use {1} instead."
		);

	public static DiagnosticDescriptor X2007_AssertIsTypeShouldUseGenericOverload { get; } =
		Diagnostic(
			"xUnit2007",
			"Do not use typeof expression to check the type",
			Assertions,
			Warning,
			"Do not use typeof({0}) expression to check the type. Use Assert.IsType<{0}> instead."
		);

	public static DiagnosticDescriptor X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck { get; } =
		Diagnostic(
			"xUnit2008",
			"Do not use boolean check to match on regular expressions",
			Assertions,
			Warning,
			"Do not use {0} to match on regular expressions. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2009_AssertSubstringCheckShouldNotUseBoolCheck { get; } =
		Diagnostic(
			"xUnit2009",
			"Do not use boolean check to check for substrings",
			Assertions,
			Warning,
			"Do not use {0} to check for substrings. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer { get; } =
		Diagnostic(
			"xUnit2010",
			"Do not use boolean check to check for string equality",
			Assertions,
			Warning,
			"Do not use {0} to check for string equality. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2011_AssertEmptyCollectionCheckShouldNotBeUsed { get; } =
		Diagnostic(
			"xUnit2011",
			"Do not use empty collection check",
			Assertions,
			Warning,
			"Do not use {0} to check for empty collections. Add element inspectors (for non-empty collections), or use Assert.Empty (for empty collections) instead."
		);

	public static DiagnosticDescriptor X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck { get; } =
		Diagnostic(
			"xUnit2012",
			"Do not use boolean check to check if a value exists in a collection",
			Assertions,
			Warning,
			"Do not use {0} to check if a value exists in a collection. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck { get; } =
		Diagnostic(
			"xUnit2013",
			"Do not use equality check to check for collection size.",
			Assertions,
			Warning,
			"Do not use {0} to check for collection size. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck { get; } =
		Diagnostic(
			"xUnit2014",
			"Do not use throws check to check for asynchronously thrown exception",
			Assertions,
			Error,
			"Do not use {0} to check for asynchronously thrown exceptions. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2015_AssertThrowsShouldUseGenericOverload { get; } =
		Diagnostic(
			"xUnit2015",
			"Do not use typeof expression to check the exception type",
			Assertions,
			Warning,
			"Do not use typeof({1}) expression to check the exception type. Use Assert.{0}<{1}> instead."
		);

	public static DiagnosticDescriptor X2016_AssertEqualPrecisionShouldBeInRange { get; } =
		Diagnostic(
			"xUnit2016",
			"Keep precision in the allowed range when asserting equality of doubles or decimals.",
			Assertions,
			Error,
			"Keep precision in range {0} when asserting equality of {1} typed actual value."
		);

	public static DiagnosticDescriptor X2017_AssertCollectionContainsShouldNotUseBoolCheck { get; } =
		Diagnostic(
			"xUnit2017",
			"Do not use Contains() to check if a value exists in a collection",
			Assertions,
			Warning,
			"Do not use {0} to check if a value exists in a collection. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2018_AssertIsTypeShouldNotBeUsedForAbstractType { get; } =
		Diagnostic(
			"xUnit2018",
			"Do not compare an object's exact type to an abstract class or interface",
			Assertions,
			Warning,
			"Do not compare an object's exact type to the {0} '{1}'. Use Assert.{2} instead."
		);

	// Note: X2019 was already covered by X2014, and should not be reused

	public static DiagnosticDescriptor X2020_UseAssertFailInsteadOfBooleanAssert { get; } =
		Diagnostic(
			"xUnit2020",
			"Do not use always-failing boolean assertions",
			Assertions,
			Warning,
			"Do not use Assert.{0}({1}, message) to fail a test. Use Assert.Fail(message) instead."
		);

	public static DiagnosticDescriptor X2021_AsyncAssertionsShouldBeAwaited { get; } =
		Diagnostic(
			"xUnit2021",
			"Async assertions should be awaited",
			Assertions,
			Error,
			"Assert.{0} is async. The resulting task should be awaited (or stored for later awaiting)."
		);

	public static DiagnosticDescriptor X2022_BooleanAssertionsShouldNotBeNegated { get; } =
		Diagnostic(
			"xUnit2022",
			"Boolean assertions should not be negated",
			Assertions,
			Info,
			"Do not negate your value when calling Assert.{0}. Call Assert.{1} without the negation instead."
		);

	public static DiagnosticDescriptor X2023_AssertSingleShouldBeUsedForSingleParameter { get; } =
		Diagnostic(
			"xUnit2023",
			"Do not use collection methods for single-item collections",
			Assertions,
			Info,
			"Do not use Assert.{0} if there is one element in the collection. Use Assert.Single instead."
		);

	public static DiagnosticDescriptor X2024_BooleanAssertionsShouldNotBeUsedForSimpleEqualityCheck { get; } =
		Diagnostic(
			"xUnit2024",
			"Do not use boolean asserts for simple equality tests",
			Assertions,
			Info,
			"Do not use Assert.{0} to test equality against null, numeric, string, or enum literals. Use Assert.{1} instead."
		);

	public static DiagnosticDescriptor X2025_BooleanAssertionCanBeSimplified { get; } =
		Diagnostic(
			"xUnit2025",
			"The boolean assertion statement can be simplified",
			Assertions,
			Info,
			"The use of Assert.{0} can be simplified to avoid using a boolean literal value in an equality test."
		);

	public static DiagnosticDescriptor X2026_SetsMustBeComparedWithEqualityComparer { get; } =
		Diagnostic(
			"xUnit2026",
			"Comparison of sets must be done with IEqualityComparer",
			Assertions,
			Warning,
			"Comparison of two sets may produce inconsistent results if GetHashCode() is not overriden. Consider using Assert.{0}(IEnumerable<T>, IEnumerable<T>, IEqualityComparer<T>) instead."
		);

	public static DiagnosticDescriptor X2027_SetsShouldNotBeComparedToLinearContainers { get; } =
		Diagnostic(
			"xUnit2027",
			"Comparison of sets to linear containers have undefined results",
			Assertions,
			Warning,
			"Comparing an instance of {0} with an instance of {1} has undefined results, because the order of items in the set is not predictable. Create a stable order for the set (i.e., by using OrderBy from Linq)."
		);

	public static DiagnosticDescriptor X2028_DoNotUseAssertEmptyWithProblematicTypes { get; } =
		Diagnostic(
			"xUnit2028",
			"Do not use Assert.Empty or Assert.NotEmpty with problematic types",
			Assertions,
			Warning,
			"Using Assert.{0} with an instance of {1} is problematic, because {2}. Check the length with .Count instead."
		);

	public static DiagnosticDescriptor X2029_AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck { get; } =
		Diagnostic(
			"xUnit2029",
			"Do not use Empty() to check if a value does not exist in a collection",
			Assertions,
			Warning,
			"Do not use Assert.Empty() to check if a valude does not exist in a collection. Use Assert.DoesNotContain() instead.")

	// Placeholder for rule X2030

	// Placeholder for rule X2031

	// Placeholder for rule X2032

	// Placeholder for rule X2033

	// Placeholder for rule X2034

	// Placeholder for rule X2035

	// Placeholder for rule X2036

	// Placeholder for rule X2037

	// Placeholder for rule X2038

	// Placeholder for rule X2039
}
