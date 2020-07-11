using Microsoft.VisualStudio.TestTools.UnitTesting;
using Axion.Common.Extensions;
using System;

namespace Axion.Tests.Extensions
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