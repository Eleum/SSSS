using StoneShardSaveScumming.Domain.Context.Game;

namespace StoneShardSaveScumming.Domain.Context.Backups
{
    public sealed record BackupId
    {
        private const string _backupPathTemplate = "{0}_{1}";

        public string Id { get; }

        public BackupId(GameDirectory directory, SaveId save)
        {
            Id = string.Format(_backupPathTemplate, directory.Name, save.Current);
        }
    }
}
