using System.Collections.Generic;
using System.Linq;

namespace Xunit.Analyzers
{
    public class _1FunWithSyntaxVisualizer
    {
        [Fact]
        public void TestMethod()
        {
            var collection = new List<string>();

            // Expected behavior
            // Assert.Null({expr}.FirstOrDefault()) -> Assert.Empty({expr})
            // Assert.NotNull({expr}.FirstOrDefault()) -> Assert.NotEmpty({expr})
            // Assert.Null({expr}.FirstOrDefault({expr1})) -> Assert.DoesNotContain({expr}, {expr1})
            // Assert.NotNull({expr}.FirstOrDefault({expr1})) -> Assert.Contains({expr}, {expr1}) 

            Assert.Null(collection.Concat(collection).FirstOrDefault(x => x == "asd"));
        }
    }
}
