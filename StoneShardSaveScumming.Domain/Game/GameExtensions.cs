using System.Diagnostics;

namespace StoneShardSaveScumming.Domain.Game
{
#pragma warning disable CA1708 // Identifiers should differ by more than case
    public static class GameExtensions
#pragma warning restore CA1708 // Identifiers should differ by more than case
    {
        extension(GameDirectoryId)
        {
            public static GameDirectoryId? From(string value)
            {
                return ExitSaveDirectoryId.From(value) as GameDirectoryId
                    ?? CharacterDirectoryId.From(value);
            }
        }

        extension(GameDirectory directory)
        {
            public GameDirectoryId Id => directory switch
            {
                CharacterDirectory characterDirectory => characterDirectory.Id,
                ExitSaveDirectory exitSaveDirectory => exitSaveDirectory.Id,
                _ => throw new UnreachableException()
            };

            public CharacterDirectory? Character => directory switch
            {
                CharacterDirectory characterDirectory => characterDirectory,
                ExitSaveDirectory exitSaveDirectory => exitSaveDirectory.Character,
                _ => throw new UnreachableException()
            };
        }
    }
}
