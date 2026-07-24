namespace StoneShardSaveScumming.Domain.Backups
{
    public sealed record SaveId
    {
        internal static Guid New => Guid.NewGuid();

        public Guid Previous { get; } = Guid.Empty;

        public Guid Current { get; } = New;

        public SaveId()
        {
        }

        public SaveId(Guid current)
        {
            Current = current;
        }

        public SaveId(Guid previous, Guid current)
        {
            Previous = previous;
            Current = current;
        }

        public static SaveId? From(string value)
            => value switch
            {
                var guid when Guid.TryParse(guid, out var parsed) => new SaveId(parsed),
                _ => null
            };
    }
}
