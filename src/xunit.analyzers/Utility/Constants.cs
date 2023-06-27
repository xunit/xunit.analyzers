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
		public const string Collection = "Collection";
		public const string Contains = "Contains";
		public const string DoesNotContain = "DoesNotContain";
		public const string DoesNotMatch = "DoesNotMatch";
		public const string Empty = "Empty";
		public const string EndsWith = "EndsWith";
		public const string Equal = "Equal";
		public const string Fail = "Fail";
		public const string False = "False";
		public const string IsAssignableFrom = "IsAssignableFrom";
		public const string IsNotAssignableFrom = "IsNotAssignableFrom";
		public const string IsNotType = "IsNotType";
		public const string IsType = "IsType";
		public const string Matches = "Matches";
		public const string NotEmpty = "NotEmpty";
		public const string NotEqual = "NotEqual";
		public const string NotNull = "NotNull";
		public const string NotSame = "NotSame";
		public const string NotStrictEqual = "NotStrictEqual";
		public const string Null = "Null";
		public const string Same = "Same";
		public const string Single = "Single";
		public const string StartsWith = "StartsWith";
		public const string StrictEqual = "StrictEqual";
		public const string Throws = "Throws";
		public const string ThrowsAny = "ThrowsAny";
		public const string ThrowsAnyAsync = "ThrowsAnyAsync";
		public const string ThrowsAsync = "ThrowsAsync";
		public const string True = "True";
	}

	/// <summary>
	/// Attribute names (without the Attribute suffix unless otherwise noted)
	/// </summary>
	public static class Attributes
	{
		public const string Fact = "Fact";
		public const string Theory = "Theory";
	}

	/// <summary>
	/// Property names from xUnit.net attributes
	/// </summary>
	public static class AttributeProperties
	{
		public const string DeclaringType = "DeclaringType";
		public const string MemberName = "MemberName";
		public const string MemberType = "MemberType";
	}

	/// <summary>
	/// Properties placed into diagnostics to be picked up by fixes
	/// </summary>
	public static class Properties
	{
		public const string AssertMethodName = "AssertMethodName";
		public const string CanFix = "CanFix";
		public const string DeclaringType = "DeclaringType";
		public const string IgnoreCase = "IgnoreCase";
		public const string IsStatic = "IsStatic";
		public const string IsStaticMethodCall = "IsStaticMethodCall";
		public const string LiteralValue = "LiteralValue";
		public const string MemberName = "MemberName";
		public const string MethodName = "MethodName";
		public const string ParameterArrayStyle = "ParameterArrayStyle";
		public const string ParameterIndex = "ParameterIndex";
		public const string ParameterName = "ParameterName";
		public const string ParameterSpecialType = "ParameterSpecialType";
		public const string Replacement = "Replacement";
		public const string SizeValue = "SizeValue";
		public const string SubstringMethodName = "SubstringMethodName";
		public const string TestClassName = "TestClassName";
		public const string TFixtureDisplayName = "TFixtureDisplayName";
		public const string TFixtureName = "TFixtureName";
		public const string TypeName = "TypeName";
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
			}
		}
	}
}
