using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using NUnit.Framework;

namespace BackendSite.Service.Model.TpMaintain.Tests
{
    [TestFixture()]
    public class FinancialLogTests
    {
        [TestCase(1, "", ExpectedResult = FinancialType.Deposit)]
        [TestCase(101, "", ExpectedResult = FinancialType.Deposit)]
        [TestCase(102, "", ExpectedResult = FinancialType.Deposit)]
        [TestCase(1000, "", ExpectedResult = FinancialType.Deposit)]
        [TestCase(2, "Payment Adjustment", ExpectedResult = FinancialType.PaymentAdjustment)]
        [TestCase(3, "Payment Adjustment", ExpectedResult = FinancialType.PaymentAdjustment)]
        [TestCase(7, "Payment Adjustment", ExpectedResult = FinancialType.PaymentAdjustment)]
        [TestCase(8, "Payment Adjustment", ExpectedResult = FinancialType.PaymentAdjustment)]
        [TestCase(5, "", ExpectedResult = FinancialType.BonusReward)]
        [TestCase(6, "", ExpectedResult = FinancialType.LockedAmount)]
        [TestCase(7, "", ExpectedResult = FinancialType.CancelledBonus)]
        [TestCase(8, "", ExpectedResult = FinancialType.CashReward)]
        [TestCase(9, "", ExpectedResult = FinancialType.Commision)]
        [TestCase(10, "", ExpectedResult = FinancialType.BonusTransferIn)]
        [TestCase(11, "", ExpectedResult = FinancialType.Rebate)]
        [TestCase(12, "", ExpectedResult = FinancialType.CashBack)]
        [TestCase(13, "", ExpectedResult = FinancialType.ReferFriendsRewards)]
        [TestCase(14, "", ExpectedResult = FinancialType.VIPUpgradeReward)]
        [TestCase(15, "", ExpectedResult = FinancialType.AffTransferToPlayer)]
        [TestCase(16, "", ExpectedResult = FinancialType.RunningBonus)]
        [TestCase(20, "", ExpectedResult = FinancialType.VerificationReward)]
        [TestCase(1001, "", ExpectedResult = FinancialType.Withdrawal)]
        [TestCase(1002, "", ExpectedResult = FinancialType.Withdrawal)]
        [TestCase(1003, "", ExpectedResult = FinancialType.Withdrawal)]
        [TestCase(2000, "", ExpectedResult = FinancialType.Withdrawal)]
        [TestCase(2001, "", ExpectedResult = FinancialType.TpTransfer)]
        [TestCase(3000, "", ExpectedResult = FinancialType.TpTransfer)]
        [TestCase(2001, "", ExpectedResult = FinancialType.TpTransfer)]
        [TestCase(11001, "", ExpectedResult = FinancialType.TpWager)]
        [TestCase(12000, "", ExpectedResult = FinancialType.TpWager)]
        [TestCase(12001, "", ExpectedResult = FinancialType.TpRefunds)]
        [TestCase(13000, "", ExpectedResult = FinancialType.TpRefunds)]
        [TestCase(13001, "", ExpectedResult = FinancialType.TpPayout)]
        [TestCase(14000, "", ExpectedResult = FinancialType.TpPayout)]
        [TestCase(14001, "", ExpectedResult = FinancialType.TpPrize)]
        [TestCase(15000, "", ExpectedResult = FinancialType.TpPrize)]
        [TestCase(15001, "", ExpectedResult = FinancialType.TpUndo)]
        [TestCase(16000, "", ExpectedResult = FinancialType.TpUndo)]
        [TestCase(16001, "", ExpectedResult = FinancialType.TpTip)]
        [TestCase(17000, "", ExpectedResult = FinancialType.TpTip)]
        public FinancialType FinancialTypeTest(int type, string Memo)
            => new FinancialLog
            {
                Type = type,
                Memo = Memo,
            }.FinancialType;
    }
}