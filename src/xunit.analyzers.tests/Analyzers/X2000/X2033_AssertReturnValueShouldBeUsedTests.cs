using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertReturnValueShouldBeUsed>;

public class X2033_AssertReturnValueShouldBeUsedTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			class SingleSamples {
				// Success cases

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

				void ExpectedValueOverloadReturnsNothing() {
					var xs = new List<int> { 42 };
					Assert.Single(xs, 42);
					var item = xs.First();
				}

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

				// Failure cases

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

				void RederivationInsideDeclarationInitializer() {
					var xs = new List<string> { "Hello world" };
					{|#4:Assert.Single(xs)|};
					var length = xs.Single().Length;
				}
			}

			class IsTypeSamples {
				// Success cases

				void TypeGuardWithoutRederivation() {
					object value = "Hello world";
					Assert.IsType<string>(value);
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

				// Failure cases

				void ViaCast() {
					object value = "Hello world";
					{|#10:Assert.IsType<string>(value)|};
					var text = (string)value;
				}

				void ViaAsExpression() {
					object value = "Hello world";
					{|#11:Assert.IsType<string>(value)|};
					var text = value as string;
				}
			}

			class IsAssignableFromSamples {
				// Success cases

				void TypeGuardWithoutRederivation() {
					object value = "Hello world";
					Assert.IsAssignableFrom<string>(value);
				}

				void DifferentTypeArgument() {
					object value = "Hello world";
					Assert.IsAssignableFrom<string>(value);
					var comparable = (IComparable)value;
				}

				void CastOfDifferentExpression() {
					object value = "Hello world";
					object other = "Other value";
					Assert.IsAssignableFrom<string>(value);
					var text = (string)other;
				}

				// Failure cases

				void ViaCast() {
					object value = "Hello world";
					{|#20:Assert.IsAssignableFrom<string>(value)|};
					var text = (string)value;
				}

				void ViaAsExpression() {
					object value = "Hello world";
					{|#21:Assert.IsAssignableFrom<string>(value)|};
					var text = value as string;
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Single", "xs.Single()"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Single", "xs.First()"),
			Verify.Diagnostic().WithLocation(2).WithArguments("Single", "xs.ElementAt(0)"),
			Verify.Diagnostic().WithLocation(3).WithArguments("Single", "xs[0]"),
			Verify.Diagnostic().WithLocation(4).WithArguments("Single", "xs.Single()"),

			Verify.Diagnostic().WithLocation(10).WithArguments("IsType", "(string)value"),
			Verify.Diagnostic().WithLocation(11).WithArguments("IsType", "value as string"),

			Verify.Diagnostic().WithLocation(20).WithArguments("IsAssignableFrom", "(string)value"),
			Verify.Diagnostic().WithLocation(21).WithArguments("IsAssignableFrom", "value as string"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
