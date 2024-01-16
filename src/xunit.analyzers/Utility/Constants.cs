namespace Xunit.Analyzers;

public static class Constants
{
	/// <summary>
	/// Argument names for Assert methods
	/// </summary>
	public static class AssertArguments
	{
		public const string Actual = "actual";
		public const string Expected = "expected";
		public const string IgnoreCase = "ignoreCase";
	}

	/// <summary>
	/// Method names from Assert
	/// </summary>
	public static class Asserts
	{
		public const string Collection = nameof(Collection);
		public const string CollectionAsync = nameof(CollectionAsync);
		public const string Contains = nameof(Contains);
		public const string DoesNotContain = nameof(DoesNotContain);
		public const string DoesNotMatch = nameof(DoesNotMatch);
		public const string Empty = nameof(Empty);
		public const string EndsWith = nameof(EndsWith);
		public const string Equal = nameof(Equal);
		public const string Fail = nameof(Fail);
		public const string False = nameof(False);
		public const string IsAssignableFrom = nameof(IsAssignableFrom);
		public const string IsNotAssignableFrom = nameof(IsNotAssignableFrom);
		public const string IsNotType = nameof(IsNotType);
		public const string IsType = nameof(IsType);
		public const string Matches = nameof(Matches);
		public const string NotEmpty = nameof(NotEmpty);
		public const string NotEqual = nameof(NotEqual);
		public const string NotNull = nameof(NotNull);
		public const string NotSame = nameof(NotSame);
		public const string NotStrictEqual = nameof(NotStrictEqual);
		public const string Null = nameof(Null);
		public const string PropertyChangedAsync = nameof(PropertyChangedAsync);
		public const string RaisesAnyAsync = nameof(RaisesAnyAsync);
		public const string RaisesAsync = nameof(RaisesAsync);
		public const string Same = nameof(Same);
		public const string Single = nameof(Single);
		public const string StartsWith = nameof(StartsWith);
		public const string StrictEqual = nameof(StrictEqual);
		public const string Throws = nameof(Throws);
		public const string ThrowsAny = nameof(ThrowsAny);
		public const string ThrowsAnyAsync = nameof(ThrowsAnyAsync);
		public const string ThrowsAsync = nameof(ThrowsAsync);
		public const string True = nameof(True);
	}

	/// <summary>
	/// Attribute names (without the Attribute suffix unless otherwise noted)
	/// </summary>
	public static class Attributes
	{
		public const string Fact = nameof(Fact);
		public const string Theory = nameof(Theory);
	}

	/// <summary>
	/// Property names from xUnit.net attributes
	/// </summary>
	public static class AttributeProperties
	{
		public const string DeclaringType = nameof(DeclaringType);
		public const string MemberName = nameof(MemberName);
		public const string MemberType = nameof(MemberType);
	}

	/// <summary>
	/// Properties placed into diagnostics to be picked up by fixes
	/// </summary>
	public static class Properties
	{
		public const string ArgumentValue = nameof(ArgumentValue);
		public const string AssertMethodName = nameof(AssertMethodName);
		public const string DeclaringType = nameof(DeclaringType);
		public const string IgnoreCase = nameof(IgnoreCase);
		public const string IsStatic = nameof(IsStatic);
		public const string IsStaticMethodCall = nameof(IsStaticMethodCall);
		public const string LiteralValue = nameof(LiteralValue);
		public const string MemberName = nameof(MemberName);
		public const string MethodName = nameof(MethodName);
		public const string NewBaseType = nameof(NewBaseType);
		public const string ParameterArrayStyle = nameof(ParameterArrayStyle);
		public const string ParameterIndex = nameof(ParameterIndex);
		public const string ParameterName = nameof(ParameterName);
		public const string ParameterSpecialType = nameof(ParameterSpecialType);
		public const string Replacement = nameof(Replacement);
		public const string SizeValue = nameof(SizeValue);
		public const string SubstringMethodName = nameof(SubstringMethodName);
		public const string TestClassName = nameof(TestClassName);
		public const string TFixtureDisplayName = nameof(TFixtureDisplayName);
		public const string TFixtureName = nameof(TFixtureName);
		public const string TypeName = nameof(TypeName);
	}

	/// <summary>
	/// Type names as strings for runtime lookup
	/// </summary>
	public static class Types
	{
		public static class System
		{
			public const string ObsoleteAttribute = "System.ObsoleteAttribute";
		}

		public static class Xunit
		{
			public const string FactAttribute = "Xunit.FactAttribute";
			public const string LongLivedMarshalByRefObject = "Xunit.LongLivedMarshalByRefObject";
			public const string TheoryAttribute = "Xunit.TheoryAttribute";

			public static class Sdk
			{
				public const string DataAttribute = "Xunit.Sdk.DataAttribute";
				public const string LongLivedMarshalByRefObject = "Xunit.Sdk.LongLivedMarshalByRefObject";
			}
		}
	}
}
