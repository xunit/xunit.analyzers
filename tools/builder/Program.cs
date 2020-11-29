using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Builder
{
	public class Program
	{
		public static Task<int> Main(string[] args)
			=> CommandLineApplication.ExecuteAsync<BuildContext>(args);
	}
}
