using StoneShardSaveScumming.Domain.Common;
using StoneShardSaveScumming.Domain.Game;
using System.Text;

namespace StoneShardSaveScumming.Domain.Backups
{
    public sealed record BackupId
    {
        private static readonly CompositeFormat _template = CompositeFormat.Parse("{0}_{1}");
        private static readonly CompositeFormat _lookupTemplate = CompositeFormat.Parse("{0}_{1}_{2}");
        private static readonly CompositeFormat _oldLookupTemplate = CompositeFormat.Parse("{0}_{1}_{2}_{3}");

        public string Value { get; }

        public GameDirectoryId Directory { get; }

        public SaveId Save { get; }

        public BackupId(GameDirectoryId directory, SaveId save)
        {
            Value = string.Format(CultureSettings.FormatProvider, _template, directory.Value, save.Current);
            Directory = directory;
            Save = save;
        }

        public static BackupId? FromPath(string path)
        {
            if (!System.IO.Directory.Exists(path))
                return null;

            var directoryName = Path.GetFileName(path);
            var separator = '_';
            var parts = directoryName.Split(separator);

            if (parts.Length != _lookupTemplate.MinimumArgumentCount
                && parts.Length != _oldLookupTemplate.MinimumArgumentCount)
            {
                return null;
            }

            var directoryPart = directoryName[..IndexOfSecondOccurence(directoryName, separator)];
            var savePart = parts[2];

            return (
                GameDirectoryId.From(directoryPart),
                SaveId.From(savePart)
            ) switch
            {
                (GameDirectoryId directory, SaveId save) => new BackupId(directory, save),
                _ => null
            };

            static int IndexOfSecondOccurence(string value, char item)
                => value.IndexOf(item) switch
                {
                    var idx when idx != -1 => value.IndexOf(item, idx + 1),
                    var other => other
                };
        }
    }
}
