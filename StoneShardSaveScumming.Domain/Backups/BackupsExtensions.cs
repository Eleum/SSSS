namespace StoneShardSaveScumming.Domain.Backups
{
#pragma warning disable CA1708 // Identifiers should differ by more than case
    public static class BackupsExtensions
#pragma warning restore CA1708 // Identifiers should differ by more than case
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
