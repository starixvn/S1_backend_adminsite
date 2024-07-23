using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text;

using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendSite.Service.Model
{
    public class GetBetListRequest : IOrderBy
    {
        /// <summary>
        /// 頁數, -1 = export all
        /// </summary>
        [Required]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁資料筆數
        /// </summary>
        public int PageSize { get; set; } = 25;

        [Required]
        public IEnumerable<int> SiteIds { get; set; }
        [Required]
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> Tags { get; set; } = new List<int>();
        public int? VIPlevel { get; set; } = 0;
        public int TpId { get; set; } = 0;
        public int GameType { get; set; } = 0;
        public string TransId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TpUserName { get; set; } = string.Empty;
        public string RefNo { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public IEnumerable<string> Game { get; set; } = new List<string>();
        public string Title { get; set; } = string.Empty;
        public string MatchID { get; set; } = string.Empty;
        public string BetType { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal Stake { get; set; } = 0;
        public decimal WinlostAmount { get; set; } = 0;
        /// <summary>
        /// 是否排序
        /// </summary>
        public bool OrderBy { get; set; }
        /// <summary>
        /// 排序欄位名稱
        /// </summary>
        public string OrderByProperty { get; set; }
        /// <summary>
        /// 排序類型:
        /// 0=升序
        /// 1=降序
        /// </summary>
        public OrderByDirection OrderByDirection { get; set; }
        public string GetRedisKey()
        {
            var jObject = JObject.Parse(JsonConvert.SerializeObject(this));
            jObject["StartTime"] = StartTime.ToString("yyyyMMddHHmmssfff");
            jObject["EndTime"] = EndTime.ToString("yyyyMMddHHmmssfff");
            //移除排序跟分頁
            jObject.Remove("PageSize");
            jObject.Remove("PageNumber");
            jObject.Remove("OrderBy");
            jObject.Remove("OrderByProperty");
            jObject.Remove("OrderByDirection");

            var key = new StringBuilder();
            foreach (var item in jObject.Properties())
            {
                key.Append($"{item.Name}:{item.Value}:");
            }
            return key.ToString();
        }
        public IEnumerable<MemberTypeEnum> MemberTypes { get; set; }
    }

    public class BetListReport : IDisposable
    {
        private bool _isDisposed;
        private IntPtr _nativeResource = Marshal.AllocHGlobal(100);

        public string SiteId { get; set; }

        public string SiteName {  get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string TpUserName { get; set; }
        public int TpId { get; set; }
        public string TransId { get; set; }
        public int GameType { get; set; }
        public DateTimeOffset TransTime
        {
            get
            {
                return new DateTimeOffset(_transTime, _offset);
            }
            set
            {
                _transTime = value.DateTime;
            }
        }
        private DateTime _transTime { get; set; }
        public DateTimeOffset SettledTime
        {
            get
            {
                return new DateTimeOffset(_settledTime, _offset);
            }
            set
            {
                _settledTime = value.DateTime;
            }

        }
        private DateTime _settledTime { get; set; }
        private TimeSpan _offset { get; set; } = TimeSpan.Zero;
        public void SetBetListTimeOffset(TimeSpan timeSpan)
        {
            _offset = timeSpan;
        }
        public decimal Stake { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
        public string ProviderID { get; set; }
        public string Currency { get; set; }
        public string RefNo { get; set; }
        public string GameID { get; set; }
        public string Provider { get; set; }
        public string Product { get; set; }
        public string Category { get; set; }
        public string Game { get; set; }
        public string Title { get; set; }
        public string MatchID { get; set; }
        public string BetType { get; set; }
        public string BetChoice { get; set; }
        public string Result { get; set; }
        public decimal Odds { get; set; }
        public string OddsType { get; set; }
        public string Platform { get; set; }
        public string Live { get; set; }
        public string IP { get; set; }
        public string TagList { get; set; }
        public IEnumerable<int> Tags
        {
            get
            {
                if (TagList?.Length > 0)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<int>>(TagList);
                }
                return null;
            }
        }
        public int VipLevel { get; set; }
        public string LevelName { get; set; }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // free managed resources
            }

            // free native resources if there are any.
            if (_nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_nativeResource);
                _nativeResource = IntPtr.Zero;
            }

            _isDisposed = true;
        }
    }
    public class SportTypeList
    {
        public string Name { get; set; }
    }
}
