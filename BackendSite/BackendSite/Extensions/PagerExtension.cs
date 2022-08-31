namespace BackendSite.Extensions
{
    public static class PagerExtension
    {
        public static (int StartIndex, int AbsPageSize) GetStartIndex(this int pageNumber, int pageSize)
        {
            var absPageSize = Math.Abs(pageSize);
            var startIndex = (Math.Abs(pageNumber) - 1) * absPageSize;

            return (startIndex, absPageSize);
        }

        public static string GetPageRange(this int pageNumber, int pageSize, int totalCount)
            => GetPageRangeInfo(pageNumber, pageSize, totalCount).PageRange;
        
        public static (string PageRange, int Index, int Count) GetPageRangeInfo(this int pageNumber, int pageSize, int totalCount)
        {
            var index = pageNumber.GetStartIndex(pageSize).StartIndex;
            var count = index + pageSize <= totalCount ? pageSize : totalCount - index;
            var pageRange = $"{index + (totalCount == 0 ? 0 : 1)}-{index + count}";
            
            return (pageRange, index, count);
        }
    }
}