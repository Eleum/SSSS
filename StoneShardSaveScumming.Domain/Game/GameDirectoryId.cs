using StoneShardSaveScumming.Domain.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace StoneShardSaveScumming.Domain.Game
{
    public abstract class GameDirectoryId
    {
        public string Value { get; protected set; } = default!;
    }
}
