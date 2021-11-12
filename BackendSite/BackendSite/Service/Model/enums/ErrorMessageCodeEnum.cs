using System.ComponentModel;

namespace BackendSite.Service.Model.enums
{
    public enum ErrorMessageCodeEnum
    {
        //編碼規則
        //1XXX:Home
        //2XXX:Deposit
        //3XXX:Withdrawal
        //4XXX:Adjustment
        //9XXX:System常使用代碼
        
        #region In Common Use

        [Description("msg_Success")]
        Success = 0,

        [Description("msg_Fail")]
        Fail = -1,

        [Description("msg_NoData")]
        NoData = -2,

        [Description("msg_SystemException")]
        SystemException = 9999,

        [Description("msg_UserNotFound")]
        CustNotFound = 9000,

        #endregion

        #region Home

        [Description("errlogin_enter_un")]
        AccountInvalid = 1001,

        [Description("errlogin_enter_pw")]
        PasswordInvalid = 1002,

        [Description("incorrect_un")]
        AccountError = 1003,

        [Description("incorrect_pw")]
        PasswordError = 1004,

        [Description("msg_ACLocked")]
        AccountLocked = 1005,

        [Description("msg_OldpwdNotMatch")]
        OldpwdNotMatch = 1006,

        #endregion

        #region Finance

        [Description("msg_WithdrawalLimitAdjustmentNotBalance")]
        WithdrawalLimitAdjustmentNotBalance = 4002

        #endregion
    }
}
