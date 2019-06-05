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

            Assert.Null(collection.Concat(collection).FirstOrDefault());
            Assert.Empty(collection.Concat(collection));
        }
    }
}
