using GameCube.GX.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

public static class GfzCliImageUtilities
{

    public static Image<Rgba32> TextureToImage(Texture texture)
    {
        Image<Rgba32> image = new Image<Rgba32>(texture.Width, texture.Height);

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

    public static void WriteImage(Options options, IImageEncoder encoder, Texture texture, FileWriteInfo info)
    {
        var action = () =>
        {
            Image<Rgba32> image = TextureToImage(texture);
            image.Save(info.OutputFilePath, encoder);
        };

        FileWriteOverwriteHandler(options, action, info);
    }
}
