namespace Manifold.GFZCLI;

/// <summary>
///     Define how mipmaps are generated.
/// </summary>
public enum MipmapGenerationMode
{
    /// <summary>
    ///     Generate subsequent mipmaps using last image spercified.
    /// </summary>
    Last,

    /// <summary>
    ///     Generate subsequent mipmaps by wrapping around and reusing images.
    /// </summary>
    Wrap,

    /// <summary>
    ///     Generate subsequent mipmaps by cycling through exisint images back and forth.
    /// </summary>
    PingPong,
}
