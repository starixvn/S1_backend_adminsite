using System.Text;

namespace BackendSite.Service.Library;

public static class AddressFormat
{
    public static string Format(string line2, string line1, string city, string postCode, string state, string country)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(line2))
            sb.Append($"{line2},");
        if (!string.IsNullOrEmpty(line1))
            sb.Append($"{line1},");
        if (!string.IsNullOrEmpty(city))
            sb.Append($"{city},");
        if (!string.IsNullOrEmpty(postCode))
            sb.Append($"{postCode},");
        if (!string.IsNullOrEmpty(state))
            sb.Append($"{state},");
        if (!string.IsNullOrEmpty(country))
            sb.Append($"{country}");

        return sb.ToString();
    }
}