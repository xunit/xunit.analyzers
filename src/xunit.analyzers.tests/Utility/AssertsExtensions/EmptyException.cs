using System;

namespace Xunit.Sdk;

internal partial class EmptyException
{
	public static EmptyException ForNamedNonEmptyCollection(
		string collection,
		string collectionName) =>
			new(
				$"Assert.Empty() Failure: Collection '{collectionName}' was not empty" + Environment.NewLine +
				"Collection: " + collection
			);
}
