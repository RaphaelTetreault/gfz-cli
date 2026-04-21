namespace Manifold.GfzCli;

/// <summary>
///     Indicates the result state of each action task.
/// </summary>
public enum FileResult
{
    FileWriteSuccess,
    FileOverwriteSuccess,
    FileOverwriteSkip,

    FilePatchSuccess,
}
