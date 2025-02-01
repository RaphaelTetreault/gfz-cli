using GameCube.GFZ.TPL;
using GameCube.GX.Texture;
using Manifold.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Manifold.GFZCLI.GfzCliUtilities;
using static Manifold.GFZCLI.GfzCliImageUtilities;


namespace Manifold.GFZCLI;

public static class ActionsTPL
{
    public static readonly GfzCliAction ActionTplGenerateMipmaps = new()
    {
        Description = "TEMPORARY! Create mipmap images of source image.",
        Action = TplGenerateMipmaps,
        ActionID = CliActionID.tpl_generate_mipmaps,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            IOptionsImageSharp.Arguments.Resampler,
            IOptionsImageSharp.Arguments.ImageFormat,
        ],
    };

    public static readonly GfzCliAction ActionTplPack = new()
    {
        Description = "WORK IN PROGRESS. Pack a directory into a TPL file.",
        Action = TplPack,
        ActionID = CliActionID.tpl_pack,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionTplUnpack = new()
    {
        Description = "Unpack a TPL file into a directory of images.",
        Action = TplUnpack,
        ActionID = CliActionID.tpl_unpack,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    [Obsolete("Poor quality code.")]
    public static void TplUnpack(Options options)
    {
        // Force search TPL files IF there is no defined search pattern
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = "*.tpl";

        Terminal.WriteLine("TPL: unpacking file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, TplUnpackFile);
        Terminal.WriteLine($"TPL: done unpacking {taskCount} TPL file{(taskCount != 1 ? 's' : "")}.");
    }

    [Obsolete("Poor quality code.")]
    public static void TplUnpackFile(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Deserialize the TPL
        Tpl tpl = new();
        using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), Tpl.endianness))
        {
            tpl.Deserialize(reader);
            tpl.FileName = Path.GetFileNameWithoutExtension(inputFile);
        }

        // Create folder named the same thing as the TPL input file
        string directory = Path.GetDirectoryName(outputFile);
        string outputDirectory = Path.Combine(directory, tpl.FileName);
        Directory.CreateDirectory(outputDirectory);

        // Prepare image encoder
        var encoder = options.ImageEncoder;
        outputFile.SetExtensions(options.ImageExtension);
        outputFile.PushDirectory(tpl.FileName);

        // Iterate over texture and mipmaps, save to disk
        int tplIndex = 0;
        foreach (var textureBundle in tpl.TextureBundles)
        {
            tplIndex++;

            if (textureBundle is null)
                continue;

            int mipmapIndex = -1;
            int entryIndex = -1;
            foreach (var textureEntry in textureBundle.Elements)
            {
                entryIndex++;

                bool isMipmap = mipmapIndex > 0;
                bool skipMipmaps = isMipmap && !options.TplUnpackMipmaps;
                if (skipMipmaps)
                    continue;

                mipmapIndex++;

                // Optionally bow out of saving texture if it is corrupted.
                bool skipCorruptedTexture = textureEntry.IsCorrupted && !options.TplUnpackSaveCorruptedTextures;
                if (skipCorruptedTexture)
                    continue;

                // 
                var texture = textureEntry.Texture;
                string textureHash = textureBundle.Elements[entryIndex].Crc32Text;
                OSPath textureOutput = new(outputFile);
                textureOutput.SetFileName($"{tplIndex}-{mipmapIndex}-{texture.Format}-{textureHash}");

                // Write file
                bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
                PrintFileWriteResult(result, outputFile, options.ActionStr);
                if (doWriteFile)
                {
                    // Copy contents of GameCube texture into ImageSharp representation
                    Image<Rgba32> image = TextureToImage(texture);
                    // Save to disk
                    image.Save(textureOutput, encoder);
                }
            }
        }
    }

    [Obsolete("Poor quality code.")]
    public static void TplPack(Options options)
    {
        string path = options.InputPath;
        if (string.IsNullOrEmpty(path))
            return;

        // Ensure input file exists
        bool fileExists = File.Exists(path);
        if (!fileExists)
            throw new ArgumentException($"Target file '{path}' does not exist.");

        // TEMP: just do 1 file
        Image<Rgba32> image = Image.Load<Rgba32>(path);
        Texture texture = new(image.Width, image.Height, TextureFormat.CMPR);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Rgba32 pixel = image[x, y];
                texture[x, y] = new TextureColor(pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        string directory = Path.GetDirectoryName(path);
        string fileName = $"temp.tpl";
        string filePath = Path.Combine(directory, fileName);
        //using (var writer = new EndianBinaryWriter(new MemoryStream(), Tpl.endianness))
        using var writer = new EndianBinaryWriter(File.Create(filePath), Tpl.endianness);
        var encoding = new EncodingCMPR(BCnEncoder.Encoder.CompressionQuality.BestQuality);
        //var encoding = Encoding.EncodingRGBA8;
        //var encoding = Encoding.EncodingRGB565;
        //var encoding = Encoding.EncodingRGB5A3;
        //var encoding = Encoding.EncodingIA8;
        //var encoding = Encoding.EncodingIA4;
        var blocks = Texture.CreateDirectColorBlocksFromTexture(texture, encoding, out int bch, out int bcv);
        encoding.WriteTexture(writer, blocks);
        writer.Flush();

        writer.BaseStream.Position = 0;
        using var reader = new EndianBinaryReader(writer.BaseStream, Tpl.endianness);
        var blocksCopy = encoding.ReadBlocks<DirectBlock>(reader, encoding, blocks.Length);
        Texture textureCopy = Texture.FromDirectBlocks(blocksCopy, bch, bcv);

        // HACK - copy/paste garbage test
        // Copy contents of GameCube texture into ImageSharp representation
        Image<Rgba32> imageCopy = new(textureCopy.Width, textureCopy.Height);
        for (int y = 0; y < textureCopy.Height; y++)
        {
            for (int x = 0; x < textureCopy.Width; x++)
            {
                TextureColor pixel = textureCopy[x, y];
                imageCopy[x, y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }

        //var tempStream = new MemoryStream();
        //var format = PngFormat.Instance;
        //imageCopy.Save(tempStream, format);
        //var imageHash = GetMD5Hastpl-packhName(tempStream);

        // Find where to save file
        directory = Path.GetDirectoryName(path);
        fileName = $"temp.png";
        filePath = Path.Combine(directory, fileName);
        // Save to disk
        imageCopy.SaveAsPng(filePath);
        Terminal.WriteLine($"Wrote file: {filePath}");
    }

    [Obsolete("Poor quality code.")]
    public static void TplGenerateMipmaps(Options options)
    {
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = "*.png";

        Terminal.WriteLine("TPL: generating mipmaps.");
        int taskCount = ParallelizeFileInFileOutTasks(options, TplGenerateMipmaps);
        Terminal.WriteLine($"TPL: done generating mipmaps for {taskCount} file{(taskCount != 1 ? 's' : "")}.");
    }

    [Obsolete("Poor quality code.")]
    public static void TplGenerateMipmaps(Options options, OSPath inputFilePath, OSPath outputFilePath)
    {
        // Check to see if file can be loaded, error if not.
        using var sourceImage = File.OpenRead(inputFilePath);
        try
        {
            IImageFormat imageFormat = Image.DetectFormat(sourceImage);
        }
        catch (Exception e)
        {
            StringBuilder supportedTypes = new();
            foreach (var type in Enum.GetNames<ImageFormat>())
                supportedTypes.Append($" {type}");

            string msg =
                $"File {inputFilePath} is invalid {inputFilePath.Extension}. " +
                $"Use supported types{supportedTypes}." +
                $"\n{e.Message}";

            throw new ArgumentException(msg);
        }

        // Load image
        using var image = Image.Load(sourceImage);
        // Prepare file and encoding information
        IResampler resampler = options.Resampler;
        ImageEncoder imageEncoder = options.ImageEncoder;
        List<OSPath> mipmapNames = [outputFilePath];
        TplTextureName baseTextureName = new(outputFilePath.FileName);

        // Calculate number of texture levels (1 main tex + mipmap count)
        int numberOfLevels = 1 + Texture.GetMaxMipmapCount(image.Width, image.Height);

        // Create mipmaps
        for (int mipmapLevel = 1; mipmapLevel < numberOfLevels; mipmapLevel++)
        {
            // Check to see if mipmap already exists, skip if so
            // NOTE: ignores 'name' and file extension
            TplTextureName searchPattern = new()
            {
                TplIndex = baseTextureName.TplIndex,
                TextureLevel = mipmapLevel,
                TextureFormat = baseTextureName.TextureFormat,
                Name = $"*",
            };
            string[] matches = Directory.GetFiles(outputFilePath.Directories, searchPattern);
            if (matches != null && matches.Length > 0)
            {
                mipmapNames.Add(new());
                continue;
            }

            // Create name for this mipmap based on main texture
            TplTextureName mipmapName = new()
            {
                TplIndex = baseTextureName.TplIndex,
                TextureLevel = mipmapLevel,
                TextureFormat = baseTextureName.TextureFormat,
                Name = "generated",
            };
            OSPath mipmapPath = outputFilePath.Copy();
            mipmapPath.SetFileName(mipmapName);
            mipmapNames.Add(mipmapPath);

            // Actual code which writes mipmaps
            bool doWriteFile = CheckWillFileWrite(options, mipmapPath, out ActionTaskResult result);
            PrintFileWriteResult(result, mipmapPath, options.ActionStr);
            if (doWriteFile)
            {
                // Compute w/h for this iteration
                int resizeWidth = image.Width >> mipmapLevel;
                int resizeHeight = image.Height >> mipmapLevel;

                // Create resized clone
                var imageCopy = image.Clone(c => c.Resize(resizeWidth, resizeHeight, resampler));

                // Save out mipmap
                string o = mipmapNames[mipmapLevel];
                using var mipmapFile = File.Create(o);
                imageCopy.Save(mipmapFile, imageEncoder);
            }
        }
    }
}
