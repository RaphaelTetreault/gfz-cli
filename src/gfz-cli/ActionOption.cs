namespace Manifold.GFZCLI;

/// <summary>
///     Describes the general options for <see cref="Actions"/> via <see cref="ActionAttribute"/>.
///     Refers specifically to options in <see cref="IOptionsGfzCli"/>.
/// </summary>
[System.Flags]
public enum ActionOption
{
    /// <summary>
    ///     No generic parameters
    /// </summary>
    None,

    /// <summary>
    ///     AX/GX format
    /// </summary>
    SerializationFormat = 1 << 0,

    /// <summary>
    ///     Overwrite files in output
    /// </summary>
    OverwriteFiles = 1 << 1,

    /// <summary>
    ///     Game region J/E/P or JP/NA/EU
    /// </summary>
    SerializationRegion = 1 << 2,

    /// <summary>
    ///     File search pattern (for directories)
    /// </summary>
    SearchPattern = 1 << 3,

    /// <summary>
    ///     Do search subdirectories?
    /// </summary>
    SearchSubdirectories = 1 << 4,

    /// <summary>
    ///     Overwrite, Search Pattern, Search Subdirectories.
    ///     -o -p -s
    /// </summary>
    OPS = OverwriteFiles | SearchPattern | SearchSubdirectories,

    /// <summary>
    ///     Overwrite, Search Pattern, Search Subdirectories.
    ///     -o -p -s
    /// </summary>
    OPRS = OverwriteFiles | SearchPattern | SerializationRegion | SearchSubdirectories,

    /// <summary>
    ///     Format, Overwrite, Search Pattern, Search Subdirectories.
    ///     -f -o -p -s
    /// </summary>
    FOPS = SerializationFormat | OverwriteFiles | SearchPattern | SearchSubdirectories,

    /// <summary>
    ///     All options on.
    ///     -f -o -p -r -s
    /// </summary>
    All = SerializationFormat | OverwriteFiles | SearchPattern | SerializationRegion | SearchSubdirectories,
    FOPRS = All,
}
