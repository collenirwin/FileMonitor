namespace FileMonitorConsole
{
    /// <summary>
    /// How was the file changed?
    /// </summary>
    public enum ChangeType
    {
        Edited,
        Added,
        Removed,
        ArchiveChanged,
        HiddenChanged,
        ReadOnlyChanged
    }
}
