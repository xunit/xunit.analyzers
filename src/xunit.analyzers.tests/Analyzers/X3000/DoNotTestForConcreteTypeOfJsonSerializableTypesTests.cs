using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotTestForConcreteTypeOfJsonSerializableTypes>;

namespace Xunit.Analyzers;

public class DoNotTestForConcreteTypeOfJsonSerializableTypesTests
{
	[Fact]
	public async Task AcceptanceTest()
	{
		var code = /* lang=c#-test */ """
			using Xunit;
			using Xunit.Sdk;
			using System.Collections.Generic;
			using System.Linq;
			
			[JsonTypeID("MyMessage")]
			public class MyMessage { };

			public class TheClass {
			    public void TheMethod() {
			        var message = new object();
			        var collection = new List<object>();

			        // Not testing against a serializable type
			        Assert.True(message is string);
			        Assert.True(message is not string);
			        Assert.NotNull(message as string);
			        Assert.NotNull((string)message);
			        Assert.Empty(collection.OfType<string>());
			        Assert.IsType(typeof(string), message);
			        Assert.IsType<string>(message);
			        Assert.IsNotType(typeof(string), message);
			        Assert.IsNotType<string>(message);
			        Assert.IsAssignableFrom(typeof(string), message);
			        Assert.IsAssignableFrom<string>(message);
			        Assert.IsNotAssignableFrom(typeof(string), message);
			        Assert.IsNotAssignableFrom<string>(message);

			        // Testing against a serializable type
			        Assert.True([|message is MyMessage|]);
			        Assert.True([|message is not MyMessage|]);
			        Assert.NotNull([|message as MyMessage|]);
			        Assert.NotNull([|(MyMessage)message|]);
			        Assert.Empty([|collection.OfType<MyMessage>()|]);
			        [|Assert.IsType(typeof(MyMessage), message)|];
			        [|Assert.IsType<MyMessage>(message)|];
			        [|Assert.IsNotType(typeof(MyMessage), message)|];
			        [|Assert.IsNotType<MyMessage>(message)|];
			        [|Assert.IsAssignableFrom(typeof(MyMessage), message)|];
			        [|Assert.IsAssignableFrom<MyMessage>(message)|];
			        [|Assert.IsNotAssignableFrom(typeof(MyMessage), message)|];
			        [|Assert.IsNotAssignableFrom<MyMessage>(message)|];
			    }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, code);
	}
}
