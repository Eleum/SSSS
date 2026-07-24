using StoneShardSaveScumming.Domain.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace StoneShardSaveScumming.Domain.Game
{
    public sealed partial class CharacterDirectoryId : GameDirectoryId
    {
        [GeneratedRegex(@"^character_(?<number>\d+)$", RegexOptions.Compiled)]
        private static partial Regex CharacterRegex();

        private readonly CompositeFormat _characterTemplate = CompositeFormat.Parse("character_{0}");

        public int Number { get; }

        public int Version { get; } = 1;

        public CharacterDirectoryId(int number)
        {
            Value = string.Format(CultureSettings.FormatProvider, _characterTemplate, number);
            Number = number;
        }

        public static CharacterDirectoryId? From(string value)
            => CharacterRegex().Match(value) switch
            {
                Match match => new CharacterDirectoryId(int.Parse(match.Groups["number"].Value, CultureSettings.FormatProvider)),
                _ => null
            };

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                CharacterDirectoryId other => (Value, Number) == (other.Value, other.Number),
                _ => base.Equals(obj)
            };
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Number);
        }
    }
}
