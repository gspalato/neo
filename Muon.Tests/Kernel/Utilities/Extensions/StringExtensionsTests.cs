using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muon.Kernel.Utilities;

namespace Muon.Tests
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

        [TestMethod]
        public void Escape()
        {
            var initial = "|*~`_";
            var expected = @"\|\*\~\`\_";

            var actual = initial.Escape();

            Assert.AreEqual(expected, actual);
        }
    }
}