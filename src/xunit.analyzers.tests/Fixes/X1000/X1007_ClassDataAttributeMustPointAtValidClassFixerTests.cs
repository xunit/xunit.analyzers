using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class X1007_ClassDataAttributeMustPointAtValidClassFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			#pragma warning disable xUnit1050

			using System.Collections.Generic;
			using Xunit;

			public class AddsIEnumerable {
			}

			public class AddsIEnumerableTestClass {
				[Theory]
				[{|xUnit1007:ClassData(typeof(AddsIEnumerable))|}]
				public void TestMethod(int _) { }
			}

			public class ConvertsParameterlessConstructorToPublic : {|CS0535:{|CS0535:IEnumerable<object[]>|}|} {
				ConvertsParameterlessConstructorToPublic() { }
			}

			public class ConvertsParameterlessConstructorToPublicTestClass {
				[Theory]
				[{|xUnit1007:ClassData(typeof(ConvertsParameterlessConstructorToPublic))|}]
				public void TestMethod(int _) { }
			}

			public class AddsPublicParameterlessConstructor : {|CS0535:{|CS0535:IEnumerable<object[]>|}|} {
				AddsPublicParameterlessConstructor(int _) { }
			}

			public class AddsPublicParameterlessConstructorTestClass {
				[Theory]
				[{|xUnit1007:ClassData(typeof(AddsPublicParameterlessConstructor))|}]
				public void TestMethod(int _) { }
			}

			public abstract class RemovesAbstractModifierFromDataClass : {|CS0535:{|CS0535:IEnumerable<object[]>|}|} { }

			public class RemovesAbstractModifierFromDataClassTestClass {
				[Theory]
				[{|xUnit1007:ClassData(typeof(RemovesAbstractModifierFromDataClass))|}]
				public void TestMethod(int _) { }
			}
			""";
		var after = /* lang=c#-test */ """
			#pragma warning disable xUnit1050

			using System.Collections.Generic;
			using Xunit;

			public class AddsIEnumerable : {|CS0535:{|CS0535:IEnumerable<object[]>|}|}
			{
			}

			public class AddsIEnumerableTestClass {
				[Theory]
				[ClassData(typeof(AddsIEnumerable))]
				public void TestMethod(int _) { }
			}

			public class ConvertsParameterlessConstructorToPublic : {|CS0535:{|CS0535:IEnumerable<object[]>|}|} {
				public ConvertsParameterlessConstructorToPublic() { }
			}

			public class ConvertsParameterlessConstructorToPublicTestClass {
				[Theory]
				[ClassData(typeof(ConvertsParameterlessConstructorToPublic))]
				public void TestMethod(int _) { }
			}

			public class AddsPublicParameterlessConstructor : {|CS0535:{|CS0535:IEnumerable<object[]>|}|} {
				AddsPublicParameterlessConstructor(int _) { }

				public AddsPublicParameterlessConstructor()
				{
				}
			}

			public class AddsPublicParameterlessConstructorTestClass {
				[Theory]
				[ClassData(typeof(AddsPublicParameterlessConstructor))]
				public void TestMethod(int _) { }
			}

			public class RemovesAbstractModifierFromDataClass : {|CS0535:{|CS0535:IEnumerable<object[]>|}|} { }

			public class RemovesAbstractModifierFromDataClassTestClass {
				[Theory]
				[ClassData(typeof(RemovesAbstractModifierFromDataClass))]
				public void TestMethod(int _) { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}
}
