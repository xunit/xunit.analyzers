namespace Xunit.Analyzers
{
    internal static class Constants
    {
        internal static class Categories
        {
            internal static string Usage { get; } = "Usage";
            internal static string Assertions { get; } = "Assertions";
            internal static string Extensibility { get; } = "Extensibility";
        }

        internal static class Types
        {
            internal static readonly string XunitClassDataAttribute = "Xunit.ClassDataAttribute";
            internal static readonly string XunitIAsyncLifetime = "Xunit.IAsyncLifetime";
            internal static readonly string XunitInlineDataAttribute = "Xunit.InlineDataAttribute";
            internal static readonly string XunitMemberDataAttribute = "Xunit.MemberDataAttribute";
            internal static readonly string XunitFactAttribute = "Xunit.FactAttribute";
            internal static readonly string XunitTheoryAttribute = "Xunit.TheoryAttribute";

            internal static readonly string XunitSdkDataAttribute = "Xunit.Sdk.DataAttribute";

            internal static readonly string XunitAssert = "Xunit.Assert";

            internal static readonly string SystemCollectionsICollection = "System.Collections.ICollection";
            internal static readonly string SystemThreadingTasksTask = "System.Threading.Tasks.Task";
        }
    }
}
