namespace StoneShardSaveScumming.Domain.Game
{
    public sealed record CharacterDirectory : GameDirectory<CharacterDirectoryId>
    {
        public override CharacterDirectoryId Id { get; }

        public override string PathLocal => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StoneShard",
            $"characters_v{Id.Version}",
            Id.Value
        );

        public CharacterDirectory(int number)
        {
            Id = new CharacterDirectoryId(number);
        }
    }
}
