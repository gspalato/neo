using Microsoft.VisualStudio.TestTools.UnitTesting;
using Muon.Kernel.Utilities;
using System;

namespace Muon.Tests
{
    [TestClass]
    public class DateTimeExtensionsTests
    {
        [TestMethod]
        public void ToHumanDuration()
        {
            var initial = TimeSpan.FromSeconds(1432);
            var expected = "23m 52s";

            var actual = initial.ToHumanDuration();

            Assert.AreEqual(expected, actual);
        }
    }
}