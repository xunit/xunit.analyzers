using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public class AbstractionsContext
    {
        readonly Compilation compilation;

        public AbstractionsContext(Compilation compilation)
            => this.compilation = compilation;

        public INamedTypeSymbol ITestCaseType => compilation.GetTypeByMetadataName(Constants.Types.XunitAbstractionsITestCase);
        public INamedTypeSymbol IXunitSerializableType => compilation.GetTypeByMetadataName(Constants.Types.XunitAbstractionsIXunitSerializableType);
    }
}
