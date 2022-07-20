using System;
using NUnit.Framework;

namespace BackendSiteTests.Service.BLL
{
    [TestFixture]
    public class TimesSpanTests
    {
        [TestCase("+05:30")]
        public void TimeSpanParse(string timezone)
        {
            Assert.AreEqual(new TimeSpan(5, 30, 0), TimeSpan.Parse(timezone.TrimStart('+')));
        }
    }
}