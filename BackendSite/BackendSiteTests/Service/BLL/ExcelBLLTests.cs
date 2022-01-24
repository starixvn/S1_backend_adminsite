using BackendSite.Service.BLL;
using NUnit.Framework;

namespace BackendSiteTests.Service.BLL
{
    [TestFixture]
    public class ExcelBLLTests
    {
        [TestCase("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true)]
        [TestCase("application/vnd.ms-excel", true)]
        [TestCase("application/octet-stream", true)]
        [TestCase("application/text", false)]
        public void IsValidFileTest(string contentType, bool expect)
        {
            var excelBll = new ExcelBLL();
            var actual = excelBll.IsValidFile(contentType);
            Assert.AreEqual(expect, actual);
        }
    }
}