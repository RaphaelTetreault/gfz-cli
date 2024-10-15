namespace Manifold.GFZCLI;

[System.Flags]
public enum ActionIO
{
    PathIn = 1 << 0,
    PathOut = 1 << 1,
    PathInOut = PathIn | PathOut,

    DirectoryIn = 1 << 2,
    DirectoryOut = 1 << 3,
    DirectoryInOut = DirectoryIn | DirectoryOut,

    FileIn = 1 << 4,
    FileOut = 1 << 5,
    FileInOut = FileIn | FileOut,
}
