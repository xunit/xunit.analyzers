using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class XunitContext
	{
		public XunitContext(
			Compilation compilation,
			Version versionOverride = null)
		{
			Compilation = compilation;

			Abstractions = new AbstractionsContext(compilation, versionOverride);
			Core = new CoreContext(compilation, versionOverride);
			CoreV3 = new CoreV3Context(compilation, versionOverride);
			Execution = new ExecutionContext(compilation, versionOverride);
		}

		public AbstractionsContext Abstractions { get; set; }

		public Compilation Compilation { get; set; }

		public CoreContext Core { get; set; }

		public CoreV3Context CoreV3 { get; set; }

		public ExecutionContext Execution { get; set; }

		public bool HasAbstractionsReference =>
			Abstractions.Version != null;

		public bool HasCoreReference =>
			Core.Version != null;

		public bool HasCoreV3Reference =>
			CoreV3.Version != null;

		public bool HasExecutionReference =>
			Execution.Version != null;
	}
}
