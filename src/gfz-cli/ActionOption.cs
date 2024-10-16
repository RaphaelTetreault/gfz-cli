namespace Manifold.GFZCLI;

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
    Format = 1 << 0,

    /// <summary>
    ///     Overwrite files in output
    /// </summary>
    Overwrite = 1 << 1,

    /// <summary>
    ///     Game region J/E/P or JP/NA/EU
    /// </summary>
    Region = 1 << 2,

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
    OPS = Overwrite | SearchPattern | SearchSubdirectories,

    /// <summary>
    ///     Overwrite, Search Pattern, Search Subdirectories.
    ///     -f -o -p -s
    /// </summary>
    FOPS = Format | Overwrite | SearchPattern | SearchSubdirectories,

    /// <summary>
    ///     All options on.
    ///     -f -o -p -r -s
    /// </summary>
    All = Format | Overwrite | Region | SearchPattern | SearchSubdirectories,
    FORPS = All,
}
