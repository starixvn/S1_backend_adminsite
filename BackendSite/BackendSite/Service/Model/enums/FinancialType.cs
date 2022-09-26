namespace BackendSite.Service.Model.enums
{
    public enum FinancialType
    {
        Deposit = 1,                //1 and 101 ~ 1000
        PaymentAdjustment = 2,      //2,3,7,8
        BonusReward = 5,            //5	
        LockedAmount = 6,           //6
        CancelledBonus = 7,         //7
        CashReward = 8,             //8
        Commision = 9,              //9
        BonusTransferIn = 10,       //10
        Rebate = 11,                //11
        CashBack = 12,              //12
        ReferFriendsRewards = 13,   //13
        VIPUpgradeReward = 14,      //14
        AffTransferToPlayer = 15,   //15
        RunningBonus = 16,          //16
        VerificationReward = 20,    //20
        Withdrawl = 1001,           //1001 ~ 2000
        TpTransfer = 2001,          //2001 + tpId
        TpWager = 11000,            //11000 + tpId
        TpRefunds = 12000,          //12000 + tpId
        TpPayout = 13000,           //13000 + tpId
        TpPrize = 14000,            //14000 + tpId
        TpUndo = 15000,             //15000 + tpId
        TpTip = 16000               //16000 + tpId
    }
}
