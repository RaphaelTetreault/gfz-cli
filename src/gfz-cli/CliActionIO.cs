namespace Manifold.GFZCLI;

/// <summary>
///     Describes the input and outputs for <see cref="CliActionID"/> via <see cref="ActionAttribute"/>.
/// </summary>
[System.Flags]
public enum CliActionIO
{
    /// <summary>
    ///     IO supports either a file path or a directory path.
    /// </summary>
    Path,

    /// <summary>
    ///     IO only supports directory paths.
    /// </summary>
    Directory,

    /// <summary>
    ///     IO only supports file paths.
    /// </summary>
    File,

    /// <summary>
    ///     IO is irrelevant for this action (input or output).
    /// </summary>
    None,
}
