namespace Manifold.GfzCli;

/// <summary>
///     SixLabours.ImageSharp resampler types.
/// </summary>
/// <seealso cref="https://github.com/SixLabors/ImageSharp/blob/main/src/ImageSharp/Processing/KnownResamplers.cs"/>
public enum ResamplerType
{
    Bicubic,
    Box,
    CatmullRom,
    Hermite,
    Lanczos2,
    Lanczos3,
    Lanczos5,
    Lanczos8,
    MitchellNetravali,
    NearestNeighbor,
    Robidoux,
    RobidouxSharp,
    Spline,
    Triangle,
    Welch,
}
