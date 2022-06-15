using System.ComponentModel.DataAnnotations;

namespace BackendSite.Service.Model.ApiModel
{
    /// <summary>
    /// Request 頁碼相關參數
    /// </summary>
    public class ApiRequestPageModel
    {
        /// <summary>
        /// 頁數
        /// </summary>
        [Required]
        [Range(1,9999)]
        public int PageNumber { get; set; }

        /// <summary>
        /// 每頁資料筆數
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

}
