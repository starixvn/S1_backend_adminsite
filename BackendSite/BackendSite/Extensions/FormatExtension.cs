namespace BackendSite.Extensions
{
    public static class FormatExtension
    {
        public static IEnumerable<string> SplitRemoveSpace(string str, char separator)
        {
            return string.IsNullOrWhiteSpace(str) 
                ? Array.Empty<string>()
                : str.Replace(" ", "").Split(separator);
        }
    }
}