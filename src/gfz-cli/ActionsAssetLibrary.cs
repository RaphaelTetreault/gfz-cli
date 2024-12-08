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
    private const string Designator = "Asset Library";

    /// <summary>
    ///     Create library of individual textures and models from TPLs and GMAs, respectively.
    ///     Library includes files which correlate textures to each model using named references.
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentException">Thrown if input or output are files.</exception>
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
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="resampler">Pre-cached image resampler.</param>
    /// <returns>
    ///     Runs an action on all texture bundles (TPLs).
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
                builder.Append($"{textureEntry.CRC32}-");
            string name = builder.ToString()[..^1]; // removes last dash
            textureNames[i] = name;

            // Create final output path
            OSPath imageOutputPath = outputPath.Copy();
            imageOutputPath.SetFileName(name);
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

        // To be used to map GMA texture indexes to specific image files.
        return textureNames;
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
        Image<Rgba32> image = new(width, height, new(0, 0, 0));
        // Where to draw within the larger texture, changes with each write (so not to overlap)
        Point offset = new(0, 0);

        // Always process main texture
        var mainTexture = TextureToImage(textureBundle.Elements[0].Texture);
        image.Mutate(c => c.DrawImage(mainTexture, 1f));
        offset.X = mainTexture.Width;

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
            else // is corrupted
            {
                // Otherwise is corrupted, generate new mipmap
                int resizeWidth = mainTexture.Width >> i;
                int resizeHeight = mainTexture.Height >> i;
                // If texture does not even have data, break loop
                if (resizeWidth == 0 || resizeHeight == 0)
                    break;
                // Resize texture
                var generatedMipmap = image.Clone(c => c.Resize(resizeWidth, resizeHeight, resampler));
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
    ///     Writes single texture bundle (texture with mipmaps) as single <see cref="GxTexture"/>.
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
        GxTexture gxTex = new()
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
        using var writer = new EndianBinaryWriter(File.Create(fullOutputPath), GxTexture.endianness);
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

                // Apply as normal
                if (tplTextureIndex < gmaTextures.Length)
                    tevTextureReferences[index] = gmaTextures[tplTextureIndex];
                // weird stuff with vehicle textures, basically write an error
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
            gmarefOutputPath.SetExtensions("gmaref");
            string directories = Path.GetDirectoryName(inputPath)![options.InputPath.Length..];
            gmarefOutputPath.PushDirectories(directories);

            bool doWriteWrite = CheckWillFileWrite(options, gmarefOutputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, gmarefOutputPath, options.ActionStr);
            if (doWriteWrite)
            {
                // Create .GMAREF file
                EnsureDirectoriesExist(gmarefOutputPath);
                using var writer = new StreamWriter(File.Create(gmarefOutputPath));
                // Write a reference to each GCMF for this GMA file as a .GMAREF
                int padWidth = gcmfAssetNames.Count.ToString().Length;
                for (int i = 0; i < gcmfAssetNames.Count; i++)
                    writer.Write($"{i.PadLeft(padWidth)}:\t{gcmfAssetNames[i]}\n");
            }
        }
    }
}