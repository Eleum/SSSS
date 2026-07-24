using StoneShardSaveScumming.Domain.Game;

namespace StoneShardSaveScumming.Domain.Backups
{
    public sealed record BackupsStorageDirectory
    {
        private const string _pathLocal = "./backups";

        private IEnumerable<BackupDirectory> AvailableBackups => Directory.GetDirectories(PathLocal)
            .Select(BackupDirectory.FromPath)
            .OfType<BackupDirectory>();

        public string PathLocal { get; } = _pathLocal.ToLocalPath();

        public BackupsStorageDirectory()
        {
        }

        public BackupsStorageDirectory(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path, nameof(path));

            PathLocal = path;
        }

        public string GetBackupLocalPathForSave(GameDirectoryId id, SaveId save)
            => Path.Combine(
                PathLocal,
                new BackupId(id, save).Value
            );

        public BackupDirectory? GetBackupOfSave(GameDirectoryId directory, SaveId save)
            => AvailableBackups.FirstOrDefault(x => x.Id.Directory.Equals(directory) && x.Id.Save.Equals(save));

        public BackupDirectory? GetLatestBackupOf(GameDirectoryId directory)
            => AvailableBackups.Where(x => x.Id.Directory.Equals(directory))
            .OrderByDescending(x => x.Created)
            .FirstOrDefault();

        public IEnumerable<BackupDirectory> GetAllBackupsOf(GameDirectoryId directory)
            => AvailableBackups.Where(x => x.Id.Directory.Equals(directory));
    }
}
