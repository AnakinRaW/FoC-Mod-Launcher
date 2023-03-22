using System.Globalization;
using System.Text;
using AnakinRaW.CommonUtilities.Wpf.Converters;

namespace AnakinRaW.AppUpdaterFramework.Converters;

internal class ProductBranchNameConverter : ValueConverter<string, string>
{
    protected override string? Convert(string value, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        if (value.Length == 1)
            return char.ToUpperInvariant(value[0]).ToString();
        var sb = new StringBuilder();
        sb.Append(char.ToUpperInvariant(value[0]));
        sb.Append(value, 1, value.Length - 1);
        return sb.ToString();
    }
}