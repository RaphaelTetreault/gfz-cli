using GameCube.GFZ.GMA;
using GameCube.GFZ.TPL;
using GameCube.GX.Texture;
using static Manifold.GFZCLI.GfzCliUtilities;
using static Manifold.GFZCLI.GfzCliImageUtilities;
using Manifold.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Manifold.GFZCLI;

/// <summary>
///     Actions for creating a litghtly-managed GFZ asset library.
/// </summary>
public static class ActionsAssetLibrary
{
    public static readonly GfzCliAction ActionAssetGenerateLibrary = new()
    {
        Description = "Create a text-reference-linked GMA and TPL library.",
        Action = CreateGmaTplLibrary,
        ActionID = CliActionID.asset_generate_library,
        InputIO = CliActionIO.Directory,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            IOptionsImageSharp.Arguments.Resampler,
            ],
    };

    public static readonly GfzCliAction ActionAssetImageToGxtex = new()
    {
        Description = "Convert image to a raw GameCube GX texture.",
        Action = ImageToGxTexture,
        ActionID = CliActionID.asset_image_to_gxtex,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = false,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [
            IOptionsTpl.Arguments.TextureFormat,
            IOptionsImageSharp.Arguments.Width, // Size.X
            IOptionsImageSharp.Arguments.Height, // Size.Y
            IOptionsImageSharp.Arguments.Compand,
            IOptionsImageSharp.Arguments.PadColor,
            IOptionsImageSharp.Arguments.Position,
            IOptionsImageSharp.Arguments.PremultiplyAlpha,
            IOptionsImageSharp.Arguments.Resampler,
            IOptionsImageSharp.Arguments.ResizeMode, // Mode
            ],
    };

    private const string Designator = "Asset Library";

    /// <summary>
    ///     Create library of individual textures and models from TPLs and GMAs, respectively.
    ///     Library includes files which correlate textures to each model using named references.
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentException">Thrown if input or output are files.</exception>
    /// <remarks>
    ///     Output not optional. Input is directory, ideally root of ISO directory.
    /// <list type="bullet">
    ///     <item>
    ///         <term>TPL</term>
    ///         <description>
    ///             Creates .PNG and .GXTEX files for each image in TPLs. Duplicates are
    ///             not written using CRC32 hashes of the image data.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>GMA</term>
    ///         <description>
    ///             Creates .GMAREF files which map GMA to individual GCMF files. Creates 
    ///             .GCMFX files for each GCMF in models. Duplicates are not written using
    ///             CRC32 hashes of the model data.
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    public static void CreateGmaTplLibrary(Options options)
    {
        // Assert that destination is a folder.
        bool isInputFile = File.Exists(options.InputPath);
        bool isOutputFile = File.Exists(options.OutputPath);
        if (isInputFile || isOutputFile)
        {
            string msg = $"Incorrect command usage.";
            throw new ArgumentException(msg);
        }

        Terminal.WriteLine($"{Designator}: generating asset library.");
        CreateGmaTplLibrary(options, new(options.InputPath), new(options.OutputPath + "/"));
        Terminal.WriteLine($"{Designator}: done.");
    }

    /// <summary>
    ///     Create library of individual textures and models from TPLs and GMAs, respectively.
    ///     Library includes files which correlate textures to each model using named references.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="_">Input path (unused).</param>
    /// <param name="outputPath"></param>
    public static void CreateGmaTplLibrary(Options options, OSPath _, OSPath outputPath)
    {
        // Copy original argument
        string searchPattern = options.SearchPattern;
        // Get GMA file paths
        options.SearchPattern = "*.gma";
        string[] gmaFiles = GetInputFiles(options);
        // Get TPL file paths
        options.SearchPattern = "*.tpl";
        List<string> tplFiles = GetInputFiles(options).ToList();
        // Restore search pattern
        options.SearchPattern = searchPattern;

        // Clear file paths of extension since it's implied in variable names
        for (int i = 0; i < gmaFiles.Length; i++)
            gmaFiles[i] = Path.ChangeExtension(gmaFiles[i], string.Empty);
        for (int i = 0; i < tplFiles.Count; i++)
            tplFiles[i] = Path.ChangeExtension(tplFiles[i], string.Empty);

        // Get image resampler
        IResampler resampler = options.Resampler;

        // Create copies of paths
        OSPath tplOutputPath = outputPath.Copy();
        OSPath gmaOutputPath = outputPath.Copy();
        // Mutate copies (add subdirectory to path)
        tplOutputPath.PushDirectory("tex");
        gmaOutputPath.PushDirectory("mdl");

        // Create TPL textures alongside GMA models
        foreach (var assetFile in gmaFiles)
        {
            // Get path to TPL
            OSPath tplPath = new(assetFile);
            tplPath.SetExtensions("tpl");
            // GMA path is same as TPL, just with GMA extension
            OSPath gmaPath = tplPath.Copy();
            gmaPath.SetExtensions("gma");

            // Check: does GMA have a TPL file beside it in the directory?
            bool hasTpl = tplFiles.Contains(assetFile);
            if (hasTpl)
            {
                // Remove file from future processing
                tplFiles.Remove(assetFile);

                // Write out textures
                var textureNames = TplToGxtexAndPng(options, tplPath, tplOutputPath, resampler);
                // Write out models with texture references :)
                WriteModels(options, gmaPath, gmaOutputPath, textureNames);
            }
            else
            {
                // TODO
                // GMA uses common TPL, write it out without texture references
                WriteModels(options, gmaPath, gmaOutputPath, []);
            }
        }

        // Extract remaining TPLs without associated GMA
        foreach (var tplFile in tplFiles)
        {
            // Get path to TPL
            OSPath tplFilePath = new(tplFile);
            tplFilePath.SetExtensions("tpl");
            // Write out textures
            TplToGxtexAndPng(options, tplFilePath, tplOutputPath, resampler);
        }
    }


    /// <summary>
    ///     Create a .GXTEX and preview .PNG from a source image.
    /// </summary>
    /// <param name="options"></param>
    public static void ImageToGxTexture(Options options)
    {
        // TODO: ingject search pattern?

        Terminal.WriteLine($"{Designator}: converting image to GameCube GX texture.");
        ParallelizeFileInFileOutTasks(options, ImageToGxTexture);
        Terminal.WriteLine($"{Designator}: done.");
    }

    /// <summary>
    ///     Create a .GXTEX and preview .PNG from a source image.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputPath">Input image path.</param>
    /// <param name="outputPath">Output .GXTEX and .PNG path.</param>
    public static void ImageToGxTexture(Options options, OSPath inputPath, OSPath outputPath)
    {
        // Load image
        Image<Rgba32> image = (Image<Rgba32>)Image.Load(inputPath);
        // Resize image if specified
        IResampler resampler = options.Resampler;
        bool doResize = options.Width > 0 || options.Height > 0;
        if (doResize)
        {
            var resizeOptions = IOptionsImageSharp.GetResizeOptions(options);
            resizeOptions.Size = IOptionsImageSharp.GetResizeSize(options, image);
            image.Mutate(img => img.Resize(resizeOptions));
        }
        // Convert to texture
        Texture texture = ImageToTexture(image);

        // Create TextureBundle (main text + mipmaps)
        int textureCount = 1 + Texture.GetMaxMipmapCount(texture.Width, texture.Height);
        // TRICK: Set only the first texture in the bundle. The default state for
        //        Element.IsValid is false, which will force regeneration in the
        //        serialization code :)
        TextureBundleElement[] elements = new TextureBundleElement[textureCount];
        elements[0] = new TextureBundleElement(texture);
        // Init remaining elements
        for (int i = 1; i < elements.Length; i++)
            elements[i] = new();

        // Add elements to bundle, save
        TextureBundle textureBundle = new(elements, options.TextureFormat);
        SaveGxtexAndPng(options, outputPath, resampler, textureBundle);
    }

    /// <summary>
    ///     Create a .GXTEX and .PNG for each texture in a TPL <paramref name="inputPath"/> file.
    ///     Files are stored at <paramref name="outputPath"/>. Images that have invalid mipmaps
    ///     will use <paramref name="resampler"/> to generate new mipmaps.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="resampler">Pre-cached image resampler.</param>
    /// <returns>
    ///     All CRC32 texture names, one for each texture in the TPL, with null strings for null
    ///     entries in TPL (thus, all indexes match those in the TPL).
    /// </returns>
    private static string[] TplToGxtexAndPng(Options options, OSPath inputPath, OSPath outputPath, IResampler resampler)
    {
        // Load TPL file
        Tpl tpl = BinarySerializableIO.LoadFile<Tpl>(inputPath);
        tpl.FileName = inputPath;

        // Iterate over all texture bundle (each bundle is main texture + optional mipmaps)
        int numTextures = tpl.TextureBundles.Length;
        string[] textureNames = new string[numTextures];

        for (int i = 0; i < numTextures; i++)
        {
            // Get texture bundle
            TextureBundle textureBundle = tpl.TextureBundles[i];
            // Skip if bleh
            if (textureBundle is null ||
                textureBundle.Description.IsGarbageEntry ||
                textureBundle.Description.IsNull)
                continue;

            // Output name is the hash of each texture in bundle
            StringBuilder builder = new();
            foreach (var textureEntry in textureBundle.Elements)
                builder.Append($"{textureEntry.Crc32Text}-");
            string textureCrc32sName = builder.ToString()[..^1]; // removes last dash

            // Many images are the same, but lowers mipmaps are bit-inaccurate, and so duplicates
            // of the same image are made due to different CRCs. This function weeds those out.
            // Function mutates name if neighbour exists.
            textureCrc32sName = GetSameCrc32FileNameOrMipmapBitNeighbourFileName(outputPath.Directories, textureCrc32sName);

            // Assign potentially corrected texture, and create output path with it too
            textureNames[i] = textureCrc32sName;
            OSPath targetOutputPath = outputPath.Copy();
            targetOutputPath.SetFileName(textureCrc32sName);

            // Output images
            SaveGxtexAndPng(options, targetOutputPath, resampler, textureBundle);
        }

        // To be used to map GMA texture indexes to specific image files.
        return textureNames;
    }

    /// <summary>
    ///     Creates a .GXTEX and preview .PNG from a <paramref name="textureBundle"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="outputPath"></param>
    /// <param name="resampler"></param>
    /// <param name="textureBundle"></param>
    private static void SaveGxtexAndPng(Options options, OSPath outputPath, IResampler resampler, TextureBundle textureBundle)
    {
        // Prepare output paths
        OSPath imageOutputPath = outputPath.Copy();
        imageOutputPath.SetExtensions("png");
        OSPath gxtexOutputPath = imageOutputPath.Copy();
        gxtexOutputPath.SetExtensions("gxtex");

        // PNG
        {
            bool doWriteWrite = CheckWillFileWrite(options, imageOutputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, imageOutputPath, options.ActionStr);
            if (doWriteWrite)
            {
                WriteTextureBundleAsPNG(textureBundle, imageOutputPath, resampler);
            }
        }

        // GXTEX
        {
            bool doWriteWrite = CheckWillFileWrite(options, gxtexOutputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, gxtexOutputPath, options.ActionStr);
            if (doWriteWrite)
            {
                WriteTextureBundleAsGxTexture(textureBundle, gxtexOutputPath, resampler);
            }
        }
    }

    /// <summary>
    ///     Finds the texture by name in <paramref name="directory"/>> for this texture that shares
    ///     the same main texture CRC32 and higher mipmaps, but ignores lower mipmap CRC32s as they
    ///     are sometimes bit-different, but functionally idential. If not match found, returns the
    ///     same <paramref name="crc32FileName"/>.
    /// </summary>
    /// <param name="directory">The directory to search for a match in.</param>
    /// <param name="crc32FileName">The CRC32 filename to compare against.</param>
    /// <returns>
    ///     CRC32 file name of that is the same, or that shares the same upper CRC32s.
    /// </returns>
    private static string GetSameCrc32FileNameOrMipmapBitNeighbourFileName(string directory, string crc32FileName)
    {
        // It's worth noting most (all?) CRC32 mismatches seem to happen on the last valid
        // valid mipmap, which is often 3rd from last, and on CMPR ones where compression
        // settings could be messing with that, depending on PC/CPU or other factors.

        int minLength = (8 * 3) + 2; // 3 CRC32s with 2 dashes
        bool isImageWith3OrMoreMipmaps = crc32FileName.Length > minLength;
        if (isImageWith3OrMoreMipmaps && Directory.Exists(directory))
        {
            // Find files with same starting name
            string partialCrc32FileName = $"{crc32FileName[..^minLength]}*"; // add * for wildcard
            string[] matches = Directory.GetFiles(directory, partialCrc32FileName, SearchOption.TopDirectoryOnly);
            foreach (var match in matches)
            {
                // Check to see if these are compatible. Will often times compares the same image names
                // as the image we are checking just is a plain copy of an existing one. However, this
                // will also weed out those with just 1 CRC32 mismatch.
                string matchFileName = Path.GetFileNameWithoutExtension(match);
                bool hasSameMipmapCount = matchFileName.Length == crc32FileName.Length;
                if (hasSameMipmapCount)
                    return matchFileName;
            }
        }

        // Image does not have enough mipmaps to be worth comparing, return filename
        return crc32FileName;
    }


    /// <summary>
    ///     Writes single texture bundle (texture with mipmaps) as single PNG.
    /// </summary>
    /// <param name="textureBundle"></param>
    /// <param name="fullOutputPath"></param>
    /// <param name="resampler"></param>
    private static void WriteTextureBundleAsPNG(TextureBundle textureBundle, string fullOutputPath, IResampler resampler)
    {
        // Prepare image buffer. Twice width to fit mipmaps if they exist.
        int width = textureBundle.Length > 1 ? textureBundle.Description.Width * 2 : textureBundle.Description.Width;
        int height = textureBundle.Description.Height;
        Image<Rgba32> image = new(width, height, new(0, 0, 0, 0)); // transparent, alpha images are drawn on top of this
        // Where to draw within the larger texture, changes with each write (so not to overlap)
        Point offset = new(0, 0);

        // Always process main texture
        var mainTexture = TextureToImage(textureBundle.Elements[0].Texture);
        // Apply main texture to blank image
        image.Mutate(c => c.DrawImage(mainTexture, 1f));
        // Set offset for next iteration
        offset.X = mainTexture.Width;

        // Do a check where 4:1 ratio (wide/tall) textures actually have bad lowest mipmaps
        bool isHighRatio = image.Width / image.Height > 4 || image.Height / image.Width > 4;

        // Get or generate mipmaps
        for (int i = 1; i < textureBundle.Length; i++)
        {
            // Get texture data
            TextureBundleElement textureData = textureBundle.Elements[i];
            Image<Rgba32> mipmap;

            if (textureData.IsValid)
            {
                // If valid, load as-is
                mipmap = TextureToImage(textureData.Texture);
            }
            else // is corrupted or missing
            {
                // Otherwise is corrupted, generate new mipmap
                int resizeWidth = mainTexture.Width >> i;
                int resizeHeight = mainTexture.Height >> i;
                // If texture does not even have data, break loop
                if (resizeWidth == 0 || resizeHeight == 0)
                    break;
                // Resize main texture for new mipmap
                var generatedMipmap = mainTexture.Clone(c => c.Resize(resizeWidth, resizeHeight, resampler));
                mipmap = generatedMipmap;
            }

            // Apply mipmap to texture
            image.Mutate(c => c.DrawImage(mipmap, offset, 1f));
            offset.X += mipmap.Width;
        }

        // Write out texture
        EnsureDirectoriesExist(fullOutputPath);
        PngEncoder imageEncoder = new()
        {
            CompressionLevel = PngCompressionLevel.BestCompression,
        };
        image.Save(fullOutputPath, imageEncoder);
    }

    /// <summary>
    ///     Writes single texture bundle (texture with mipmaps) as single <see cref="GxTextureAsset"/>.
    /// </summary>
    /// <param name="textureBundle"></param>
    /// <param name="fullOutputPath"></param>
    /// <param name="resampler"></param>
    private static void WriteTextureBundleAsGxTexture(TextureBundle textureBundle, string fullOutputPath, IResampler resampler)
    {
        // Break outy some data
        var description = textureBundle.Description;
        var textureEncoding = GameCube.GX.Texture.Encoding.GetEncoding(description.TextureFormat);

        // Get main texture if CMPR, will need to fix texture
        bool isCMPR = textureBundle.Description.TextureFormat == TextureFormat.CMPR;
        Image<Rgba32> mainTexture = isCMPR
            ? TextureToImage(textureBundle.Elements[0].Texture)
            : new Image<Rgba32>(1, 1);

        // Load up texture data or create it if needed
        byte actualTextureCount = 0;
        var textureBundleData = new List<byte>(textureBundle.AddressRange.Size);
        // Iterate over each texture/mipmap in bundle
        for (int i = 0; i < textureBundle.Length; i++)
        {
            TextureBundleElement textureBundleElement = textureBundle.Elements[i];
            if (textureBundleElement.IsValid)
            {
                textureBundleData.AddRange(textureBundleElement.RawTextureData);
            }
            else // is corrupted
            {
                // Otherwise is corrupted, generate new mipmap
                int resizeWidth = mainTexture.Width >> i;
                int resizeHeight = mainTexture.Height >> i;
                // If texture does not even have data, break loop
                if (resizeWidth == 0 || resizeHeight == 0)
                    break;
                // Resize texture
                Image<Rgba32> mipmapImage = mainTexture.Clone(c => c.Resize(resizeWidth, resizeHeight, resampler));
                Texture mipmapTexture = ImageToTexture(mipmapImage);
                // Write texture data to memory
                using var memory = new MemoryStream();
                using var memoryWriter = new EndianBinaryWriter(memory, Tpl.endianness);
                Texture.WriteDirectColorTexture(memoryWriter, mipmapTexture, description.TextureFormat);
                memoryWriter.Flush();
                // Add data to array
                byte[] mipmapData = memory.ToArray();
                textureBundleData.AddRange(mipmapData);
            }

            // If we get this far, we know we have a real texture encoded
            actualTextureCount++;
        }

        // Prepare container
        GxTextureAsset gxTex = new()
        {
            Width = description.Width,
            Height = description.Height,
            Format = description.TextureFormat,
            Count = actualTextureCount,
            DataLength = textureBundleData.Count,
            Data = textureBundleData.ToArray(),
        };
        // Write out texture
        EnsureDirectoriesExist(fullOutputPath);
        using var writer = new EndianBinaryWriter(File.Create(fullOutputPath), GxTextureAsset.endianness);
        writer.Write(gxTex);
    }

    /// <summary>
    ///     Writes all models of a single GMA file as <see cref="GcmfAsset"/> binary and .gmaref text file.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputPath">The input GMA file.</param>
    /// <param name="outputPath">The output directory.</param>
    /// <param name="gmaTextures">This GMA's TPL texture names.</param>
    private static void WriteModels(Options options, OSPath inputPath, OSPath outputPath, string[] gmaTextures)
    {
        // Load GMA file
        Gma gma = BinarySerializableIO.LoadFile<Gma>(inputPath);
        gma.FileName = inputPath;

        //　Record names of generated files for .gmaref
        List<string> gcmfAssetNames = new();

        // Iterate over all models in GMA
        int numModels = gma.Models.Length;
        for (int i = 0; i < numModels; i++)
        {
            // Get model data
            string name = gma.Models[i].Name;
            Gcmf gcmf = gma.Models[i].Gcmf;

            // Get this GMA's texture references.
            // If no textures provided, do not get texture names
            string[] tevTextureReferences = gmaTextures.Length > 0
                ? new string[gcmf.TevLayers.Length]
                : Array.Empty<string>();
            // Iterate and assign references
            for (int index = 0; index < tevTextureReferences.Length; index++)
            {
                // The index the model wants from the TPL
                int tplTextureIndex = gcmf.TevLayers[index].TplTextureIndex;

                // Get the texture name using index
                if (tplTextureIndex < gmaTextures.Length)
                    tevTextureReferences[index] = gmaTextures[tplTextureIndex];
                // Weird stuff with vehicle textures, basically write an error
                else
                    tevTextureReferences[index] = $"dynamic-reference:{tplTextureIndex}";
            }

            // Create final output path
            OSPath modelOutputPath = outputPath.Copy();
            modelOutputPath.SetFileName($"{name}-{gcmf.CRC32}");
            modelOutputPath.SetExtensions("gcmfx");
            //
            gcmfAssetNames.Add(modelOutputPath.FileNameAndExtensions);

            // GCMFX
            // Create standalone GCMF with reference to textures!
            {
                bool doWriteWrite = CheckWillFileWrite(options, modelOutputPath, out ActionTaskResult result);
                PrintFileWriteResult(result, modelOutputPath, options.ActionStr);
                if (doWriteWrite)
                {
                    GcmfAsset gcmfAsset = new()
                    {
                        Name = name,
                        TevTextureReferences = tevTextureReferences,
                        Gcmf = gcmf,
                    };
                    EnsureDirectoriesExist(modelOutputPath);
                    using var writer = new EndianBinaryWriter(File.Create(modelOutputPath), Gma.endianness);
                    writer.Write(gcmfAsset);
                }
            }
        }

        // GMAREF
        // Create GMA ref file (plaintext)
        {
            OSPath gmarefOutputPath = outputPath.Copy();
            string fileName = Path.GetFileNameWithoutExtension(gma.FileName);
            gmarefOutputPath.SetFileName(fileName);
            gmarefOutputPath.SetExtensions(GmaRef.Extension);
            string directories = Path.GetDirectoryName(inputPath)![options.InputPath.Length..];
            gmarefOutputPath.PushDirectories(directories);

            bool doWriteWrite = CheckWillFileWrite(options, gmarefOutputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, gmarefOutputPath, options.ActionStr);
            if (doWriteWrite)
            {
                // Create .GMAREF file
                EnsureDirectoriesExist(gmarefOutputPath);
                using var writer = new PlainTextWriter(File.Create(gmarefOutputPath), GmaRef.Encoding);
                // Write a reference to each GCMF for this GMA file as a .GMAREF
                GmaRef gmaRef = new();
                gmaRef.GcmfModels = gcmfAssetNames.ToArray();
                gmaRef.Serialize(writer);
            }
        }
    }

}