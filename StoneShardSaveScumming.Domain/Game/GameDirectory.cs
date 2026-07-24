namespace StoneShardSaveScumming.Domain.Game
{
    public abstract record GameDirectory
    {
        public abstract string PathLocal { get; }
    }

    public abstract record GameDirectory<T> : GameDirectory
        where T : GameDirectoryId
    {
        public abstract T Id { get; }
    }
}
