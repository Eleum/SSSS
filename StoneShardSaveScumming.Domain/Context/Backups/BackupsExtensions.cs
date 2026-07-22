namespace StoneShardSaveScumming.Domain.Context.Backups
{
    public static class BackupsExtensions
    {
        extension(string path)
        {
            public string ToLocalPath() => Path.GetFullPath(path);
        }

        extension(SaveId id)
        {
            public SaveId Next() => new(id.Current, SaveId.New);
        }
    }
}
