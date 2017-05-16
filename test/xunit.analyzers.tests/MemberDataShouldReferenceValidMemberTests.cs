using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public class MemberDataShouldReferenceValidMemberTests
    {
        readonly DiagnosticAnalyzer analyzer = new MemberDataShouldReferenceValidMember();
        static readonly string sharedCode =
        @"public partial class TestClass { public static System.Collections.Generic.IEnumerable<object[]> Data { get;set; } }
public class OtherClass { public static System.Collections.Generic.IEnumerable<object[]> OtherData { get;set; } }
";

        [Fact]
        public async void DoesNotFindError_ForNameofOnSameClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                sharedCode,
                "public partial class TestClass { [Xunit.MemberData(nameof(Data))] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotFindError_ForNameofOnOtherClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                sharedCode,
                "public partial class TestClass { [Xunit.MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void FindsError_ForStringReferenceOnSameClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                sharedCode,
                "public partial class TestClass { [Xunit.MemberData(\"Data\")] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("MemberData should use nameof operator to reference member 'Data' on type 'TestClass'.", d.GetMessage());
                    Assert.Equal("xUnit1014", d.Descriptor.Id);
                });
        }

        [Fact]
        public async void FindsError_ForStringReferenceOnOtherClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                sharedCode,
                "public partial class TestClass { [Xunit.MemberData(\"OtherData\", MemberType = typeof(OtherClass))] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("MemberData should use nameof operator to reference member 'OtherData' on type 'OtherClass'.", d.GetMessage());
                    Assert.Equal("xUnit1014", d.Descriptor.Id);
                });
        }

        [Fact]
        public async void FindsError_ForInvalidNameString()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   [Xunit.MemberData(\"BogusName\")] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("MemberData must reference an existing member 'BogusName' on type 'TestClass'.", d.GetMessage());
                    Assert.Equal("xUnit1015", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Fact]
        public async void FindsError_ForInvalidNameString_UsingMemberType()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   [Xunit.MemberData(\"BogusName\", MemberType = typeof(TestClass))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("MemberData must reference an existing member 'BogusName' on type 'TestClass'.", d.GetMessage());
                    Assert.Equal("xUnit1015", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Fact]
        public async void FindsError_ForInvalidNameString_UsingMemberTypeWithOtherType()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class OtherClass {}",
                "public class TestClass {" +
                "   [Xunit.MemberData(\"BogusName\", MemberType = typeof(OtherClass))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("MemberData must reference an existing member 'BogusName' on type 'OtherClass'.", d.GetMessage());
                    Assert.Equal("xUnit1015", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Fact]
        public async void FindsError_ForValidNameofExpression_UsingMemberTypeSpecifyingOtherType()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class OtherClass {}",
                "public class TestClass {" +
                "   [Xunit.MemberData(nameof(TestClass.TestMethod), MemberType = typeof(OtherClass))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("MemberData must reference an existing member 'TestMethod' on type 'OtherClass'.", d.GetMessage());
                    Assert.Equal("xUnit1015", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Theory]
        [InlineData("")]
        [InlineData("private")]
        [InlineData("protected")]
        [InlineData("internal")]
        [InlineData("protected internal")]
        public async void FindsError_ForNonPublicMember(string accessModifier)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                $"  {accessModifier} static System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("MemberData must reference a public member", d.GetMessage());
                    Assert.Equal("xUnit1016", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Fact]
        public async void DoesNotFindError_ForPublicMember()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("\"Data\"")]
        [InlineData("DataNameConst")]
        [InlineData("DataNameofConst")]
        [InlineData("nameof(Data)")]
        [InlineData("nameof(TestClass.Data)")]
        [InlineData("OtherClass.Data")]
        [InlineData("nameof(OtherClass.Data)")]
        public async void FindsError_ForNameExpressions(string dataNameExpression)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public static class OtherClass { public const string Data = \"Data\"; }",
                "public class TestClass {" +
                "   const string DataNameConst = \"Data\";" +
                "   const string DataNameofConst = nameof(Data);" +
                $"  private static System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(" + dataNameExpression + ")] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics.Where(d => d.Id != "xUnit1014"),
                d =>
                {
                    Assert.Equal("MemberData must reference a public member", d.GetMessage());
                    Assert.Equal("xUnit1016", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Fact]
        public async void FindsError_ForInstanceMember()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   public System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("MemberData must reference a static member", d.GetMessage());
                    Assert.Equal("xUnit1017", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Fact]
        public async void DoesNotFindError_ForStaticMember()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data = null;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("public delegate System.Collections.Generic.IEnumerable<object[]> Data();")]
        [InlineData("public static class Data { }")]
        [InlineData("public static event System.EventHandler Data;")]
        public async void FindsError_ForInvalidMemberKind(string member)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                member +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("xUnit1018", d.Descriptor.Id);
                    Assert.Equal("MemberData must reference a property, field, or method", d.GetMessage());
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Theory]
        [InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data;")]
        [InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data { get; set; }")]
        [InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data() { return null; }")]
        public async void DoesNotFindError_ForValidMemberKind(string member)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                member +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("System.Collections.Generic.IEnumerable<object>")]
        [InlineData("object[]")]
        [InlineData("object")]
        [InlineData("System.Tuple<string, int>")]
        [InlineData("System.Tuple<string, int>[]")]
        public async void FindsError_ForInvalidMemberType(string memberType)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                $"public static {memberType} Data;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("xUnit1019", d.Descriptor.Id);
                    Assert.Equal("MemberData must reference a data type assignable to 'System.Collections.Generic.IEnumerable<object[]>'. The referenced type '" + memberType + "' is not valid.", d.GetMessage());
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Theory]
        [InlineData("System.Collections.Generic.IEnumerable<object[]>")]
        [InlineData("System.Collections.Generic.List<object[]>")]
        [InlineData("Xunit.TheoryData<int>")]
        public async void DoesNotFindError_ForCompatibleMemberType(string memberType)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                $"public static {memberType} Data;" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void FindsError_ForMemberPropertyWithoutGetter()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data { set { } }" +
                "   [Xunit.MemberData(nameof(Data))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("xUnit1020", d.Descriptor.Id);
                    Assert.Equal("MemberData must reference a property with a getter", d.GetMessage());
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Theory]
        [InlineData("'a', 123")]
        [InlineData("new object[] { 'a', 123 }")]
        [InlineData("parameters: new object[] { 'a', 123 }")]
        public async void FindsWarning_ForMemberDataParametersForFieldMember(string paramsArgument)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data;" +
                "   [Xunit.MemberData(nameof(Data), " + paramsArgument + ", MemberType = typeof(TestClass))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("xUnit1021", d.Descriptor.Id);
                    Assert.Equal("MemberData should not have parameters if the referenced member is not a method", d.GetMessage());
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
        }

        [Theory]
        [InlineData("'a', 123")]
        [InlineData("new object[] { 'a', 123 }")]
        [InlineData("parameters: new object[] { 'a', 123 }")]
        public async void FindsWarning_ForMemberDataParametersForPropertyMember(string paramsArgument)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data { get; set; }" +
                "   [Xunit.MemberData(nameof(Data), " + paramsArgument + ", MemberType = typeof(TestClass))] public void TestMethod() { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("xUnit1021", d.Descriptor.Id);
                    Assert.Equal("MemberData should not have parameters if the referenced member is not a method", d.GetMessage());
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
        }

        [Fact]
        public async void DoesNotFindWarning_ForMemberDataAttributeWithNamedParameter()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass {" +
                "   public static System.Collections.Generic.IEnumerable<object[]> Data;" +
                "   [Xunit.MemberData(nameof(Data), MemberType = typeof(TestClass))] public void TestMethod() { }" +
                "}");

            Assert.Empty(diagnostics);
        }
    }
}

