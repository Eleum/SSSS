namespace StoneShardSaveScumming.Domain.Game
{
    public static class GameExtensions
    {
        extension(GameDirectory directory)
        {
            public CharacterDirectory? GetCharacter() => directory switch
            {
                CharacterDirectory characterDirectory => characterDirectory,
                ExitSaveDirectory exitSaveDirectory => exitSaveDirectory.Character,
                _ => null
            };
        }
    }
}
