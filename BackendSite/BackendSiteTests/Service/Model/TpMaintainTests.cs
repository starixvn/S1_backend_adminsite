using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model.TpMaintain.Tests
{
    [TestFixture()]
    public class TpMaintainTests
    {
        private TpMaintain _tpMaintain;

        [SetUp]
        public void Setup()
        {
            _tpMaintain = new TpMaintain
            {
                SiteId = 1,
                TpId = 1,
                TpGameType = 1,
                TpCategoryId = 1,
                BeginDate = new DateTime(2020, 01, 01),
                EndDate = new DateTime(2021, 01, 01),
            };
        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = false)]
        public bool SiteFilterTes(int filter) => _tpMaintain.SiteFilter(new FilterInfo
        {
            SiteId = new List<int>() { filter }
        });

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = true)]
        public bool TpFilterTest(int filter) => _tpMaintain.TpFilter(new FilterInfo
        {
            TpId = filter
        });


        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = true)]
        public bool TpGameTypeFilterTest(int filter) => _tpMaintain.TpGameTypeFilter(new FilterInfo
        {
            TpGameType = filter
        });

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = true)]
        public bool TpCategoryIdFilterTest(int filter) => _tpMaintain.TpCategoryIdFilter(new FilterInfo
        {
            TpCategoryId = filter
        });

        [TestCase("2019-01-01", "2022-01-01", ExpectedResult = true)]
        [TestCase("2020-01-01", "2022-01-01", ExpectedResult = true)]
        [TestCase("2019-01-01", "2023-01-01", ExpectedResult = true)]
        [TestCase("2020-02-01", "2020-03-01", ExpectedResult = true)]
        [TestCase("2022-01-01", "2023-01-01", ExpectedResult = false)]
        public bool DateFilterTest(DateTime startDate, DateTime endDate) => _tpMaintain.DateFilter(new FilterInfo
        {
            StartDate = startDate,
            EndDate = endDate,
        });


        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = true)]
        public bool ExpiredFilterTest(bool IsExpired) => _tpMaintain.ExpiredFilter(new FilterInfo
        {
            IsExpired = IsExpired
        });      
    }
}