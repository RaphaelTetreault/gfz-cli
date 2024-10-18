namespace Manifold.GFZCLI;

/// <summary>
///     Simple record for file write information display.
/// </summary>
public readonly record struct FileWriteInfo
{
    public string InputFilePath { get; init; } 
    public string OutputFilePath { get; init; } 
    public string PrintPrefix { get; init; } 
    public string PrintActionDescription { get; init; } 
    public string PrintMoreInfoOnSkip { get; init; }
}
