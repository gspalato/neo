using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oculus.Common.Extensions;
using System.Linq;

namespace Oculus.Tests.Extensions
{
    [TestClass]
    public class EnumerableExtensionsTests
    {
        [TestMethod]
        public void ChunkTest()
        {
            var initial = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            var actual = initial.Chunk(3);

            if (!(actual.Count() == 3 && actual.Last().Last() == 7))
                Assert.Fail("Array wasn't split right.");
        }
    }
}