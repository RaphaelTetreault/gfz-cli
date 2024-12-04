namespace Manifold.GFZCLI;

/// <summary>
///     Describes the input and outputs for <see cref="Actions"/> via <see cref="ActionAttribute"/>.
/// </summary>
[System.Flags]
public enum ActionIO
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
}
