using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

namespace Xunit.Analyzers
{
    public class ClassDataAttributeMustPointAtValidClassTests
    {
        private static readonly string TestMethodSource = "public class TestClass { [Xunit.Theory][Xunit.ClassData(typeof(DataClass))] public void TestMethod() { } }";

        [Fact]
        public async void DoesNotFindErrorForFactMethod()
        {
            var source =
@"class DataClass : System.Collections.Generic.IEnumerable<object[]> {
    public System.Collections.Generic.IEnumerator<object[]> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}";

            await new Verify.Test
            {
                TestState = { Sources = { TestMethodSource, source } },
            }.RunAsync();
        }

        [Fact]
        public async void FindsErrorForDataClassNotImplementingInterface()
        {
            var source =
@"class DataClass : System.Collections.Generic.IEnumerable<object> {
    public System.Collections.Generic.IEnumerator<object> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}";

            await new Verify.Test
            {
                TestState =
                {
                    Sources = { TestMethodSource, source },
                    ExpectedDiagnostics = { Verify.Diagnostic().WithSpan(1, 64, 1, 73) },
                },
            }.RunAsync();
        }

        [Fact]
        public async void FindsErrorForAbstractDataClass()
        {
            var source =
@"abstract class DataClass : System.Collections.Generic.IEnumerable<object[]> {
    public DataClass() {}
    public System.Collections.Generic.IEnumerator<object[]> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}";

            await new Verify.Test
            {
                TestState =
                {
                    Sources = { TestMethodSource, source },
                    ExpectedDiagnostics = { Verify.Diagnostic().WithSpan(1, 64, 1, 73) },
                },
            }.RunAsync();
        }

        [Fact]
        public async void FindsErrorForDataClassWithImplicitPrivateConstructor()
        {
            var source =
@"class DataClass : System.Collections.Generic.IEnumerable<object[]> {
    public DataClass(string parameter) {}
    public System.Collections.Generic.IEnumerator<object[]> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}";

            await new Verify.Test
            {
                TestState =
                {
                    Sources = { TestMethodSource, source },
                    ExpectedDiagnostics = { Verify.Diagnostic().WithSpan(1, 64, 1, 73) },
                },
            }.RunAsync();
        }

        [Theory]
        [InlineData("private")]
        [InlineData("internal")]
        public async void FindsErrorForDataClassWithExplicitNonPublicConstructor(string accessibility)
        {
            var source =
string.Format(@"class DataClass : System.Collections.Generic.IEnumerable<object[]> {{
    {0} DataClass() {{}}
    public System.Collections.Generic.IEnumerator<object[]> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}}", accessibility);
            await new Verify.Test
            {
                TestState =
                {
                    Sources = { TestMethodSource, source },
                    ExpectedDiagnostics = { Verify.Diagnostic().WithSpan(1, 64, 1, 73) },
                },
            }.RunAsync();
        }
    }
}
