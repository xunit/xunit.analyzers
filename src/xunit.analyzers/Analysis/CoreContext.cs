using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public class CoreContext
    {
        readonly Compilation compilation;

        public CoreContext(Compilation compilation)
            => this.compilation = compilation;

        public INamedTypeSymbol FactAttributeType => compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute);
        public INamedTypeSymbol TheoryAttributeType => compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
        public INamedTypeSymbol DataAttributeType => compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute);
        public INamedTypeSymbol InlineDataAttributeType => compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute);
        public INamedTypeSymbol ClassDataAttributeType => compilation.GetTypeByMetadataName(Constants.Types.XunitClassDataAttribute);
        public INamedTypeSymbol MemberDataAttributeType => compilation.GetTypeByMetadataName(Constants.Types.XunitMemberDataAttribute);
    }
}
