using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.ThreadBlockingCodeInTestContext>;

namespace Xunit.Analyzers
{
    public class ThreadBlockingCodeInTestContextTests
    {
        [Fact]
        public async Task WaitInvocation_InSyncTest_Reports()
        {
            var source = @"
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public void T()
    {
        new Task(() => {}).Wait();
    }
}";
            var expected = Verify.Diagnostic("xUnit1027")
                                 .WithSpan(10, 9, 10, 32)
                                 .WithSeverity(DiagnosticSeverity.Warning)
                                 .WithArguments("new Task(() => {}).Wait", "T");

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task ResultInvocation_InSyncTest_Reports()
        {
            var source = @"
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public void T()
    {
        var unused = new Task<int>(() => 1).Result;
    }
}";
            var expected = Verify.Diagnostic("xUnit1027")
                                 .WithSpan(10, 22, 10, 51)
                                 .WithSeverity(DiagnosticSeverity.Warning)
                                 .WithArguments("new Task<int>(() => 1).Result", "T");

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task ResultGenericInvocation_InSyncTest_Reports()
        {
            var source = @"
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public void T()
    {
        var unused = Helper<int>();
    }

    private T Helper<T>()
    {
        return new Task<T>(() => default(T)).Result;
    }
}";
            var expected = Verify.Diagnostic("xUnit1027")
                                 .WithSpan(15, 16, 15, 52)
                                 .WithSeverity(DiagnosticSeverity.Warning)
                                 .WithArguments("new Task<T>(() => default(T)).Result", "Helper");

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task GetAwaiterInvocation_InSyncTest_Reports()
        {
            var source = @"
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public void T()
    {
        new Task(() => {}).GetAwaiter();
    }
}";
            var expected = Verify.Diagnostic("xUnit1027")
                                 .WithSpan(10, 9, 10, 38)
                                 .WithSeverity(DiagnosticSeverity.Warning)
                                 .WithArguments("new Task(() => {}).GetAwaiter", "T");

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task GetAwaiterGenericInvocation_InSyncTest_Reports()
        {
            var source = @"
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public void T()
    {
        var unused = new Task<int>(() => 1).GetAwaiter().GetResult();
    }
}";
            var expected = Verify.Diagnostic("xUnit1027")
                                 .WithSpan(10, 22, 10, 55)
                                 .WithSeverity(DiagnosticSeverity.Warning)
                                 .WithArguments("new Task<int>(() => 1).GetAwaiter", "T");

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task TaskWaitAllInvocation_InSyncTest_Reports()
        {
            var source = @"
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public void T()
    {
        Task.WaitAll(new Task(() => {}));
    }
}";
            var expected = Verify.Diagnostic("xUnit1027")
                                 .WithSpan(10, 9, 10, 21)
                                 .WithSeverity(DiagnosticSeverity.Warning)
                                 .WithArguments("Task.WaitAll", "T");

            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task TaskWaitAnyInvocation_InSyncTest_Reports()
        {
            var source = @"
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public void T()
    {
        Task.WaitAny(new Task(() => {}));
    }
}";
            var expected = Verify.Diagnostic("xUnit1027")
                                 .WithSpan(10, 9, 10, 21)
                                 .WithSeverity(DiagnosticSeverity.Warning)
                                 .WithArguments("Task.WaitAny", "T");

            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
