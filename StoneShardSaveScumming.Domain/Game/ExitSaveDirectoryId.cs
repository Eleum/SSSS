using StoneShardSaveScumming.Domain.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace StoneShardSaveScumming.Domain.Game
{
    public sealed partial class ExitSaveDirectoryId : GameDirectoryId
    {
        [GeneratedRegex(@"^exitsave_(?<number>\d+)$", RegexOptions.Compiled)]
        private static partial Regex ExitSaveRegex();

        private readonly CompositeFormat _exitSaveTemplate = CompositeFormat.Parse("exitsave_{0}");

        public int Number { get; init; }

        public ExitSaveDirectoryId(int number)
        {
            Value = string.Format(CultureSettings.FormatProvider, _exitSaveTemplate, number);
            Number = number;
        }

        public static ExitSaveDirectoryId? From(string value)
            => ExitSaveRegex().Match(value) switch
            {
                Match match => new ExitSaveDirectoryId(int.Parse(match.Groups["number"].Value, CultureSettings.FormatProvider)),
                _ => null
            };

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                ExitSaveDirectoryId other => (Value, Number) == (other.Value, other.Number),
                _ => base.Equals(obj)
            };
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Number);
        }
    }
}
