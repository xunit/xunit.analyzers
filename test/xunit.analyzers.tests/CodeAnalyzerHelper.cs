using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Analyzers
{
    class CodeAnalyzerHelper
    {
#if NETCOREAPP2_2
        internal static readonly PortableExecutableReference SystemCollectionsReference;
        internal static readonly MetadataReference SystemCollectionsNonGenericReference;
        internal static readonly MetadataReference SystemConsoleReference;
        internal static readonly MetadataReference SystemRuntimeReference;
        internal static readonly MetadataReference SystemRuntimeExtensionsReference;
        internal static readonly MetadataReference SystemTextRegularExpressionsReference;
#endif
        internal static readonly MetadataReference SystemThreadingTasksReference;
        internal static readonly MetadataReference XunitAbstractionsReference = MetadataReference.CreateFromFile(typeof(ITest).GetTypeInfo().Assembly.Location);
        internal static readonly MetadataReference XunitAssertReference = MetadataReference.CreateFromFile(typeof(Assert).GetTypeInfo().Assembly.Location);
        internal static readonly MetadataReference XunitCoreReference = MetadataReference.CreateFromFile(typeof(FactAttribute).GetTypeInfo().Assembly.Location);
        internal static readonly MetadataReference XunitExecutionReference = MetadataReference.CreateFromFile(typeof(XunitTestCase).GetTypeInfo().Assembly.Location);

        static CodeAnalyzerHelper()
        {
            // Xunit is a PCL linked against System.Runtime, however on the Desktop framework all types in that assembly have been forwarded to
            // System.Core, so we need to find the assembly by name to compile without errors.
            var referencedAssemblies = typeof(FactAttribute).Assembly.GetReferencedAssemblies();
#if NETCOREAPP2_2
            SystemCollectionsReference = GetAssemblyReference(referencedAssemblies, "System.Collections");
            SystemRuntimeReference = GetAssemblyReference(referencedAssemblies, "System.Runtime");
            SystemRuntimeExtensionsReference = GetAssemblyReference(referencedAssemblies, "System.Runtime.Extensions");
#endif

            SystemThreadingTasksReference = GetAssemblyReference(referencedAssemblies, "System.Threading.Tasks");

#if NETCOREAPP2_2
            // Xunit doesn't directly reference System.Collections.NonGeneric, so we locate it relative to
            // System.Collections.
            SystemCollectionsNonGenericReference = MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(SystemCollectionsReference.FilePath), "System.Collections.NonGeneric.dll"));
            SystemConsoleReference = MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(SystemCollectionsReference.FilePath), "System.Console.dll"));
            SystemTextRegularExpressionsReference = MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(SystemCollectionsReference.FilePath), "System.Text.RegularExpressions.dll"));
#endif
        }

        static PortableExecutableReference GetAssemblyReference(IEnumerable<AssemblyName> assemblies, string name)
        {
            return MetadataReference.CreateFromFile(Assembly.Load(assemblies.First(n => n.Name == name)).Location);
        }
    }
}
