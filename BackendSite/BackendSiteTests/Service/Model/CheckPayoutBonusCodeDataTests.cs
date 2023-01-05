using BackendSite.Service.Model;
using NUnit.Framework;

namespace BackendSiteTests.Service.Model
{
    [TestFixture()]
    public class CheckPayoutBonusCodeDataTests
    {
        private CheckPayoutBonusCodeData _checkPayoutBonusCodeData;

        [SetUp]
        public void Setup()
        {
            _checkPayoutBonusCodeData = new CheckPayoutBonusCodeData
            {
                TierAmount ="[{\"Bonus\":100,\"MaxBonus\":100,\"RequiredAmount\":100},{\"Bonus\":200,\"MaxBonus\":200,\"RequiredAmount\":200},{\"Bonus\":300,\"MaxBonus\":300,\"RequiredAmount\":300}]"
            };
        }

        [TestCase(50, ExpectedResult = null)]
        [TestCase(100, ExpectedResult = 100)]
        [TestCase(150, ExpectedResult = 100)]
        [TestCase(200, ExpectedResult = 200)]
        [TestCase(250, ExpectedResult = 200)]
        [TestCase(300, ExpectedResult = 300)]
        [TestCase(400, ExpectedResult = 300)]
        public decimal? CalculateTierAmountTest(decimal actualDeposit) => _checkPayoutBonusCodeData.CalculateTierAmount(actualDeposit)?.Bonus;
    }
}
