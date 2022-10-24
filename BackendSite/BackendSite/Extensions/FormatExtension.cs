namespace BackendSite.Extensions
{
    public static class FormatExtension
    {
        public static IEnumerable<string> SplitRemoveSpace(string str, char separator)
        {
            return string.IsNullOrEmpty(str) 
                ? new string[]{}
                : str.Replace(" ", "").Split(separator);
        }

        ///// <summary>
        ///// decimal 小數兩位
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static decimal ToDecimal2(this decimal source)
        //{
        //    return Math.Round(source, 2, MidpointRounding.AwayFromZero);
        //}
    }
}