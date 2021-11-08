using System.ComponentModel;

namespace BackendSite.ExceptionHandling
{
    public enum ErrorCodeEnum
    {
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
    }
}
