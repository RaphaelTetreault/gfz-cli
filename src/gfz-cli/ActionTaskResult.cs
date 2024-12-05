namespace Manifold.GFZCLI;

/// <summary>
///     Indicates the result state of each action task.
/// </summary>
public enum ActionTaskResult
{
    Success,
    Failure,

    FileWriteSuccess,
    FileOverwriteSuccess,
    FileOverwriteSkip,

    FilePatchSuccess,
}
