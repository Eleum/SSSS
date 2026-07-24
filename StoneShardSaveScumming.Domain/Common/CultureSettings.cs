using System.Globalization;

namespace StoneShardSaveScumming.Domain.Common
{
    internal sealed record CultureSettings
    {
        public static IFormatProvider FormatProvider => CultureInfo.InvariantCulture;
    }
}
