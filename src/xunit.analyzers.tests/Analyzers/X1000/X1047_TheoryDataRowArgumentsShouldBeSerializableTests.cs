using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataRowArgumentsShouldBeSerializable>;
using Verify_v3_Pre301 = CSharpVerifier<X1047_TheoryDataRowArgumentsShouldBeSerializableTests.Analyzer_v3_Pre301>;

public sealed class X1047_TheoryDataRowArgumentsShouldBeSerializableTests
{
	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using System.Diagnostics.CodeAnalysis;
			using Xunit;

			public class MyClass {
				// Tuples are known supported in v3 3.0.1+

				public IEnumerable<TheoryDataRowBase> TupleValue() {
					var value = Tuple.Create("Hello world", 42);

					yield return new TheoryDataRow(value);
					yield return new TheoryDataRow(Tuple.Create("Hello world", 42));
					yield return new TheoryDataRow<Tuple<string, int>>(value);
					yield return new TheoryDataRow<Tuple<string, int>>(Tuple.Create("Hello world", 42));
				}

				public IEnumerable<TheoryDataRowBase> ValueTupleValue() {
					var value = ValueTuple.Create("Hello world", 42);

					yield return new TheoryDataRow(value);
					yield return new TheoryDataRow(ValueTuple.Create("Hello world", 42));
					yield return new TheoryDataRow<ValueTuple<string, int>>(value);
					yield return new TheoryDataRow<ValueTuple<string, int>>(ValueTuple.Create("Hello world", 42));
				}

				// Maybe non-serializable values

				public IEnumerable<TheoryDataRowBase> ObjectValue() {
					var defaultValue = default(object);
					var nullValue = default(object?);
					var arrayValue = new object[0];

					yield return new TheoryDataRow({|#0:defaultValue|}, {|#1:nullValue|}, {|#2:arrayValue|});
					yield return new TheoryDataRow<object?, object?, object[]>({|#3:default(object)|}, {|#4:default(object?)|}, {|#5:new object[0]|});
				}

				public IEnumerable<TheoryDataRowBase> ArrayValue() {
					var defaultValue = default(Array);
					var nullValue = default(Array?);
					var arrayValue = new Array[0];

					yield return new TheoryDataRow({|#10:defaultValue|}, {|#11:nullValue|}, {|#12:arrayValue|});
					yield return new TheoryDataRow<Array?, Array?, Array[]>({|#13:default(Array)|}, {|#14:default(Array?)|}, {|#15:new Array[0]|});
				}

				public IEnumerable<TheoryDataRowBase> InterfaceValue() {
					var defaultValue = default(IEnumerable<int>);
					var nullValue = default(IEnumerable<int>?);
					var arrayValue = new IEnumerable<int>[0];

					yield return new TheoryDataRow({|#20:defaultValue|}, {|#21:nullValue|}, {|#22:arrayValue|});
					yield return new TheoryDataRow<IEnumerable<int>?, IEnumerable<int>?, IEnumerable<int>[]>({|#23:default(IEnumerable<int>)|}, {|#24:default(IEnumerable<int>?)|}, {|#25:new IEnumerable<int>[0]|});
				}

				public IEnumerable<TheoryDataRowBase> UnsealedClassValue() {
					var defaultValue = default(UnsealedClass);
					var nullValue = default(UnsealedClass?);
					var arrayValue = new UnsealedClass[0];

					yield return new TheoryDataRow({|#30:defaultValue|}, {|#31:nullValue|}, {|#32:arrayValue|});
					yield return new TheoryDataRow<UnsealedClass?, UnsealedClass?, UnsealedClass[]>({|#33:default(UnsealedClass)|}, {|#34:default(UnsealedClass?)|}, {|#35:new UnsealedClass[0]|});

					var value = new UnsealedClass();

					yield return new TheoryDataRow({|#36:value|});
					yield return new TheoryDataRow({|#37:new UnsealedClass()|});
					yield return new TheoryDataRow<UnsealedClass>({|#38:value|});
					yield return new TheoryDataRow<UnsealedClass>({|#39:new UnsealedClass()|});
				}
			}

			public class UnsealedClass { }
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1047").WithLocation(0).WithArguments("defaultValue", "object?"),
			Verify.Diagnostic("xUnit1047").WithLocation(1).WithArguments("nullValue", "object?"),
			Verify.Diagnostic("xUnit1047").WithLocation(2).WithArguments("arrayValue", "object[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(3).WithArguments("default(object)", "object?"),
			Verify.Diagnostic("xUnit1047").WithLocation(4).WithArguments("default(object?)", "object?"),
			Verify.Diagnostic("xUnit1047").WithLocation(5).WithArguments("new object[0]", "object[]"),

			Verify.Diagnostic("xUnit1047").WithLocation(10).WithArguments("defaultValue", "Array?"),
			Verify.Diagnostic("xUnit1047").WithLocation(11).WithArguments("nullValue", "Array?"),
			Verify.Diagnostic("xUnit1047").WithLocation(12).WithArguments("arrayValue", "Array[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(13).WithArguments("default(Array)", "Array?"),
			Verify.Diagnostic("xUnit1047").WithLocation(14).WithArguments("default(Array?)", "Array?"),
			Verify.Diagnostic("xUnit1047").WithLocation(15).WithArguments("new Array[0]", "Array[]"),

			Verify.Diagnostic("xUnit1047").WithLocation(20).WithArguments("defaultValue", "IEnumerable<int>?"),
			Verify.Diagnostic("xUnit1047").WithLocation(21).WithArguments("nullValue", "IEnumerable<int>?"),
			Verify.Diagnostic("xUnit1047").WithLocation(22).WithArguments("arrayValue", "IEnumerable<int>[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(23).WithArguments("default(IEnumerable<int>)", "IEnumerable<int>?"),
			Verify.Diagnostic("xUnit1047").WithLocation(24).WithArguments("default(IEnumerable<int>?)", "IEnumerable<int>?"),
			Verify.Diagnostic("xUnit1047").WithLocation(25).WithArguments("new IEnumerable<int>[0]", "IEnumerable<int>[]"),

			Verify.Diagnostic("xUnit1047").WithLocation(30).WithArguments("defaultValue", "UnsealedClass?"),
			Verify.Diagnostic("xUnit1047").WithLocation(31).WithArguments("nullValue", "UnsealedClass?"),
			Verify.Diagnostic("xUnit1047").WithLocation(32).WithArguments("arrayValue", "UnsealedClass[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(33).WithArguments("default(UnsealedClass)", "UnsealedClass?"),
			Verify.Diagnostic("xUnit1047").WithLocation(34).WithArguments("default(UnsealedClass?)", "UnsealedClass?"),
			Verify.Diagnostic("xUnit1047").WithLocation(35).WithArguments("new UnsealedClass[0]", "UnsealedClass[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(36).WithArguments("value", "UnsealedClass"),
			Verify.Diagnostic("xUnit1047").WithLocation(37).WithArguments("new UnsealedClass()", "UnsealedClass"),
			Verify.Diagnostic("xUnit1047").WithLocation(38).WithArguments("value", "UnsealedClass"),
			Verify.Diagnostic("xUnit1047").WithLocation(39).WithArguments("new UnsealedClass()", "UnsealedClass"),
		};

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source, expected);
	}

	[Fact]
	public async ValueTask V3_only_NonAOT_PreTupleSupport()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using Xunit;

			public class MyClass {
				public IEnumerable<TheoryDataRowBase> TupleValue() {
					var value = Tuple.Create("Hello world", 42);

					yield return new TheoryDataRow({|#0:value|});
					yield return new TheoryDataRow({|#1:Tuple.Create("Hello world", 42)|});
					yield return new TheoryDataRow<Tuple<string, int>>({|#2:value|});
					yield return new TheoryDataRow<Tuple<string, int>>({|#3:Tuple.Create("Hello world", 42)|});
				}

				public IEnumerable<TheoryDataRowBase> ValueTupleValue() {
					var value = ValueTuple.Create("Hello world", 42);

					yield return new TheoryDataRow({|#10:value|});
					yield return new TheoryDataRow({|#11:ValueTuple.Create("Hello world", 42)|});
					yield return new TheoryDataRow<ValueTuple<string, int>>({|#12:value|});
					yield return new TheoryDataRow<ValueTuple<string, int>>({|#13:ValueTuple.Create("Hello world", 42)|});
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1047").WithLocation(0).WithArguments("value", "Tuple<string, int>"),
			Verify.Diagnostic("xUnit1047").WithLocation(1).WithArguments("Tuple.Create(\"Hello world\", 42)", "Tuple<string, int>"),
			Verify.Diagnostic("xUnit1047").WithLocation(2).WithArguments("value", "Tuple<string, int>"),
			Verify.Diagnostic("xUnit1047").WithLocation(3).WithArguments("Tuple.Create(\"Hello world\", 42)", "Tuple<string, int>"),

			Verify.Diagnostic("xUnit1046").WithLocation(10).WithArguments("value", "(string, int)"),
			Verify.Diagnostic("xUnit1046").WithLocation(11).WithArguments("ValueTuple.Create(\"Hello world\", 42)", "(string, int)"),
			Verify.Diagnostic("xUnit1046").WithLocation(12).WithArguments("value", "(string, int)"),
			Verify.Diagnostic("xUnit1046").WithLocation(13).WithArguments("ValueTuple.Create(\"Hello world\", 42)", "(string, int)"),
		};

		await Verify_v3_Pre301.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source, expected);
	}

	[Fact]
	public async ValueTask V3_only_NonAOT_FormattableAndParsable()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using System.Diagnostics.CodeAnalysis;
			using Xunit;

			public class Formattable : IFormattable
			{
				public string ToString(string? format, IFormatProvider? formatProvider) => string.Empty;
			}

			public class Parsable : IParsable<Parsable>
			{
				public static Parsable Parse(string s, IFormatProvider? provider) => new();
				public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Parsable result)
				{
					result = new();
					return true;
				}
			}

			public class FormattableAndParsable : IFormattable, IParsable<FormattableAndParsable>
			{
				public static FormattableAndParsable Parse(string s, IFormatProvider? provider) => new();
				public string ToString(string? format, IFormatProvider? formatProvider) => string.Empty;
				public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FormattableAndParsable result)
				{
					result = new();
					return true;
				}
			}

			public class FormattableData {
				public IEnumerable<TheoryDataRowBase> MyMethod() {
					var defaultValue = default(Formattable);
					var nullValue = default(Formattable?);
					var arrayValue = new Formattable[0];

					yield return new TheoryDataRow({|#0:defaultValue|}, {|#1:nullValue|}, {|#2:arrayValue|});
					yield return new TheoryDataRow<Formattable, Formattable, Formattable[]>({|#3:default(Formattable)|}, {|#4:default(Formattable?)|}, {|#5:new Formattable[0]|});
				}
			}

			public class ParsableData {
				public IEnumerable<TheoryDataRowBase> MyMethod() {
					var defaultValue = default(Parsable);
					var nullValue = default(Parsable?);
					var arrayValue = new Parsable[0];

					yield return new TheoryDataRow({|#10:defaultValue|}, {|#11:nullValue|}, {|#12:arrayValue|});
					yield return new TheoryDataRow<Parsable, Parsable, Parsable[]>({|#13:default(Parsable)|}, {|#14:default(Parsable?)|}, {|#15:new Parsable[0]|});
				}
			}

			public class FormattableAndParsableData {
				public IEnumerable<TheoryDataRowBase> MyMethod() {
					var defaultValue = default(FormattableAndParsable);
					var nullValue = default(FormattableAndParsable?);
					var arrayValue = new FormattableAndParsable[0];

					yield return new TheoryDataRow(defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<FormattableAndParsable, FormattableAndParsable, FormattableAndParsable[]>(default(FormattableAndParsable), default(FormattableAndParsable?), new FormattableAndParsable[0]);
				}
			}
			""";
#if ROSLYN_LATEST && NET8_0_OR_GREATER
		var expected = new[] {
			Verify.Diagnostic("xUnit1047").WithLocation(0).WithArguments("defaultValue", "Formattable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(1).WithArguments("nullValue", "Formattable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(2).WithArguments("arrayValue", "Formattable[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(3).WithArguments("default(Formattable)", "Formattable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(4).WithArguments("default(Formattable?)", "Formattable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(5).WithArguments("new Formattable[0]", "Formattable[]"),

			Verify.Diagnostic("xUnit1047").WithLocation(10).WithArguments("defaultValue", "Parsable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(11).WithArguments("nullValue", "Parsable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(12).WithArguments("arrayValue", "Parsable[]"),
			Verify.Diagnostic("xUnit1047").WithLocation(13).WithArguments("default(Parsable)", "Parsable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(14).WithArguments("default(Parsable?)", "Parsable?"),
			Verify.Diagnostic("xUnit1047").WithLocation(15).WithArguments("new Parsable[0]", "Parsable[]"),
		};

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp11, source, expected);
#else
		// For some reason, 'dotnet format' complains about the indenting of #nullable enable in the source code line
		// above if the #if statement surrounds the whole method, so we use this "workaround" to do nothing in that case.
		Assert.NotEqual(string.Empty, source);
		await Task.Yield();
#endif
	}

	internal class Analyzer_v3_Pre301 : TheoryDataRowArgumentsShouldBeSerializable
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(3, 0, 0));
	}
}
