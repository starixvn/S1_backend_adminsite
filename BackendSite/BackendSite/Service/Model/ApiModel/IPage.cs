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
        public (int index, int count) ListPagination(int totalCount, int pageNumber, int pageSize)
        {
            //Pagination
            TotalPages = (int)Math.Ceiling(totalCount / (float)pageSize);
            RowCount = totalCount;
            PageNumber = pageNumber;
            var index = (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize);
            var count = index + pageSize <= totalCount ? pageSize : totalCount - index;
            PageRange = $"{index + 1}-{index + count}";
            return (index, count);
        }

        /// <summary>
        /// 設定分頁資訊，並且回傳RedisCacheHelper.GetListByRange所需要的startIndex與endIndex
        /// </summary>
        /// <param name="totalCount">資料總筆數</param>
        /// <param name="pageNumber">資料頁數</param>
        /// <param name="pageSize">每頁數量</param>
        /// <returns></returns>
        public (long startIndex, long count) RedisPagination(int totalCount, int pageNumber, int pageSize)
        {
            //Pagination
            TotalPages = (int)Math.Ceiling(totalCount / (float)pageSize);
            RowCount = totalCount;
            PageNumber = pageNumber;
            var startIndex = (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize);
            var endIndex = startIndex + pageSize <= totalCount ? startIndex + pageSize : totalCount;
            PageRange = $"{startIndex + 1}-{endIndex}";
            return (startIndex, endIndex - 1);
        }
    }
}