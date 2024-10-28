using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Class which provides information about references to xUnit.net assemblies.
/// </summary>
public class XunitContext
{
	IAssertContext? assert;
	ICommonContext? common;
	ICoreContext? core;
	IRunnerUtilityContext? runnerUtility;
	static readonly Version v2AbstractionsVersion = new(2, 0, 3);

	XunitContext()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitContext"/> class.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation object used to look up types and
	/// inspect references</param>
	public XunitContext(Compilation compilation)
	{
		V2Abstractions = V2AbstractionsContext.Get(compilation);
		V2Assert = V2AssertContext.Get(compilation);
		V2Core = V2CoreContext.Get(compilation);
		V2Execution = V2ExecutionContext.Get(compilation);
		V2RunnerUtility = V2RunnerUtilityContext.Get(compilation);

		V3Assert = V3AssertContext.Get(compilation);
		V3Common = V3CommonContext.Get(compilation);
		V3Core = V3CoreContext.Get(compilation);
		V3RunnerCommon = V3RunnerCommonContext.Get(compilation);
	}

	/// <summary>
	/// Gets a combined view of the assertion library features available to either v2 tests (linked
	/// against <c>xunit.assert</c> or <c>xunit.assert.source</c>) or v3 tests (linked against
	/// <c>xunit.v3.assert</c> or <c>xunit.v3.assert.source</c>).
	/// </summary>
	public IAssertContext Assert
	{
		get
		{
			assert ??= V3Assert ?? (IAssertContext?)V2Assert ?? EmptyAssertContext.Instance;
			return assert;
		}
	}

	/// <summary>
	/// Gets a combined view of features that are common to both tests and runners, available to either
	/// v2 tests (linked against <c>xunit.abstractions</c>) or v3 tests (linked against <c>xunit.v3.common</c>).
	/// </summary>
	public ICommonContext Common
	{
		get
		{
			common ??= V3Common ?? (ICommonContext?)V2Abstractions ?? EmptyCommonContext.Instance;
			return common;
		}
	}

	/// <summary>
	/// Gets a combined view of features available to either v2 tests (linked against <c>xunit.core</c>)
	/// or v3 tests (linked against <c>xunit.v3.core</c>).
	/// </summary>
	public ICoreContext Core
	{
		get
		{
			core ??= V3Core ?? (ICoreContext?)V2Core ?? EmptyCoreContext.Instance;
			return core;
		}
	}

	/// <summary>
	/// Gets a flag which indicates whether there are any xUnit.net v2 references in the project
	/// (including abstractions, assert, core, execution, and runner utility references).
	/// </summary>
	public bool HasV2References =>
		V2Abstractions is not null || V2Assert is not null || V2Core is not null || V2Execution is not null || V2RunnerUtility is not null;

	/// <summary>
	/// Gets a flag which indicates whether there are any xUnit.net v3 references in the project
	/// (including assert, common, and core references).
	/// </summary>
	public bool HasV3References =>
		V3Assert is not null || V3Common is not null || V3Core is not null;

	/// <summary>
	/// Gets a combined view of features available to either v2 runners (linked against <c>xunit.runner.utility</c>)
	/// or v3 runners (linked against <c>xunit.v3.runner.utility</c>).
	/// </summary>
	public IRunnerUtilityContext RunnerUtility
	{
		get
		{
			runnerUtility ??= V3RunnerUtility ?? (IRunnerUtilityContext?)V2RunnerUtility ?? EmptyRunnerUtilityContext.Instance;
			return runnerUtility;
		}
	}

	/// <summary>
	/// Gets information about the reference to <c>xunit.abstractions</c> (v2). If the project does
	/// not reference the v2 abstractions library, then returns <c>null</c>.
	/// </summary>
	public V2AbstractionsContext? V2Abstractions { get; private set; }

	/// <summary>
	/// Gets information about the reference to <c>xunit.assert</c> or <c>xunit.assert.source</c> (v2). If
	/// the project does not reference the v2 assertion library, then returns <c>null</c>.
	/// </summary>
	public V2AssertContext? V2Assert { get; private set; }

	/// <summary>
	/// Gets information about the reference to <c>xunit.core</c>(v2). If the project does not
	/// reference the v2 core library, then returns <c>null</c>.
	/// </summary>
	public V2CoreContext? V2Core { get; private set; }

	/// <summary>
	/// Gets information about the reference to <c>xunit.execution</c> (v2). If the project does
	/// not reference the v2 execution library, then returns <c>null</c>.
	/// </summary>
	public V2ExecutionContext? V2Execution { get; private set; }

	/// <summary>
	/// Gets information about the reference to <c>xunit.runner.utility</c> (v2). If the project does
	/// not reference the v2 runner utility library, then returns <c>null</c>.
	/// </summary>
	public V2RunnerUtilityContext? V2RunnerUtility { get; private set; }

	/// <summary>
	/// Gets information about the reference to <c>xunit.v3.assert</c> or <c>xunit.v3.assert.source</c>
	/// (v3). If the project does not reference the v3 assertion library, then returns <c>null</c>.
	/// </summary>
	public V3AssertContext? V3Assert { get; private set; }

	/// <summary>
	/// Gets types that exist in <c>xunit.v3.common</c> (v3). If the project does not reference
	/// the v3 common library, then returns <c>null</c>.
	/// </summary>
	/// <remarks>
	/// This also contains a few selected types from <c>xunit.v3.core</c> and <c>xunit.v3.runner.common</c>
	/// to align with the types that were all originally in <c>xunit.abstractions</c> in v2, to support
	/// the composite view from <see cref="ICommonContext"/>.
	/// </remarks>
	public V3CommonContext? V3Common { get; private set; }

	/// <summary>
	/// Gets information about the reference to <c>xunit.v3.core</c> (v3). If the project does not
	/// reference the v3 core library, then returns <c>null</c>.
	/// </summary>
	public V3CoreContext? V3Core { get; private set; }

	/// <summary>
	/// Gets information about the reference to <c>xunit.v3.runner.common</c> (v3). If the project does
	/// not reference the v3 runner common library, then returns <c>null</c>.
	/// </summary>
	public V3RunnerCommonContext? V3RunnerCommon { get; private set; }

	/// <summary>
	/// Gets information about the reference to <c>xunit.v3.runner.utility</c> (v3). If the project does
	/// not reference the v3 runner utility library, then returns <c>null</c>.
	/// </summary>
	public V3RunnerUtilityContext? V3RunnerUtility { get; private set; }

	/// <summary>
	/// Used to create a context object for testing v2 analyzers and fixers. This includes references
	/// to <c>xunit.abstactions</c> (at version 2.0.3).
	/// <param name="compilation">The Roslyn compilation object used to look up types</param>
	public static XunitContext ForV2Abstractions(Compilation compilation) =>
		new()
		{
			V2Abstractions = V2AbstractionsContext.Get(compilation, v2AbstractionsVersion),
		};

	/// <summary>
	/// Used to create a context object for testing v2 analyzers and fixers. This includes references
	/// to <c>xunit.assert</c>.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation object used to look up types</param>
	/// <param name="versionOverride">The overridden version for <c>xunit.assert</c></param>
	public static XunitContext ForV2Assert(
		Compilation compilation,
		Version? versionOverride = null) =>
			new()
			{
				V2Assert = V2AssertContext.Get(compilation, versionOverride),
			};

	/// <summary>
	/// Used to create a context object for testing v2 analyzers and fixers. This includes references
	/// to <c>xunit.abstractions</c> (at version 2.0.3) and <c>xunit.core</c>.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation object used to look up types</param>
	/// <param name="versionOverride">The overridden version for <c>xunit.core</c></param>
	public static XunitContext ForV2Core(
		Compilation compilation,
		Version? versionOverride = null) =>
			new()
			{
				V2Abstractions = V2AbstractionsContext.Get(compilation, v2AbstractionsVersion),
				V2Core = V2CoreContext.Get(compilation, versionOverride),
			};

	/// <summary>
	/// Used to create a context object for testing v2 analyzers and fixers. This includes references
	/// to <c>xunit.abstractions</c> (at version 2.0.3) and <c>xunit.execution</c>.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation object used to look up types</param>
	/// <param name="versionOverride">The overridden version for <c>xunit.execution</c></param>
	public static XunitContext ForV2Execution(
		Compilation compilation,
		Version? versionOverride = null) =>
			new()
			{
				V2Abstractions = V2AbstractionsContext.Get(compilation, v2AbstractionsVersion),
				V2Execution = V2ExecutionContext.Get(compilation, versionOverride),
			};

	/// <summary>
	/// Used to create a context object for testing v2 analyzers and fixers. This includes references
	/// to <c>xunit.abstractions</c> (at version 2.0.3) and <c>xunit.runner.utility</c>.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation object used to look up types</param>
	/// <param name="versionOverride">The overridden version for <c>xunit.runner.utility</c></param>
	public static XunitContext ForV2RunnerUtility(
		Compilation compilation,
		Version? versionOverride = null) =>
			new()
			{
				V2Abstractions = V2AbstractionsContext.Get(compilation, v2AbstractionsVersion),
				V2RunnerUtility = V2RunnerUtilityContext.Get(compilation, versionOverride),
			};

	/// <summary>
	/// Used to create a context object for testing v3 analyzers and fixers. This includes references
	/// for <c>xunit.v3.assert</c>, <c>xunit.v3.common</c>, <c>xunit.v3.core</c>, and
	/// <c>xunit.v3.runner.common</c>.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation object used to look up types</param>
	/// <param name="versionOverride">The overridden version for all libraries</param>
	public static XunitContext ForV3(
		Compilation compilation,
		Version? versionOverride = null) =>
			new()
			{
				V3Assert = V3AssertContext.Get(compilation, versionOverride),
				V3Common = V3CommonContext.Get(compilation, versionOverride),
				V3Core = V3CoreContext.Get(compilation, versionOverride),
				V3RunnerCommon = V3RunnerCommonContext.Get(compilation, versionOverride),
			};

	/// <summary>
	/// Used to create a context object for testing v3 analyzers and fixers. This includes references
	/// for <c>xunit.v3.assert</c>.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation object used to look up types</param>
	/// <param name="versionOverride">The overridden version for <c>xunit.v3.assert</c></param>
	public static XunitContext ForV3Assert(
		Compilation compilation,
		Version? versionOverride = null) =>
			new()
			{
				V3Assert = V3AssertContext.Get(compilation, versionOverride),
			};
}
