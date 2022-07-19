namespace BackendSite.Extensions
{
    public static class PagerExtension
    {
        public static int GetStartIndex(this int pageSize, int pageNum)
            => (pageNum - 1) * pageSize;
    }
}