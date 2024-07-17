using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class ClassDataAttributeMustPointAtValidClassFixerTests
{
	[Fact]
	public async Task AddsIEnumerable()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestData {
			}

			public class TestClass {
			    [Theory]
			    [{|xUnit1007:ClassData(typeof(TestData))|}]
			    public void TestMethod(int _) { }
			}
			""";
		var afterV2 = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestData : {|CS0535:{|CS0535:IEnumerable<object[]>|}|}
			{
			}

			public class TestClass {
			    [Theory]
			    [ClassData(typeof(TestData))]
			    public void TestMethod(int _) { }
			}
			""";
		var afterV3 = afterV2.Replace("ClassData(typeof(TestData))", "{|xUnit1050:ClassData(typeof(TestData))|}");

		await Verify.VerifyCodeFixV2(before, afterV2, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
		await Verify.VerifyCodeFixV3(before, afterV3, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}

	[Fact]
	public async Task ConvertsParameterlessConstructorToPublic()
	{
		var before = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class TestData : IEnumerable<object[]> {
			    TestData() { }

			    public IEnumerator<object[]> GetEnumerator() => null;
			    IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class TestClass {
			    [Theory]
			    [{|xUnit1007:ClassData(typeof(TestData))|}]
			    public void TestMethod(int _) { }
			}
			""";
		var afterV2 = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class TestData : IEnumerable<object[]> {
			    public TestData() { }

			    public IEnumerator<object[]> GetEnumerator() => null;
			    IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class TestClass {
			    [Theory]
			    [ClassData(typeof(TestData))]
			    public void TestMethod(int _) { }
			}
			""";
		var afterV3 = afterV2.Replace("ClassData(typeof(TestData))", "{|xUnit1050:ClassData(typeof(TestData))|}");

		await Verify.VerifyCodeFixV2(before, afterV2, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
		await Verify.VerifyCodeFixV3(before, afterV3, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}

	[Fact]
	public async Task AddsPublicParameterlessConstructor()
	{
		var before = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class TestData : IEnumerable<object[]> {
			    TestData(int _) { }

			    public IEnumerator<object[]> GetEnumerator() => null;
			    IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class TestClass {
			    [Theory]
			    [{|xUnit1007:ClassData(typeof(TestData))|}]
			    public void TestMethod(int _) { }
			}
			""";
		var afterV2 = """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class TestData : IEnumerable<object[]> {
			    TestData(int _) { }

			    public IEnumerator<object[]> GetEnumerator() => null;
			    IEnumerator IEnumerable.GetEnumerator() => null;

			    public TestData()
			    {
			    }
			}

			public class TestClass {
			    [Theory]
			    [ClassData(typeof(TestData))]
			    public void TestMethod(int _) { }
			}
			""";
		var afterV3 = afterV2.Replace("ClassData(typeof(TestData))", "{|xUnit1050:ClassData(typeof(TestData))|}");

		await Verify.VerifyCodeFixV2(before, afterV2, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
		await Verify.VerifyCodeFixV3(before, afterV3, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}

	[Fact]
	public async Task RemovesAbstractModifierFromDataClass()
	{
		var before = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public abstract class TestData : IEnumerable<object[]> {
			    public IEnumerator<object[]> GetEnumerator() => null;
			    IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class TestClass {
			    [Theory]
			    [{|xUnit1007:ClassData(typeof(TestData))|}]
			    public void TestMethod(int _) { }
			}
			""";
		var afterV2 = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class TestData : IEnumerable<object[]> {
			    public IEnumerator<object[]> GetEnumerator() => null;
			    IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class TestClass {
			    [Theory]
			    [ClassData(typeof(TestData))]
			    public void TestMethod(int _) { }
			}
			""";
		var afterV3 = afterV2.Replace("ClassData(typeof(TestData))", "{|xUnit1050:ClassData(typeof(TestData))|}");

		await Verify.VerifyCodeFixV2(before, afterV2, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
		await Verify.VerifyCodeFixV3(before, afterV3, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}
}
