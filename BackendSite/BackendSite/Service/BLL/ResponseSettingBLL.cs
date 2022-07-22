using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class ResponseSettingBLL
    {

        #region Field

        private readonly SelLangBLL _selLangBLL;

        #endregion

        #region Constructor

        public ResponseSettingBLL(SelLangBLL selLangBLL)
        {
            _selLangBLL = selLangBLL;
        }

        #endregion

        #region Action

        /// <summary>
        /// 裝載回傳結果
        /// </summary>
        /// <param name="status"></param>
        /// <param name="ex"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public ApiResult SetResponse(ErrorCodeEnum status,
                                     Exception ex = null,
                                     string lang = "en",
                                     string custMessage = null)
        {
            return new ApiResult()
            {
                ErrorCode = (int)status,
                Message = string.IsNullOrEmpty(custMessage) ? _selLangBLL.GetMsg(status.GetDescription(), lang) : custMessage,
                RawErrorCode = ex == null ? "0" : ex.HResult.ToString(),
                RawMessage = ex == null ? "" : ex.Message
            };
        }

        /// <summary>
        /// 裝載回傳結果(含資料)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="status"></param>
        /// <param name="data"></param>
        /// <param name="ex"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public ApiResult SetResponse<T>(ErrorCodeEnum status,
                                        T data,
                                        Exception ex = null,
                                        string lang = "en",
                                        string custMessage = null)
        {
            return new ApiResult<T>()
            {
                Data = data,
                ErrorCode = (int)status,
                Message = string.IsNullOrEmpty(custMessage) ? _selLangBLL.GetMsg(status.GetDescription(), lang) : custMessage,
                RawErrorCode = ex == null ? "0" : ex.HResult.ToString(),
                RawMessage = ex == null ? "" : ex.Message
            };
        }

        /// <summary>
        /// 裝載回傳結果(含資料+分頁)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="status"></param>
        /// <param name="data"></param>
        /// <param name="totalPages"></param>
        /// <param name="pageNum"></param>
        /// <param name="rowCount"></param>
        /// <param name="ex"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public ApiListResult<T> SetResponse<T>(ErrorCodeEnum status,
                                        IEnumerable<T> data,
                                        int totalPages = 0,
                                        int pageNum = 0,
                                        int rowCount = 0,
                                        Exception ex = null,
                                        string lang = "en",
                                        string custMessage = null)
        {
            return new ApiListResult<T>()
            {
                Data = data,
                ErrorCode = (int)status,
                Message = string.IsNullOrEmpty(custMessage) ? _selLangBLL.GetMsg(status.GetDescription(), lang) : custMessage,
                RawErrorCode = ex == null ? "0" : ex.HResult.ToString(),
                RawMessage = ex == null ? "" : ex.Message,
                TotalPages = totalPages,
                PageNumber = pageNum,
                RowCount = rowCount
            };
        }

        /// <summary>
        /// 裝載回傳例外結果
        /// </summary>
        /// <param name="status"></param>
        /// <param name="ex"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public ApiResult SetResponse(Exception ex,
                                     string lang = "en",
                                     string custMessage = null)
        {
            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.SystemException,
                Message = string.IsNullOrEmpty(custMessage) ? _selLangBLL.GetMsg(ErrorCodeEnum.SystemException.GetDescription(), lang) : custMessage,
                RawErrorCode = ex == null ? "0" : ex.HResult.ToString(),
                RawMessage = ex == null ? "" : ex.Message
            };
        }

        #endregion
    }
}
