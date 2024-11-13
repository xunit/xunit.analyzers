#if !NETCOREAPP

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute(string parameterName) :
	Attribute
{
	public string ParameterName { get; } = parameterName;
}

#endif
