using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public class ExecutionContext
    {
        readonly Compilation compilation;

        public ExecutionContext(Compilation compilation)
            => this.compilation = compilation;

        public INamedTypeSymbol LongLivedMarshalByRefObjectType => compilation.GetTypeByMetadataName(Constants.Types.XunitLongLivedMarshalByRefObject);
    }
}
