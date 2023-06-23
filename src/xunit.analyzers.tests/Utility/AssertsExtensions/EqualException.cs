using System;

namespace Xunit.Sdk;

internal partial class EqualException
{
	public static EqualException ForMismatchedValuesWithMessage(
		object? expected,
		object? actual,
		string? message)
	{
		var expectedText = expected as string ?? ArgumentFormatter.Format(expected);
		var actualText = actual as string ?? ArgumentFormatter.Format(actual);

		if (string.IsNullOrWhiteSpace(message))
			message =
				"Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"Expected: " + expectedText.Replace(Environment.NewLine, newLineAndIndent) + Environment.NewLine +
				"Actual:   " + actualText.Replace(Environment.NewLine, newLineAndIndent);

		return new(message);
	}
}
