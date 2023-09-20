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

        ///// <summary>
        ///// decimal �p�ƨ��
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static decimal ToDecimal2(this decimal source)
        //{
        //    return Math.Round(source, 2, MidpointRounding.AwayFromZero);
        //}

        public static string GetChannelName(this string currencyName, string network)
        {
            return currencyName.Equals(network, StringComparison.OrdinalIgnoreCase)
                ? currencyName
                : $"{currencyName}-{network}";
        }
    }
}