using System.ComponentModel;

namespace BackendSite.Service.Model.enums
{
    public enum ErrorCodeEnum
    {
        //編碼規則
        //1XXX:Home
        //2XXX:Deposit
        //3XXX:Withdrawal
        //4XXX:Adjustment
        //5XXX:Customer相關代碼
        //9XXX:System常使用代碼
        
        #region In Common Use

        [Description("msg_Success")]
        Success = 0,

        [Description("msg_Fail")]
        Fail = -1,

        [Description("msg_NoData")]
        NoData = -2,

        [Description("msg_FileTypeNotMatch")]
        FileTypeNotMatch = -3,

        [Description("msg_PermissionDenied")]
        PermissionDenied = -4,

        [Description("msg_DuplicateData")]
        DuplicateData = -5,

        [Description("msg_UpdateFailed")]
        UpdateFailed = 9,

        [Description("msg_SystemException")]
        SystemException = 9999,

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
        WithdrawalLimitAdjustmentNotBalance = 4002,

        #endregion

        #region CSD

        [Description("msg_UserNotFound")]
        CustNotFound = 5001,

        [Description("msg_CustStatusIsSuspend")]
        CustStatusIsSuspend = 5002,

        [Description("msg_CustStatusIsClosed")]
        CustStatusIsClosed = 5003,

        [Description("msg_CustStatusIsLocked")]
        CustStatusIsLocked = 5004,

        #endregion
    }
}
