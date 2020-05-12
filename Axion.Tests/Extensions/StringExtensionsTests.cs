using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axion.Core.Extensions;

namespace Axion.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void Truncate()
        {
            var initial = "Passion Pit - \"Sleepyhead\" (Official Music Video)";
            var expected = "Passion Pit - \"Sleepyhead\" (Official Mus...";

            var actual = initial.Truncate();

            Assert.AreEqual(expected, actual);
        }
    }
}