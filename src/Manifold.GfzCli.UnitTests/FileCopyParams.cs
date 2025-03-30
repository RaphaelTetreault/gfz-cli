using Manifold.GFZCLI;

namespace Manifold.GfzCli.UnitTests;

public readonly record struct FileCopyParams
{
    public FileCopyParams()
    {
    }

    public readonly required string FilesSource { get; init; }
    public readonly required string FilesDestination { get; init; }
    public readonly string SearchPattern { get; init; } = string.Empty;
    public readonly SearchOption SearchOption { get; init; } = SearchOption.TopDirectoryOnly;
    public readonly bool Overwrite { get; init; } = false;
    public readonly int FileCopyLimit { get; init; } = int.MaxValue;
}