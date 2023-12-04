// Imported from xUnit.net v3, must be removed when this test project is upgraded

#if NETFRAMEWORK

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
	public CallerArgumentExpressionAttribute(string parameterName)
	{
		ParameterName = parameterName;
	}

	public string ParameterName { get; }
}

#endif
