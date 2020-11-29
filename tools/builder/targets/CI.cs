namespace Builder
{
	[Target(
		BuildTarget.CI,
		BuildTarget.PR, BuildTarget.SignPackages, BuildTarget.PublishPackages
	)]
	public class CI { }
}
