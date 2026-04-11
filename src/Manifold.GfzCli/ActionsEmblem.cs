using GameCube.GFZ.Emblem;
using GameCube.GFZ.GCI;
using GameCube.GX.Texture;
using Manifold.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;
using static Manifold.GfzCli.GfzCliImageUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for managing emblem images.
/// </summary>
public static class ActionsEmblem
{
    #region BIN

    /// <summary>
    ///     Extract images from emblem binary archives.
    /// </summary>
    /// <param name="options"></param>
    public static void EmblemsBinToImages(Options options)
    {
        Terminal.WriteLine("Emblem: converting emblems from BIN files.");
        int binCount = ParallelizeFileInFileOutTasks(options, EmblemBinToImages);
        Terminal.WriteLine($"Emblem: done converting {binCount} file{Plural(binCount)}.");
    }

    /// <summary>
    ///     Extract images from emblem binary archives.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    private static void EmblemBinToImages(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Read BIN Emblem data
        EmblemBIN emblemBIN = new(inputFile);
        Emblem[] emblems = emblemBIN.Value.Emblems;

        ImageEncoder encoder = options.ImageEncoder;
        outputFile.PushDirectory(emblemBIN.FileName);
        outputFile.SetExtensions(".png");

        // Write out each emblem in file
        int formatLength = emblems.LengthToFormat();
        for (int i = 0; i < emblems.Length; i++)
        {
            // Prepare emblem name
            var emblem = emblems[i];
            int index = i + 1;
            string indexStr = index.PadLeft(formatLength, '0');
            outputFile.SetFileName($"{inputFile.FileName}-{indexStr}");
            // Write file, if able
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                EnsureDirectoriesExist(outputFile);
                WriteTextureAsImage(options, outputFile, emblem.Texture, encoder);
            }
        }
    }

    /// <summary>
    ///     Compile an emblem binary archive from multiple images.
    /// </summary>
    /// <param name="options"></param>
    public static void EmblemsBinFromImages(Options options)
    {
        Terminal.WriteLine("Emblem: converting image(s) to emblem.bin.");
        var emblems = ImageToEmblemBin(options);
        Terminal.WriteLine($"Emblem: done converting {emblems.Length} image{(emblems.Length != 1 ? 's' : "")}.");
    }

    /// <summary>
    ///     Convert single image to emblem.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if resize is greater than max emblem size.</exception>
    public static Emblem ImageToEmblemBin(Options options, OSPath inputFile)
    {
        // Make sure some option parameters are appropriate
        bool isTooLarge = options.IsSizeTooLarge(Emblem.Width, Emblem.Height);
        if (isTooLarge)
        {
            string msg =
                $"Requested resize ({options.Width},{options.Height}) exceeds the maximum " +
                $"bounds of an emblem ({Emblem.Width},{Emblem.Height}).";
            throw new ArgumentException(msg);
        }

        // Load image, get resize parameters, resize image
        Image<Rgba32> image = Image.Load<Rgba32>(inputFile);
        ResizeOptions resizeOptions = GetEmblemResizeOptions(options, image.Width, image.Height, Emblem.Width, Emblem.Height, options.EmblemHasAlphaBorder);
        image.Mutate(ipc => ipc.Resize(resizeOptions));
        // Create emblem, convert image to texture
        Emblem emblem = new()
        {
            Texture = ImageAsCenteredTexture(image, Emblem.Width, Emblem.Height)
        };

        // Write some useful information to the terminal
        // TODO: unify with new CheckWillFileWrite method?
        lock (Terminal.Lock)
        {
            Terminal.Write($"Emblem: ");
            Terminal.Write($"processing image ");
            Terminal.Write(inputFile, GfzCli.FileNameColor);
            Terminal.Write($" ({image.Width},{image.Height}).");
            Terminal.WriteLine();
        }

        return emblem;
    }

    /// <summary>
    ///     Convert multiple images to emblems.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static Emblem[] ImageToEmblemBin(Options options)
    {
        // Get emblems
        var emblems = ParallelizeFileInTypeOutTasks(options, ImageToEmblemBin);
        OSPath outputPath = new(EnforceUnixSeparators(options.OutputPath));

        // Write file, if able
        bool doWriteFile = CheckWillFileWrite(options, outputPath, out ActionTaskResult result);
        PrintFileWriteResult(result, outputPath, options.ActionStr);
        if (doWriteFile)
        {
            //using var fileStream = File.Create(outputPath);
            //using var writer = new EndianBinaryWriter(fileStream, EmblemBIN.endianness);
            EmblemBIN emblemBin = new();
            emblemBin.Value.Emblems = emblems;
            emblemBin.WriteFile(outputPath);
            //emblemBin.Serialize(writer);
        }

        // Return emblems to caller
        return emblems;
    }

    #endregion

    #region GCI

    /// <summary>
    ///     Extract images from GCI emblem save files.
    /// </summary>
    /// <param name="options"></param>
    public static void EmblemGciToImage(Options options)
    {
        // In this case where no search pattern is set, find *FZE*.GCI (emblem) files.
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = "*fze*.dat.gci";

        Terminal.WriteLine("Emblem: converting emblems from GCI files.");
        int gciCount = ParallelizeFileInFileOutTasks(options, EmblemGciToImage);
        Terminal.WriteLine($"Emblem: done converting {gciCount} file{Plural(gciCount)}.");
    }

    /// <summary>
    ///     Extract image from GCI emblem save file.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    private static void EmblemGciToImage(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Read GCI Emblem data
        var emblemGCI = new EmblemGCI();
        using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), EmblemGCI.endianness))
        {
            emblemGCI.Deserialize(reader);
            emblemGCI.FileName = Path.GetFileNameWithoutExtension(inputFile);
        }

        // Prepare image encoder
        ImageEncoder encoder = options.ImageEncoder;
        // Strip .dat.gci extensions
        outputFile.SetExtensions("png");

        // BANNER
        {
            OSPath texturePath = new(outputFile);
            texturePath.SetFileName($"{outputFile.FileName}-banner");
            // Write file, if able
            bool doWriteFile = CheckWillFileWrite(options, texturePath, out ActionTaskResult result);
            PrintFileWriteResult(result, texturePath, options.ActionStr);
            if (doWriteFile)
            {
                WriteTextureAsImage(options, texturePath, emblemGCI.Banner, encoder);
            }
        }

        // ICON
        for (int i = 0; i < emblemGCI.Icons.Length; i++)
        {
            var icon = emblemGCI.Icons[i];
            // Strip original file name, replace with GC game code
            OSPath texturePath = new(outputFile);
            texturePath.SetFileName($"{emblemGCI.Header}-icon{i}");
            // Write file, if able
            bool doWriteFile = CheckWillFileWrite(options, texturePath, out ActionTaskResult result);
            PrintFileWriteResult(result, texturePath, options.ActionStr);
            if (doWriteFile)
            {
                WriteTextureAsImage(options, texturePath, icon, encoder);
            }
        }

        // EMBLEM
        {
            // Write file, if able
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                WriteTextureAsImage(options, outputFile, emblemGCI.Emblem.Texture, encoder);
            }
        }
    }

    /// <summary>
    ///     Create a GCI emblem save file from one image.
    /// </summary>
    /// <param name="options"></param>
    public static void EmblemGciFromImage(Options options)
    {
        // In this case where no search pattern is set, find *fze*.dat.gci (emblem) files.
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = "*fze*.dat.gci";

        Terminal.WriteLine("Emblem: converting image(s) to emblem.dat.gci.");
        int gciCount = ParallelizeFileInFileOutTasks(options, ImageToEmblemGci);
        Terminal.WriteLine($"Emblem: done converting {gciCount} image{Plural(gciCount)}.");
    }

    /// <summary>
    ///     Create a GCI emblem save file from one image.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    public static void ImageToEmblemGci(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Load image
        Image<Rgba32> emblemImage = Image.Load<Rgba32>(inputFile);
        Image<Rgba32> iconImage = emblemImage.Clone();
        // Get resize targets
        ResizeOptions emblemResize = GetEmblemResizeOptions(options, emblemImage.Width, emblemImage.Height, Emblem.Width, Emblem.Height, options.EmblemHasAlphaBorder);
        ResizeOptions iconResize = GetEmblemResizeOptions(options, emblemImage.Width, emblemImage.Height, EmblemGCI.IconWidth, EmblemGCI.IconHeight, false);
        // Resize images
        emblemImage.Mutate(ipc => ipc.Resize(emblemResize));
        iconImage.Mutate(ipc => ipc.Resize(iconResize));

        // Construct data for GCI
        Texture emblemTexture = ImageAsCenteredTexture(emblemImage, Emblem.Width, Emblem.Height);
        Texture iconTexture = ImageAsCenteredTexture(iconImage, EmblemGCI.IconWidth, EmblemGCI.IconHeight);
        Texture banner = new(EmblemGCI.BannerWidth, EmblemGCI.BannerHeight, EmblemGCI.DirectFormat);
        // todo: blank banner!
        Texture[] icons = [iconTexture];
        Emblem emblem = new(emblemTexture);
        EmblemGCI emblemGci = new(options.Region);
        options.ThrowIfInvalidRegion();

        // Get name for output file
        string gciFileName = EmblemGCI.FormatGciFileName(GfzGciFileType.Emblem, emblemGci.Header, outputFile.FileName, out string fileName);
        outputFile.SetFileName(gciFileName);

        // Assign data
        emblemGci.Emblem = emblem;
        emblemGci.SetBanner(banner);
        emblemGci.SetIcons(icons);
        emblemGci.SetFileName(fileName);

        // Write file
        bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
        PrintFileWriteResult(result, outputFile, options.ActionStr);
        if (doWriteFile)
        {   
            // Save emblem
            using var fileStream = File.Create(outputFile);
            using var writer = new EndianBinaryWriter(fileStream, EmblemGCI.endianness);
            emblemGci.Serialize(writer);
        }
    }

    #endregion

    /// <summary>
    ///     Create <see cref="ResizeOptions"/> from data within <paramref name="options"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="imageWidth"></param>
    /// <param name="imageHeight"></param>
    /// <param name="resizeWidth"></param>
    /// <param name="resizeHeight"></param>
    /// <param name="resizeHasAlphaBorder"></param>
    /// <returns></returns>
    private static ResizeOptions GetEmblemResizeOptions(Options options, int imageWidth, int imageHeight, int resizeWidth, int resizeHeight, bool resizeHasAlphaBorder)
    {
        // Resize image to fit inside bounds of image.
        // eg: emblem is 64x64
        ResizeOptions resizeOptions = options.GetResizeOptions();

        // Emblem size is either 62x62 (1px alpha border, as intended) or 64x64 ("hacker" option)
        if (resizeHasAlphaBorder)
        {
            resizeWidth -= 2;
            resizeHeight -= 2;
        }
        // Choose lowest dimensions as the default size (ie: preserve pixel-perfect if possible)
        int defaultX = Math.Min(resizeWidth, imageWidth);
        int defaultY = Math.Min(resizeHeight, imageHeight);
        // Set size override, then resize image
        resizeOptions.Size = options.GetResizeSize(defaultX, defaultY);

        return resizeOptions;
    }

    /// <summary>
    ///     Move <paramref name="image"/> texture to be centered inside
    ///     <paramref name="boundsX"/> and <paramref name="boundsY"/>.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="boundsX"></param>
    /// <param name="boundsY"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if image size is greater than bounds.</exception>
    private static Texture ImageAsCenteredTexture(Image<Rgba32> image, int boundsX, int boundsY)
    {
        bool isInvalidSize = image.Width > boundsX || image.Height > boundsY;
        if (isInvalidSize)
        {
            string msg =
                $"Image size ({image.Width}, {image.Height}) cannot be " +
                $"larger than bounds ({boundsX}, {boundsY}).";
            throw new ArgumentException(msg);
        }

        Texture imageAsTexture = ImageToTexture(image, TextureFormat.RGB5A3);
        Texture centeredTexture = new(boundsX, boundsY, TextureColor.Clear, TextureFormat.RGB5A3);

        // Copy image texture to emblem center
        // Only works if image is less than bounds!
        int offsetX = (boundsX - image.Width) / 2;
        int offsetY = (boundsX - image.Height) / 2;
        Texture.Copy(imageAsTexture, centeredTexture, offsetX, offsetY);

        return centeredTexture;
    }

}
