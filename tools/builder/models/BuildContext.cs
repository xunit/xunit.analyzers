using System.Collections.Generic;

namespace Xunit.BuildTools.Models;

public partial class BuildContext
{
	public partial IReadOnlyList<string> GetSkippedAnalysisFolders() =>
		[];
}
