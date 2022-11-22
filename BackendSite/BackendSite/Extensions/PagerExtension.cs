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
    }
}
