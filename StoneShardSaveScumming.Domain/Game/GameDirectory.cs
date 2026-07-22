namespace StoneShardSaveScumming.Domain.Game
{
    public abstract record GameDirectory
    {
        public abstract string PathLocal { get; }

        public abstract string Name { get; }

        public override string ToString() => PathLocal;
    }
}
