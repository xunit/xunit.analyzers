using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class ThreadBlockingCodeInTestContextTests
    {
        readonly DiagnosticAnalyzer analyzer = new ThreadBlockingCodeInTestContext();
        
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
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);
            
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("xUnit1027", d.Id);
                Assert.Equal("'new Task(() => {}).Wait' invocation in method 'T' may " +
                             "lead to a deadlock. Consider using an async test method.", d.GetMessage());
            });
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
    public int T()
    {
        return new Task<int>(() => 1).Result;
    }
}";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("xUnit1027", d.Id);
                Assert.Equal("'new Task<int>(() => 1).Result' invocation in method 'T' may " +
                             "lead to a deadlock. Consider using an async test method.", d.GetMessage());
            });
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
    public int T()
    {
        return Helper<int>();
    }

    private T Helper<T>()
    {
        return new Task<T>(() => default(T)).Result;
    }
}";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("xUnit1027", d.Id);
                Assert.Equal("'new Task<T>(() => default(T)).Result' invocation in method 'Helper' may " +
                             "lead to a deadlock. Consider using an async test method.", d.GetMessage());
            });
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
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);
            
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("xUnit1027", d.Id);
                Assert.Equal("'new Task(() => {}).GetAwaiter' invocation in method 'T' may " +
                             "lead to a deadlock. Consider using an async test method.", d.GetMessage());
            });
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
    public int T()
    {
        return new Task<int>(() => 1).GetAwaiter().GetResult();
    }
}";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);
            
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("xUnit1027", d.Id);
                Assert.Equal("'new Task<int>(() => 1).GetAwaiter' invocation in method 'T' may " +
                             "lead to a deadlock. Consider using an async test method.", d.GetMessage());
            });
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
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);
            
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("xUnit1027", d.Id);
                Assert.Equal("'Task.WaitAll' invocation in method 'T' may " +
                             "lead to a deadlock. Consider using an async test method.", d.GetMessage());
            });
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
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);
            
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("xUnit1027", d.Id);
                Assert.Equal("'Task.WaitAny' invocation in method 'T' may " +
                             "lead to a deadlock. Consider using an async test method.", d.GetMessage());
            });
        }
    }
}
