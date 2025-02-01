using GameCube.GX.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

/// <summary>
///     Utility for handling GameCube GX textures as images and vice-versa.
/// </summary>
public static class GfzCliImageUtilities
{
    /// <summary>
    ///     Convert <see cref="Texture"/> into <see cref="Image"/>.
    /// </summary>
    /// <param name="texture">The texture to convert to image.</param>
    /// <returns>
    ///     New instance of <see cref="Image"/> from <see cref="Texture"/> data.
    /// </returns>
    public static Image<Rgba32> TextureToImage(Texture texture)
    {
        Image<Rgba32> image = new(texture.Width, texture.Height);

        for (int y = 0; y < texture.Height; y++)
        {
            for (int x = 0; x < texture.Width; x++)
            {
                TextureColor pixel = texture[x, y];
                image[x, y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }

        return image;
    }

    /// <summary>
    ///     Convert <see cref="Image"/> into <see cref="Texture"/>.
    /// </summary>
    /// <param name="image">The texture to convert to texture.</param>
    /// <param name="textureFormat">GX texture format of the output texture.</param>
    /// <returns>
    ///     New instance of <see cref="Texture"/> from <see cref="Image"/> data.
    /// </returns>
    public static Texture ImageToTexture(Image<Rgba32> image, TextureFormat textureFormat = TextureFormat.RGBA8)
    {
        var texture = new Texture(image.Width, image.Height, textureFormat);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Rgba32 pixel = image[x, y];
                texture[x, y] = new TextureColor(pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        return texture;
    }

    /// <summary>
    ///     Saves out <paramref name="texture"/> as image to disk at <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="outputPath">The file output path.</param>
    /// <param name="texture">The texture to save.</param>
    /// <param name="encoder">The image encoder to save image with.</param>
    public static void WriteTextureAsImage(Options options, OSPath outputPath, Texture texture, IImageEncoder encoder)
    {
        bool canWrite = CheckWillFileWrite(options, outputPath, out ActionTaskResult _);
        if (canWrite)
        {
            Image<Rgba32> image = TextureToImage(texture);
            image.Save(outputPath, encoder);
        }
    }
}
