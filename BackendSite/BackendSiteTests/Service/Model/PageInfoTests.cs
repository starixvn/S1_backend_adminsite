using BackendSite.Service.Model.ApiModel;
using NUnit.Framework;

namespace BackendSite.Service.Model.TpMaintain.Tests
{
    [TestFixture()]
    public class PageInfoTests
    {
        private IPageInfo _pageInfo;

        [SetUp]
        public void Setup()
        {
             _pageInfo = new PageInfo();
        }

        [TestCase(10, 1, 5, 2, "1-5")]
        [TestCase(10, 2, 5, 2, "6-10")]
        [TestCase(100, 1, 30, 4, "1-30")]
        [TestCase(100, 2, 30, 4, "31-60")]
        [TestCase(100, 3, 30, 4, "61-90")]
        [TestCase(100, 4, 30, 4, "91-100")]
        [TestCase(0, 1, 10, 0, "0-0")]
        public void SetPageInfoTest(int totalCount, int pageNumber, int pageSize, int expectedTotalPages, string expectedPageRange)
        {
            _pageInfo.SetPageInfo(totalCount, pageNumber, pageSize);
            Assert.AreEqual(_pageInfo.RowCount, totalCount);
            Assert.AreEqual(_pageInfo.PageNumber, pageNumber);
            Assert.AreEqual(_pageInfo.TotalPages, expectedTotalPages);
            Assert.AreEqual(_pageInfo.PageRange, expectedPageRange);
        }

        [TestCase(10, 1, 5, 0, 5)]
        [TestCase(10, 2, 5, 5, 5)]
        [TestCase(100, 1, 30, 0, 30)]
        [TestCase(100, 2, 30, 30, 30)]
        [TestCase(100, 3, 30, 60, 30)]
        [TestCase(100, 4, 30, 90, 10)]
        public void ListPaginationTest(int totalCount, int pageNumber, int pageSize, int expectedIndex, int expectedCount)
        {
            var (index, count) = _pageInfo.ListPagination(totalCount, pageNumber, pageSize);
            Assert.AreEqual(index, expectedIndex);
            Assert.AreEqual(count, expectedCount);
        }

        [TestCase(10, 1, 5, 0, 4)]
        [TestCase(10, 2, 5, 5, 9)]
        [TestCase(100, 1, 30, 0, 29)]
        [TestCase(100, 2, 30, 30, 59)]
        [TestCase(100, 3, 30, 60, 89)]
        [TestCase(100, 4, 30, 90, 99)]
        public void RedisPaginationTest(int totalCount, int pageNumber, int pageSize, int expectedStart, int expectedEnd)
        {
            var (startIndex, endIndex) = _pageInfo.RedisPagination(totalCount, pageNumber, pageSize);
            Assert.AreEqual(startIndex, expectedStart);
            Assert.AreEqual(endIndex, expectedEnd);
        }


    }
}