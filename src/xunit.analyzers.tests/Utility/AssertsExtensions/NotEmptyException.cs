namespace Xunit.Sdk;

internal partial class NotEmptyException
{
	public static NotEmptyException ForNamedNonEmptyCollection(string collectionName) =>
		new($"Assert.NotEmpty() Failure: Collection '{collectionName}' was empty");
}
