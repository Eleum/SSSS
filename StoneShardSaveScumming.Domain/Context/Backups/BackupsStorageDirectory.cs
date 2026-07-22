using StoneShardSaveScumming.Domain.Context.Game;

namespace StoneShardSaveScumming.Domain.Context.Backups
{
    public sealed record BackupsStorageDirectory
    {
        private const string _defaultPathLocal = "./backups";

        public string PathLocal { get; } = _defaultPathLocal.ToLocalPath();

        public BackupsStorageDirectory()
        {
        }

        public BackupsStorageDirectory(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path, nameof(path));
            
            PathLocal = path;
        }

        public string GetBackupLocalPathForSave(GameDirectory directory, SaveId save)
            => Path.Combine(
                PathLocal,
                new BackupId(directory, save).Id
            );
    }
}
