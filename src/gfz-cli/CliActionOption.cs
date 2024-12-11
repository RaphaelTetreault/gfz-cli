namespace Manifold.GFZCLI;

/// <summary>
///     Describes the general options for <see cref="CliActionID"/> via <see cref="ActionAttribute"/>.
///     Refers specifically to options in <see cref="IOptionsGfzCli"/>.
/// </summary>
[System.Flags]
public enum CliActionOption
{
    /// <summary>
    ///     No generic parameters
    /// </summary>
    None,

    /// <summary>
    ///     AX/GX format.
    /// </summary>
    F_SerializationFormat = 1 << 0,

    /// <summary>
    ///     Overwrite files in output.
    /// </summary>
    O_OverwriteFiles = 1 << 1,

    /// <summary>
    ///     Game region J/E/P or JP/NA/EU.
    /// </summary>
    R_SerializationRegion = 1 << 2,

    /// <summary>
    ///     File search pattern (for directories).
    /// </summary>
    P_SearchPattern = 1 << 3,

    /// <summary>
    ///     Do search subdirectories?
    /// </summary>
    S_SearchSubdirectories = 1 << 4,

    /// <summary>
    ///     All options on.
    /// </summary>
    All = F_SerializationFormat | O_OverwriteFiles | P_SearchPattern | R_SerializationRegion | S_SearchSubdirectories,


    /// <summary>
    ///     AX/GX format.
    /// </summary>
    F = F_SerializationFormat,

    /// <summary>
    ///     Overwrite files in output.
    /// </summary>
    O = O_OverwriteFiles,

    /// <summary>
    ///     Game region J/E/P or JP/NA/EU.
    /// </summary>
    R = R_SerializationRegion,


    /// <summary>
    ///     File search pattern (for directories).
    /// </summary>
    P = P_SearchPattern,

    /// <summary>
    ///     Do search subdirectories?
    /// </summary>
    S = S_SearchSubdirectories,


    /// <summary>
    ///     -o -p -s
    /// </summary>
    OPS = O_OverwriteFiles | P_SearchPattern | S_SearchSubdirectories,

    /// <summary>
    ///     -o -p -s
    /// </summary>
    OPRS = O_OverwriteFiles | P_SearchPattern | R_SerializationRegion | S_SearchSubdirectories,

    /// <summary>
    ///     -f -o -p -s
    /// </summary>
    FOPS = F_SerializationFormat | O_OverwriteFiles | P_SearchPattern | S_SearchSubdirectories,

    /// <summary>
    ///     -f -p -r -s
    /// </summary>
    FPRS = F_SerializationFormat  | P_SearchPattern | R_SerializationRegion | S_SearchSubdirectories,

    /// <summary>
    ///     -f -o -p -r -s
    /// </summary>
    FOPRS = All,
}
