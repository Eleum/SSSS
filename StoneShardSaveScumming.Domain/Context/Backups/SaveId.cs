namespace StoneShardSaveScumming.Domain.Context.Backups
{
    public sealed record SaveId
    {
        internal static Guid New => Guid.NewGuid();

        public Guid Previous { get; } = Guid.Empty;

        public Guid Current { get; } = New;

        public SaveId()
        {
        }

        public SaveId(Guid previous, Guid current)
        {
            Previous = previous;
            Current = current;
        }
    }
}
