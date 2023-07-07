using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public interface ISelLang
    {
        public string GetErrMsg(int code);
    }

    public class ApiResult
    {
        public int ErrorCode { get; set; }
        private string _Message;
        protected ISelLang SelLang;
        public string Message
        {
            get
            {
                if (!string.IsNullOrEmpty(_Message))
                {
                    return _Message;
                }
                else
                {
                    if (SelLang != null)
                        _Message = SelLang.GetErrMsg(ErrorCode);
                }

                return _Message;
            }
            set
            {
                _Message = value;
            }
        }

        public void SetSelLang(ISelLang selLang)
        {
            this.SelLang = selLang;
        }
        public string RawErrorCode { get; set; }
        public string RawMessage { get; set; }
        public ApiResult() : this(9000, "", "", "") { }
        public ApiResult(ISelLang selLang) : this(9000, "", "", "") { SelLang = selLang; }
        public ApiResult(int ErrorCode, string Message) : this(ErrorCode, Message, "", "") { }
        public ApiResult(int _ErrorCode, string Message, string RawErrorCode, string RawMessage)
        {
            this.ErrorCode = _ErrorCode;
            this.Message = Message;
            this.RawErrorCode = RawErrorCode;
            this.RawMessage = RawMessage;
        }
    }
    public class ApiResult<TData> : ApiResult
    {
        public ApiResult() : this(9000, "", default) { }
        public ApiResult(ISelLang selLang) : this(9000, "", default) { SelLang = selLang; }
        public ApiResult(int ErrorCode, string Message, TData data) : base(ErrorCode, Message) { Data = data; }
        public TData Data { get; set; }
    }

    public class ApiListResult<TData> : ApiResult, IPageInfo
    {
        public ApiListResult() : this(9000, "") { }
        public ApiListResult(ISelLang selLang) : this(9000, "") { SelLang = selLang; }
        public ApiListResult(int ErrorCode, string Message) : base(ErrorCode, Message) { }
        public ApiListResult(ErrorCodeEnum errorCode, IPageInfo pageInfo, IEnumerable<TData> data) : base((int)errorCode, "")
        {
            Data = data;
            PageNumber = pageInfo.PageNumber;
            RowCount = pageInfo.RowCount;
            TotalPages = pageInfo.TotalPages;
            PageRange = pageInfo.PageRange;
        }
        public IEnumerable<TData> Data { get; set; }
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int RowCount { get; set; }
        public string PageRange { get; set; }
    }
    public class ApiListResultTotalAmount<TData> : ApiResult, IPageInfo
    {
        public ApiListResultTotalAmount() : this(9000, "") { }
        public ApiListResultTotalAmount(ISelLang selLang) : this(9000, "") { SelLang = selLang; }
        public ApiListResultTotalAmount(int ErrorCode, string Message) : base(ErrorCode, Message) { }
        public IEnumerable<TData> Data { get; set; }

        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int RowCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalApproveAmount { get; set; }
        public int TotalApproveRow { get; set; }
        public string PageRange { get; set; }
    }

    public class ApiListResultTotalReferralAmount<TData> : ApiResult, IPageInfo
    {
        public ApiListResultTotalReferralAmount() : this(9000, "") { }
        public ApiListResultTotalReferralAmount(ISelLang selLang) : this(9000, "") { SelLang = selLang; }
        public ApiListResultTotalReferralAmount(int ErrorCode, string Message) : base(ErrorCode, Message) { }
        public IEnumerable<TData> Data { get; set; }

        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int RowCount { get; set; }
        public decimal TotalRewardAmount { get; set; }
        public decimal TotalPayoutAmount { get; set; }
        public string PageRange { get; set; }
    }

    public class ApiListResultCurrencyAmount<TData> : ApiResult, IPageInfo
    {
        public ApiListResultCurrencyAmount() : this(9000, "") { }
        public ApiListResultCurrencyAmount(ISelLang selLang) : this(9000, "") { SelLang = selLang; }
        public ApiListResultCurrencyAmount(int ErrorCode, string Message) : base(ErrorCode, Message) { }
        public IEnumerable<TData> Data { get; set; }

        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int RowCount { get; set; }
        public List<string> CurrencyAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalApproveAmount { get; set; }
        public int TotalApproveRow { get; set; }
        public List<TotalAmount> CurrencyAmountRecord { get; set; }
        public string PageRange { get; set; }
    }

    public class DataWithPage<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
}
