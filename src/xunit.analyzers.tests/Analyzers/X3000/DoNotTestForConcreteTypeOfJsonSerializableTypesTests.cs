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
			using System.Collections.Generic;
			using System.Linq;
			
			public class GenericClass<T1,T2> { }

			public class TheClass {
				/// <summary>
				/// Testing XMLDOC references <see cref="MyMessage"/>.
				/// </summary>
				public void TheMethod() {
					var message = new object();
					var collection = new List<object>();

					// Direct construction
					_ = new MyMessage { PropertyValue = 2112 };
					static MyMessage Create(int propertyValue) =>
						new() { PropertyValue = propertyValue };

					// Non-serialized type
					_ = message is IMyMessage;
					_ = message is not IMyMessage;
					_ = message as IMyMessage;
					_ = (IMyMessage)message;
					_ = typeof(IMyMessage);
					_ = collection.OfType<IMyMessage>();
					_ = default(IEnumerable<IMyMessage>);
					_ = new GenericClass<IMyMessage, int>();
					_ = new GenericClass<int, IMyMessage>();

					// Serialized type
					_ = [|message is MyMessage|];
					_ = [|message is not MyMessage|];
					_ = [|message as MyMessage|];
					_ = [|(MyMessage)message|];
					_ = [|typeof(MyMessage)|];
					_ = collection.[|OfType<MyMessage>|]();
					_ = default([|IEnumerable<MyMessage>|]);
					_ = new [|GenericClass<MyMessage, int>|]();
					_ = new [|GenericClass<int, MyMessage>|]();
				}
			}
			""";
		var messagePartial1 = /* lang=c#-test */ """
			using Xunit.Sdk;

			public interface IMyMessage { }

			[JsonTypeID("MyMessage")]
			sealed partial class MyMessage : IMyMessage { }
			""";
		var messagePartial2 = /* lang=c#-test */ """
			public partial class MyMessage {
				public int PropertyValue { get; set; }
			};
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [messagePartial1, messagePartial2, code]);
	}
}
