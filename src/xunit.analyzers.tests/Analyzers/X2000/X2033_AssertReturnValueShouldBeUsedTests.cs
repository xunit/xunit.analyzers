using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertReturnValueShouldBeUsed>;

public class X2033_AssertReturnValueShouldBeUsedTests
{
	[Fact]
	public async ValueTask SingleFollowedByRederivation_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			class TestClass {
				void ViaSingle() {
					var xs = new List<int> { 42 };
					{|#0:Assert.Single(xs)|};
					var item = xs.Single();
				}

				void ViaFirst() {
					var xs = new List<int> { 42 };
					{|#1:Assert.Single(xs)|};
					var item = xs.First();
				}

				void ViaElementAt() {
					var xs = new List<int> { 42 };
					{|#2:Assert.Single(xs)|};
					var item = xs.ElementAt(0);
				}

				void ViaIndexer() {
					var xs = new List<int> { 42 };
					{|#3:Assert.Single(xs)|};
					var item = xs[0];
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Single", "xs.Single()"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Single", "xs.First()"),
			Verify.Diagnostic().WithLocation(2).WithArguments("Single", "xs.ElementAt(0)"),
			Verify.Diagnostic().WithLocation(3).WithArguments("Single", "xs[0]"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask TypeAssertFollowedByRederivation_Triggers()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void ViaCast() {
					object value = "Hello world";
					{|#0:Assert.IsType<string>(value)|};
					var text = (string)value;
				}

				void ViaAsExpression() {
					object value = "Hello world";
					{|#1:Assert.IsAssignableFrom<string>(value)|};
					var text = value as string;
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("IsType", "(string)value"),
			Verify.Diagnostic().WithLocation(1).WithArguments("IsAssignableFrom", "value as string"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask RederivationOnlyInLaterStatementsOfSameBlock_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			class TestClass {
				void RederivationInsideDeclarationInitializer() {
					var xs = new List<string> { "Hello world" };
					{|#0:Assert.Single(xs)|};
					var length = xs.Single().Length;
				}
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Single", "xs.Single()");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask NoRederivation_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			class TestClass {
				void GuardWithoutRederivation() {
					var xs = new List<int> { 42 };
					Assert.Single(xs);
					var count = xs.Count;
				}

				void ReturnValueAlreadyUsed() {
					var xs = new List<int> { 42 };
					var item = Assert.Single(xs);
					var text = item.ToString();
				}

				void TypeGuardWithoutRederivation() {
					object value = "Hello world";
					Assert.IsType<string>(value);
				}

				void ExpectedValueOverloadReturnsNothing() {
					var xs = new List<int> { 42 };
					Assert.Single(xs, 42);
					var item = xs.First();
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask DifferentComputation_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			class TestClass {
				void PredicateOverloadIsDifferentComputation() {
					var xs = new List<int> { 42 };
					Assert.Single(xs);
					var filtered = xs.Single(x => x > 21);
				}

				void DifferentCollection() {
					var xs = new List<int> { 42 };
					var ys = new List<int> { 21 };
					Assert.Single(xs);
					var other = ys.First();
				}

				void DifferentTypeArgument() {
					object value = "Hello world";
					Assert.IsType<string>(value);
					var comparable = (IComparable)value;
				}

				void CastOfDifferentExpression() {
					object value = "Hello world";
					object other = "Other value";
					Assert.IsType<string>(value);
					var text = (string)other;
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask RederivationOutsideWindow_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			class TestClass {
				void ReassignmentBetweenAssertAndRederivation() {
					var xs = new List<int> { 42 };
					Assert.Single(xs);
					xs = new List<int> { 1, 2 };
					var first = xs.First();
				}

				void RederivationBeforeAssert() {
					var xs = new List<int> { 42 };
					var first = xs.First();
					Assert.Single(xs);
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
