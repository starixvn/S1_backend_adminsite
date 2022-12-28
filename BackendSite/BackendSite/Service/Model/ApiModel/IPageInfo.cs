using BackendSite.Extensions;

namespace BackendSite.Service.Model.ApiModel
{
    public interface IPageInfo
    {
        /// <summary>
        /// 資料總頁數
        /// </summary>
        int TotalPages { get; set; }
        /// <summary>
        /// 資料頁數
        /// </summary>
        int PageNumber { get; set; }
        /// <summary>
        /// 資料總筆數
        /// </summary>
        int RowCount { get; set; }
        /// <summary>
        /// 資料範圍
        /// </summary>
        string PageRange { get; set; }

        /// <summary>
        /// 設定PageInfo的分頁資訊
        /// </summary>
        /// <param name="totalCount">資料總筆數</param>
        /// <param name="pageNumber">資料頁數</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns></returns>
        public void SetPageInfo(int totalCount, int pageNumber, int pageSize)
        {
            var (_, _) = ListPagination(totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// 設定分頁資訊，並且回傳List.GetRange所需要的index與count
        /// </summary>
        /// <param name="totalCount">資料總筆數</param>
        /// <param name="pageNumber">資料頁數</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns></returns>
        public (int startIndex, int count) ListPagination(int totalCount, int pageNumber, int pageSize)
        {
            //Pagination
            TotalPages = (int)Math.Ceiling(totalCount / (float)pageSize);
            RowCount = totalCount;
            PageNumber = pageNumber;
            var startIndex = pageNumber.GetStartIndex(pageSize).StartIndex;
            var count = startIndex + pageSize <= totalCount ? pageSize : totalCount - startIndex;
            PageRange = $"{startIndex + (totalCount == 0 ? 0 : 1)}-{startIndex + count}";
            return (startIndex, count);
        }

        /// <summary>
        /// 設定分頁資訊，並且回傳RedisCacheHelper.GetListByRange所需要的startIndex與endIndex
        /// </summary>
        /// <param name="totalCount">資料總筆數</param>
        /// <param name="pageNumber">資料頁數</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns></returns>
        public (long startIndex, long endIndex) RedisPagination(int totalCount, int pageNumber, int pageSize)
        {
            //Pagination
            TotalPages = (int)Math.Ceiling(totalCount / (float)pageSize);
            RowCount = totalCount;
            PageNumber = pageNumber;
            var startIndex = pageNumber.GetStartIndex(pageSize).StartIndex;
            var endIndex = startIndex + pageSize <= totalCount ? startIndex + pageSize : totalCount;
            PageRange = $"{startIndex + (totalCount == 0 ? 0 : 1)}-{endIndex}";
            return (startIndex, endIndex - 1);
        }
    }
}