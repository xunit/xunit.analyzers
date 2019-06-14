namespace Xunit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Testing;
    using Verify = CSharpVerifier<MemberDataShouldReferenceValidMember>;

    public class MemberDataShouldReferenceValidMemberTests
    {
        static readonly string sharedCode =
        @"public partial class TestClass { public static System.Collections.Generic.IEnumerable<object[]> Data { get;set; } }
public class OtherClass { public static System.Collections.Generic.IEnumerable<object[]> OtherData { get;set; } }
";

        [Fact]
        public async void DoesNotFindError_ForNameofOnSameClass()
        {
            var source =
                "public partial class TestClass { [Xunit.MemberData(nameof(Data))] public void TestMethod() { } }";

            await new Verify.Test
            {
                TestState = { Sources = { sharedCode, source } }
            }.RunAsync();
        }

        [Fact]
        public async void DoesNotFindError_ForNameofOnOtherClass()
        {
            var source =
                "public partial class TestClass { [Xunit.MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))] public void TestMethod() { } }";

            await new Verify.Test
            {
                TestState = { Sources = { sharedCode, source } }
            }.RunAsync();
        }

        [Fact]
        public async void FindsError_ForStringReferenceOnSameClass()
        {
            var source =
                "public partial class TestClass { [Xunit.MemberData(\"Data\")] public void TestMethod() { } }";

            await new Verify.Test
            {
                TestState =
                {
                    Sources = {  source, sharedCode },
                    ExpectedDiagnostics = { Verify.Diagnostic("xUnit1014").WithSpan(1, 52, 1, 58).WithArguments("Data", "TestClass") },
                },
            }.RunAsync();
        }

        [Fact]
        public async void FindsError_ForStringReferenceOnOtherClass()
        {
            var source =
                "public partial class TestClass { [Xunit.MemberData(\"OtherData\", MemberType = typeof(OtherClass))] public void TestMethod() { } }";

            await new Verify.Test
            {
                TestState =
                {
                    Sources = {  source, sharedCode },
                    ExpectedDiagnostics = { Verify.Diagnostic("xUnit1014").WithSpan(1, 52, 1, 63).WithArguments("OtherData", "OtherClass") },
                },
            }.RunAsync();
        }

        [Fact]
        public async void FindsError_ForInvalidNameString()
        {
            var source =
                "public class TestClass {" +
                "   [Xunit.MemberData(\"BogusName\")] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1015").WithSpan(1, 29, 1, 58).WithSeverity(DiagnosticSeverity.Error).WithArguments("BogusName", "TestClass");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void FindsError_ForInvalidNameString_UsingMemberType()
        {
            var source =
                "public class TestClass {" +
                "   [Xunit.MemberData(\"BogusName\", MemberType = typeof(TestClass))] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1015").WithSpan(1, 29, 1, 90).WithSeverity(DiagnosticSeverity.Error).WithArguments("BogusName", "TestClass");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void FindsError_ForInvalidNameString_UsingMemberTypeWithOtherType()
        {
            await new Verify.Test
            {
                TestState =
                {
                    Sources =
                    {
                        "public class TestClass {" +
                        "   [Xunit.MemberData(\"BogusName\", MemberType = typeof(OtherClass))] public void TestMethod() { }" +
                        "}",
                        "public class OtherClass {}",
                    },
                },
                ExpectedDiagnostics =
                {
                    Verify.Diagnostic("xUnit1015").WithSpan(1, 29, 1, 91).WithSeverity(DiagnosticSeverity.Error).WithArguments("BogusName", "OtherClass"),
                },
            }.RunAsync();
        }

        [Fact]
        public async void FindsError_ForValidNameofExpression_UsingMemberTypeSpecifyingOtherType()
        {
            await new Verify.Test
            {
                TestState =
                {
                    Sources =
                    {
                        "public class TestClass {" +
                        "   [Xunit.MemberData(nameof(TestClass.TestMethod), MemberType = typeof(OtherClass))] public void TestMethod() { }" +
                        "}",
                        "public class OtherClass {}",
                    },
                },
                ExpectedDiagnostics =
                {
                    Verify.Diagnostic("xUnit1015").WithSpan(1, 29, 1, 108).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "OtherClass"),
                },
            }.RunAsync();
        }

        [Theory]
        [InlineData("")]
        [InlineData("private")]
        [InlineData("protected")]
        [InlineData("internal")]
        [InlineData("protected internal")]
        public async void FindsError_ForNonPublicMember(string accessModifier)
        {
            var source =
                "public class TestClass {" +
                $"  {accessModifier} static System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1016").WithSpan(1, 100 + accessModifier.Length, 1, 130 + accessModifier.Length).WithSeverity(DiagnosticSeverity.Error);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void DoesNotFindError_ForPublicMember()
        {
            var source =
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("{|xUnit1014:\"Data\"|}")]
        [InlineData("DataNameConst")]
        [InlineData("DataNameofConst")]
        [InlineData("nameof(Data)")]
        [InlineData("nameof(TestClass.Data)")]
        [InlineData("OtherClass.Data")]
        [InlineData("nameof(OtherClass.Data)")]
        public async void FindsError_ForNameExpressions(string dataNameExpression)
        {
            TestFileMarkupParser.GetPositionsAndSpans(dataNameExpression, out var parsedDataNameExpression, out _, out _);
            var dataNameExpressionLength = parsedDataNameExpression.Length;

            await new Verify.Test
            {
                TestState =
                {
                    Sources =
                    {
                        "public class TestClass {" +
                        "   const string DataNameConst = \"Data\";" +
                        "   const string DataNameofConst = nameof(Data);" +
                        "  private static System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                        "   [Xunit.MemberData(" + dataNameExpression + ")] public void TestMethod() { }" +
                        "}",
                        "public static class OtherClass { public const string Data = \"Data\"; }",
                    },
                },
                ExpectedDiagnostics =
                {
                    Verify.Diagnostic("xUnit1016").WithSpan(1, 193, 1, 211 + dataNameExpressionLength).WithSeverity(DiagnosticSeverity.Error),
                },
            }.RunAsync();
        }

        [Fact]
        public async void FindsError_ForInstanceMember()
        {
            var source =
                "public class TestClass {" +
                "   public System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1017").WithSpan(1, 100, 1, 130).WithSeverity(DiagnosticSeverity.Error);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void DoesNotFindError_ForStaticMember()
        {
            var source =
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("public delegate System.Collections.Generic.IEnumerable<object[]> Data();")]
        [InlineData("public static class Data { }")]
        [InlineData("public static event System.EventHandler Data;")]
        public async void FindsError_ForInvalidMemberKind(string member)
        {
            var source =
                "public class TestClass {" +
                member +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1018").WithSpan(1, 29 + member.Length, 1, 59 + member.Length).WithSeverity(DiagnosticSeverity.Error);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data;")]
        [InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data { get; set; }")]
        [InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data() { return null; }")]
        public async void DoesNotFindError_ForValidMemberKind(string member)
        {
            var source =
                "public class TestClass {" +
                member +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("System.Collections.Generic.IEnumerable<object>")]
        [InlineData("object[]")]
        [InlineData("object")]
        [InlineData("System.Tuple<string, int>")]
        [InlineData("System.Tuple<string, int>[]")]
        public async void FindsError_ForInvalidMemberType(string memberType)
        {
            var source =
                "public class TestClass {" +
                $"public static {memberType} Data;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1019").WithSpan(1, 49 + memberType.Length, 1, 79 + memberType.Length).WithSeverity(DiagnosticSeverity.Error).WithArguments("System.Collections.Generic.IEnumerable<object[]>", memberType);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("System.Collections.Generic.IEnumerable<object[]>")]
        [InlineData("System.Collections.Generic.List<object[]>")]
        [InlineData("Xunit.TheoryData<int>")]
        public async void DoesNotFindError_ForCompatibleMemberType(string memberType)
        {
            var source =
                "public class TestClass {" +
                $"public static {memberType} Data;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsError_ForMemberPropertyWithoutGetter()
        {
            var source =
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data { set { } }" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1020").WithSpan(1, 111, 1, 141).WithSeverity(DiagnosticSeverity.Error);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("'a', 123")]
        [InlineData("new object[] { 'a', 123 }")]
        [InlineData("parameters: new object[] { 'a', 123 }")]
        public async void FindsWarning_ForMemberDataParametersForFieldMember(string paramsArgument)
        {
            var source =
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data;" +
                "   [Xunit.MemberData(nameof(Data), " + paramsArgument + ", MemberType = typeof(TestClass))] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1021").WithSpan(1, 131, 1, 131 + paramsArgument.Length).WithSeverity(DiagnosticSeverity.Warning);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("'a', 123")]
        [InlineData("new object[] { 'a', 123 }")]
        [InlineData("parameters: new object[] { 'a', 123 }")]
        public async void FindsWarning_ForMemberDataParametersForPropertyMember(string paramsArgument)
        {
            var source =
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data { get; set; }" +
                "   [Xunit.MemberData(nameof(Data), " + paramsArgument + ", MemberType = typeof(TestClass))] public void TestMethod() { }" +
                "}";

            var expected = Verify.Diagnostic("xUnit1021").WithSpan(1, 144, 1, 144 + paramsArgument.Length).WithSeverity(DiagnosticSeverity.Warning);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void DoesNotFindWarning_ForMemberDataAttributeWithNamedParameter()
        {
            var source =
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data;" +
                "   [Xunit.MemberData(nameof(Data), MemberType = typeof(TestClass))] public void TestMethod() { }" +
                "}";

            await Verify.VerifyAnalyzerAsync(source);
        }
    }
}
