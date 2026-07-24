namespace StoneShardSaveScumming.Domain.Backups
{
    public sealed record BackupDirectory
    {
        public BackupId Id { get; }

        public DateTimeOffset Created { get; }

        public string PathLocal { get; }

        public BackupDirectory(BackupId id, DateTimeOffset created, string pathLocal)
        {
            if (!Path.Exists(pathLocal)) throw new ArgumentException("Specified path does not exist", nameof(pathLocal));

            Id = id;
            Created = created;
            PathLocal = pathLocal.ToLocalPath();
        }

        public static BackupDirectory? FromPath(string path)
            => BackupId.FromPath(path) switch
            {
                BackupId id => new BackupDirectory(id, Directory.GetCreationTimeUtc(path), path),
                _ => null
            };
    }
}
