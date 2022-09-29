using System;
using System.ComponentModel.DataAnnotations;

namespace BackendSite.Service.Model.ApiModel
{
    /// <summary>
    /// Request 頁碼相關參數
    /// </summary>
    public class ApiRequestPageModel
    {
        private int _pageNumber { get; set; }
        private int _pageSize { get; set; } = 10;
        
        /// <summary>
        /// 頁數
        /// </summary>
        [Required]
        [Range(1,9999)]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = Math.Abs(value);
        }

        /// <summary>
        /// 每頁資料筆數
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Abs(value);
        }
    }
}
