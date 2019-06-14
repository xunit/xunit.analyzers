using System.Collections.Generic;
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
            SystemThreadingTasksReference = GetAssemblyReference(referencedAssemblies, "System.Threading.Tasks");
        }

        static MetadataReference GetAssemblyReference(IEnumerable<AssemblyName> assemblies, string name)
        {
            return MetadataReference.CreateFromFile(Assembly.Load(assemblies.First(n => n.Name == name)).Location);
        }
    }
}
