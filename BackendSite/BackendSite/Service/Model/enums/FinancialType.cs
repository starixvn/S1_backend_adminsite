namespace BackendSite.Service.Model.enums
{
    public enum FinancialType
    {    
        All = 0,
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
        TpWager = 11001,            //11000 + tpId
        TpRefunds = 12001,          //12000 + tpId
        TpPayout = 13001,           //13000 + tpId
        TpPrize = 14001,            //14000 + tpId
        TpUndo = 15001,             //15000 + tpId
        TpTip = 16001,              //16000 + tpId
        Others = 99999,                 //
    }
}
