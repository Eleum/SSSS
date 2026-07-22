namespace StoneShardSaveScumming.Domain.Context.Game
{
    public sealed record CharacterDirectory(int Number, int Version = 1) : GameDirectory
    {
        private const string _characterTemplate = "character_{0}";

        public override string PathLocal => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StoneShard",
            $"characters_v{Version}",
            Name
        );

        public override string Name => string.Format(_characterTemplate, Number);
    }
}
