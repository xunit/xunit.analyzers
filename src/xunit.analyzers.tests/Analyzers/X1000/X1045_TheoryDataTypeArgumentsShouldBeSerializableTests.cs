using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataTypeArgumentsShouldBeSerializable>;
using Verify_v3_Pre301 = CSharpVerifier<X1045_TheoryDataTypeArgumentsShouldBeSerializableTests.Analyzer_v3_Pre301>;

public class X1045_TheoryDataTypeArgumentsShouldBeSerializableTests
{
	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using Xunit;

			// Things which might not be serializable

			public class ObjectClass {
				public sealed class Class : TheoryData<object> { }
				public static readonly TheoryData<object> Field = new TheoryData<object>() { };
				public static TheoryData<object> Method(int a, string b) => new TheoryData<object>() { };
				public static TheoryData<object> Property => new TheoryData<object>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(object parameter) { }

				[Theory]
				[{|xUnit1045:ClassData(typeof(Class))|}]
				[{|xUnit1045:MemberData(nameof(Field))|}]
				[{|xUnit1045:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1045:MemberData(nameof(Property))|}]
				public void Triggers(object parameter) { }
			}

			public class IPossiblySerializableInterfaceClass {
				public sealed class Class : TheoryData<IPossiblySerializableInterface> { }
				public static readonly TheoryData<IPossiblySerializableInterface> Field = new TheoryData<IPossiblySerializableInterface>() { };
				public static TheoryData<IPossiblySerializableInterface> Method(int a, string b) => new TheoryData<IPossiblySerializableInterface>() { };
				public static TheoryData<IPossiblySerializableInterface> Property => new TheoryData<IPossiblySerializableInterface>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(IPossiblySerializableInterface parameter) { }

				[Theory]
				[{|xUnit1045:ClassData(typeof(Class))|}]
				[{|xUnit1045:MemberData(nameof(Field))|}]
				[{|xUnit1045:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1045:MemberData(nameof(Property))|}]
				public void Triggers(IPossiblySerializableInterfaceClass parameter) { }
			}

			public class PossiblySerializableUnsealedClassClass {
				public sealed class Class : TheoryData<PossiblySerializableUnsealedClass> { }
				public static readonly TheoryData<PossiblySerializableUnsealedClass> Field = new TheoryData<PossiblySerializableUnsealedClass>() { };
				public static TheoryData<PossiblySerializableUnsealedClass> Method(int a, string b) => new TheoryData<PossiblySerializableUnsealedClass>() { };
				public static TheoryData<PossiblySerializableUnsealedClass> Property => new TheoryData<PossiblySerializableUnsealedClass>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(PossiblySerializableUnsealedClass parameter) { }

				[Theory]
				[{|xUnit1045:ClassData(typeof(Class))|}]
				[{|xUnit1045:MemberData(nameof(Field))|}]
				[{|xUnit1045:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1045:MemberData(nameof(Property))|}]
				public void Triggers(PossiblySerializableUnsealedClass parameter) { }
			}

			public interface IPossiblySerializableInterface { }

			public class PossiblySerializableUnsealedClass { }
			""";

		await Verify.VerifyAnalyzerNonAot(LanguageVersion.CSharp8, source);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT_PreTupleSupport()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class TupleClass {
				public sealed class Class : TheoryData<Tuple<string, int>> { }
				public static readonly TheoryData<Tuple<string, int>> Field = new TheoryData<Tuple<string, int>>() { };
				public static TheoryData<Tuple<string, int>> Method(int a, string b) => new TheoryData<Tuple<string, int>>() { };
				public static TheoryData<Tuple<string, int>> Property => new TheoryData<Tuple<string, int>>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(Tuple<string, int> parameter) { }

				[Theory]
				[{|xUnit1045:ClassData(typeof(Class))|}]
				[{|xUnit1045:MemberData(nameof(Field))|}]
				[{|xUnit1045:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1045:MemberData(nameof(Property))|}]
				public void Triggers(Tuple<string, int> parameter) { }
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
		await Verify_v3_Pre301.VerifyAnalyzerV3NonAot(source);
	}

	[Fact]
	public async ValueTask V2_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			// Things which might not be serializable in v2

			public class UriClass {
				public sealed class Class : TheoryData<Uri> { }
				public static readonly TheoryData<Uri> Field = new TheoryData<Uri>() { };
				public static TheoryData<Uri> Method(int a, string b) => new TheoryData<Uri>() { };
				public static TheoryData<Uri> Property => new TheoryData<Uri>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(Uri parameter) { }

				[Theory]
				[{|xUnit1045:ClassData(typeof(Class))|}]
				[{|xUnit1045:MemberData(nameof(Field))|}]
				[{|xUnit1045:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1045:MemberData(nameof(Property))|}]
				public void Triggers(Uri parameter) { }
			}
			""";

		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, source);
	}

#if NETCOREAPP && ROSLYN_LATEST

	[Fact]
	public async ValueTask V2_only_NetCore()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Diagnostics.CodeAnalysis;
			using Xunit;

			// Things which might not be serializable in v2

			public class FormattableAndParsableClass {
				public static readonly TheoryData<Formattable> FormattableData = new TheoryData<Formattable>() { };
				public static readonly TheoryData<Parsable> ParsableData = new TheoryData<Parsable>() { };
				public static readonly TheoryData<FormattableAndParsable> FormattableAndParsableData = new TheoryData<FormattableAndParsable>() { };

				[Theory]
				[{|xUnit1045:MemberData(nameof(FormattableData))|}]
				[{|xUnit1045:MemberData(nameof(ParsableData))|}]
				[{|xUnit1045:MemberData(nameof(FormattableAndParsableData))|}]
				public void TestMethod(object parameter) { }
			}

			public class Formattable : IFormattable	{
				public string ToString(string? format, IFormatProvider? formatProvider) => string.Empty;
			}

			public class Parsable : IParsable<Parsable>	{
				public static Parsable Parse(string s, IFormatProvider? provider) => new();
				public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Parsable result) {
					result = new();
					return true;
				}
			}

			public class FormattableAndParsable : IFormattable, IParsable<FormattableAndParsable> {
				public static FormattableAndParsable Parse(string s, IFormatProvider? provider) => new();
				public string ToString(string? format, IFormatProvider? formatProvider) => string.Empty;
				public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FormattableAndParsable result) {
					result = new();
					return true;
				}
			}
			""";

		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp11, source);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Diagnostics.CodeAnalysis;
			using Xunit;

			// Things which are newly serializable in v3

			public class UriClass {
				public sealed class Class : TheoryData<Uri> { }
				public static readonly TheoryData<Uri> Field = new TheoryData<Uri>() { };
				public static TheoryData<Uri> Method(int a, string b) => new TheoryData<Uri>() { };
				public static TheoryData<Uri> Property => new TheoryData<Uri>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(Uri parameter) { }
			}

			public class TupleClass {
				public sealed class Class : TheoryData<Tuple<string, int>> { }
				public static readonly TheoryData<Tuple<string, int>> Field = new TheoryData<Tuple<string, int>>() { };
				public static TheoryData<Tuple<string, int>> Method(int a, string b) => new TheoryData<Tuple<string, int>>() { };
				public static TheoryData<Tuple<string, int>> Property => new TheoryData<Tuple<string, int>>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(Tuple<string, int> parameter) { }
			}

			// Things which might not be serializable

			public class ObjectClass {
				public sealed class Class : TheoryData<object> { }
				public static readonly TheoryData<object> Field = new TheoryData<object>() { };
				public static TheoryData<object> Method(int a, string b) => new TheoryData<object>() { };
				public static TheoryData<object> Property => new TheoryData<object>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void DoesNotTrigger(object parameter) { }

				[Theory]
				[{|xUnit1045:ClassData(typeof(Class))|}]
				[{|xUnit1045:MemberData(nameof(Field))|}]
				[{|xUnit1045:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1045:MemberData(nameof(Property))|}]
				public void Triggers(object parameter) { }
			}

			public class IPossiblySerializableInterfaceClass {
				public sealed class Class : TheoryData<IPossiblySerializableInterface> { }
				public static readonly TheoryData<IPossiblySerializableInterface> Field = new TheoryData<IPossiblySerializableInterface>() { };
				public static TheoryData<IPossiblySerializableInterface> Method(int a, string b) => new TheoryData<IPossiblySerializableInterface>() { };
				public static TheoryData<IPossiblySerializableInterface> Property => new TheoryData<IPossiblySerializableInterface>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void DoesNotTrigger(IPossiblySerializableInterface parameter) { }

				[Theory]
				[{|xUnit1045:ClassData(typeof(Class))|}]
				[{|xUnit1045:MemberData(nameof(Field))|}]
				[{|xUnit1045:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1045:MemberData(nameof(Property))|}]
				public void Triggers(IPossiblySerializableInterface parameter) { }
			}

			public class PossiblySerializableUnsealedClassClass {
				public sealed class Class : TheoryData<PossiblySerializableUnsealedClass> { }
				public static readonly TheoryData<PossiblySerializableUnsealedClass> Field = new TheoryData<PossiblySerializableUnsealedClass>() { };
				public static TheoryData<PossiblySerializableUnsealedClass> Method(int a, string b) => new TheoryData<PossiblySerializableUnsealedClass>() { };
				public static TheoryData<PossiblySerializableUnsealedClass> Property => new TheoryData<PossiblySerializableUnsealedClass>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void DoesNotTrigger(PossiblySerializableUnsealedClass parameter) { }

				[Theory]
				[{|xUnit1045:ClassData(typeof(Class))|}]
				[{|xUnit1045:MemberData(nameof(Field))|}]
				[{|xUnit1045:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1045:MemberData(nameof(Property))|}]
				public void Triggers(PossiblySerializableUnsealedClass parameter) { }
			}

			public sealed class NonSerializableSealedClass { }

			public struct NonSerializableStruct { }

			public interface IPossiblySerializableInterface { }

			public class PossiblySerializableUnsealedClass { }
			""";

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp9, source);
	}

#if NETCOREAPP && ROSLYN_LATEST

	[Fact]
	public async ValueTask V3_only_NonAOT_NetCore()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Diagnostics.CodeAnalysis;
			using Xunit;

			// Things which are newly serializable in v3

			public class FormattableAndParsableClass {
				public static readonly TheoryData<FormattableAndParsable> FormattableAndParsableData = new TheoryData<FormattableAndParsable>() { };

				[Theory]
				[MemberData(nameof(FormattableAndParsableData))]
				public void TestMethod(object parameter) { }
			}

			public class FormattableAndParsable : IFormattable, IParsable<FormattableAndParsable> {
				public static FormattableAndParsable Parse(string s, IFormatProvider? provider) => new();
				public string ToString(string? format, IFormatProvider? formatProvider) => string.Empty;
				public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FormattableAndParsable result) {
					result = new();
					return true;
				}
			}
			""";

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp11, source);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	internal class Analyzer_v3_Pre301 : TheoryDataTypeArgumentsShouldBeSerializable
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(3, 0, 0));
	}
}
