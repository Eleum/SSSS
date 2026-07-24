namespace StoneShardSaveScumming.Domain.Game
{
    public sealed record ExitSaveDirectory : GameDirectory<ExitSaveDirectoryId>
    {
        public override ExitSaveDirectoryId Id { get; }

        public override string PathLocal => Path.Combine(Character.PathLocal, Id.Value);

        public CharacterDirectory Character { get; }

        public ExitSaveDirectory(CharacterDirectory character, int number = 1)
        {
            ArgumentNullException.ThrowIfNull(character, nameof(character));
            ArgumentOutOfRangeException.ThrowIfLessThan(number, 1, nameof(number));

            Id = new ExitSaveDirectoryId(number);
            Character = character;
        }
    }
}
