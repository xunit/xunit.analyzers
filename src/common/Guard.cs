using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Xunit;

/// <summary>
/// Helper class for guarding value arguments and valid state.
/// </summary>
static class Guard
{
	/// <summary>
	/// Ensures that a nullable reference type argument is not null.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null value</returns>
	/// <exception cref="ArgumentNullException">Thrown when the argument is null</exception>
	public static T ArgumentNotNull<T>(
		[NotNull] T? argValue,
		[CallerArgumentExpression("argValue")] string? argName = null)
			where T : class
	{
		if (argValue is null)
			throw new ArgumentNullException(argName?.TrimStart('@'));

		return argValue;
	}

	/// <summary>
	/// Ensures that a nullable enumerable type argument is not null or empty.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null, non-empty value</returns>
	/// <exception cref="ArgumentException">Thrown when the argument is null or empty</exception>
	public static T ArgumentNotNullOrEmpty<T>(
		[NotNull] T? argValue,
		[CallerArgumentExpression("argValue")] string? argName = null)
			where T : class, IEnumerable
	{
		ArgumentNotNull(argValue, argName);

		if (!argValue.GetEnumerator().MoveNext())
			throw new ArgumentException("Argument was empty", argName?.TrimStart('@'));

		return argValue;
	}

	/// <summary>
	/// Ensures that an argument is valid.
	/// </summary>
	/// <param name="message">The exception message to use when the argument is not valid</param>
	/// <param name="test">The validity test value</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null value</returns>
	/// <exception cref="ArgumentException">Thrown when the argument is not valid</exception>
	public static void ArgumentValid(
		string message,
		bool test,
		string? argName = null)
	{
		if (!test)
			throw new ArgumentException(message, argName);
	}

	/// <summary>
	/// Ensures that an argument is valid.
	/// </summary>
	/// <param name="messageFunc">The creator for an exception message to use when the argument is not valid</param>
	/// <param name="test">The validity test value</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null value</returns>
	/// <exception cref="ArgumentException">Thrown when the argument is not valid</exception>
	public static void ArgumentValid(
		Func<string> messageFunc,
		bool test,
		string? argName = null)
	{
		if (!test)
			throw new ArgumentException(messageFunc?.Invoke(), argName);
	}
}
