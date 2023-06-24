using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class ClassDataAttributeMustPointAtValidClassFixerTests
{
	[Fact]
	public async void AddsIEnumerable()
	{
		var before = @"
using System.Collections.Generic;
using Xunit;

public class TestData {
}

public class TestClass {
    [Theory]
    [ClassData(typeof([|TestData|]))]
    public void TestMethod(int _) { }
}";

		var after = @"
using System.Collections.Generic;
using Xunit;

public class TestData : {|CS0535:{|CS0535:IEnumerable<object[]>|}|}
{
}

public class TestClass {
    [Theory]
    [ClassData(typeof(TestData))]
    public void TestMethod(int _) { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}

	[Fact]
	public async void ConvertsParameterlessConstructorToPublic()
	{
		var before = @"
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
    [ClassData(typeof([|TestData|]))]
    public void TestMethod(int _) { }
}";

		var after = @"
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
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}

	[Fact]
	public async void AddsPublicParameterlessConstructor()
	{
		var before = @"
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
    [ClassData(typeof([|TestData|]))]
    public void TestMethod(int _) { }
}";

		var after = @"
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
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}

	[Fact]
	public async void RemovesAbstractModifierFromDataClass()
	{
		var before = @"
using System.Collections;
using System.Collections.Generic;
using Xunit;

public abstract class TestData : IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}

public class TestClass {
    [Theory]
    [ClassData(typeof([|TestData|]))]
    public void TestMethod(int _) { }
}";

		var after = @"
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
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, ClassDataAttributeMustPointAtValidClassFixer.Key_FixDataClass);
	}
}
