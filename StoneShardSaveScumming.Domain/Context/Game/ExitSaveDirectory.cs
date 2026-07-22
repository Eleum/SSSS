namespace StoneShardSaveScumming.Domain.Context.Game
{
    public sealed record ExitSaveDirectory : GameDirectory
    {
        private const string _exitSaveTemplate = "exitsave_{0}";

        public CharacterDirectory Character { get; }

        public int Number { get; }

        public override string PathLocal => Path.Combine(
            Character.PathLocal,
            string.Format(_exitSaveTemplate, Number)
        );

        public override string Name => string.Format(_exitSaveTemplate, Number);

        public ExitSaveDirectory(CharacterDirectory character, int number = 1)
        {
            ArgumentNullException.ThrowIfNull(character, nameof(character));
            ArgumentOutOfRangeException.ThrowIfLessThan(number, 1, nameof(number));

            Character = character;
            Number = number;
        }
    }
}
