using GameCube.GFZ.Asset;
using GameCube.GFZ.GMA;
using GameCube.GFZ.TPL;
using GameCube.GX.Texture;
using Manifold.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Manifold.GfzCli.GfzCliUtilities;
using static Manifold.GfzCli.GfzCliImageUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for creating a lightly-managed GFZ asset library.
/// </summary>
public static class CliActionsAsset
{
    /// <summary>
    ///     Create library of individual textures and models from TPLs and GMAs, respectively.
    ///     Library includes files which correlate textures to each model using named references.
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentException">Thrown if input or output are files.</exception>
    /// <remarks>
    ///     Output not optional. Input is directory, ideally root of ISO directory.
    ///     <list type="bullet">
    ///         <item>
    ///             <term>TPL</term>
    ///             <description>
    ///                 Creates .PNG and .GXTEX files for each image in TPLs. Duplicates are
    ///                 not written using CRC32 hashes of the image data.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>GMA</term>
    ///             <description>
    ///                 Creates .GMAREF files which map GMA to individual GCMF files. Creates 
    ///                 .GCMFX files for each GCMF in models. Duplicates are not written using
    ///                 CRC32 hashes of the model data.
    ///             </description>
    ///         </item>
    ///     </list>
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

        // Disable overwrite files as it does not work well with duplicate files across archives.
        if (options.OverwriteFiles)
        {
            string msg = "Disabled overwrite files as it would write duplicate files to disk thousands of times.";
            Terminal.WriteLine(msg);
            options.OverwriteFiles = false;
        }

        Terminal.WriteLine($"{options.ActionStr}: generating asset library.");
        CreateGmaTplLibraryIO(options, new(options.InputPath), new(options.OutputPath + "/"));
        Terminal.WriteLine($"{options.ActionStr}: done.");

        // Create library of individual textures and models from TPLs and GMAs, respectively.
        // Library includes files which correlate textures to each model using named references.
        static void CreateGmaTplLibraryIO(Options options, OSPath _, OSPath outputPath)
        {
            // Copy original argument
            string searchPattern = options.SearchPattern;
            // Get GMA file paths
            options.SearchPattern = "*.gma";
            string[] gmaFiles = GetInputFiles(options);
            // Get TPL file paths
            options.SearchPattern = "*.tpl";
            List<string> tplFiles = [.. GetInputFiles(options)];
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
                    TplEntryInfo[] tplEntryInfos = GetTplEntryInfos(tplPath, tplOutputPath);
                    string[] textureNames = tplEntryInfos.GetCrc32Names();
                    // Write out models with texture references :)
                    WriteModels(options, gmaPath, gmaOutputPath, textureNames);
                    // Write out texture info
                    SaveGxtexAndPng(options, tplEntryInfos, tplOutputPath);
                }
                else
                {
                    // TODO
                    // Some GMA use common TPL (eg. custom machines), write it out without texture references
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
                TplEntryInfo[] tplEntryInfos = GetTplEntryInfos(tplFilePath, tplOutputPath);
                SaveGxtexAndPng(options, tplEntryInfos, tplOutputPath);
            }
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.ActionAssetTplUnpack"/>
    /// </remarks>
    public static void TplUnpack(Options options)
    {
        options.OverrideSearchPatternIfUnset("*.tpl");
        Terminal.WriteLine($"{options.ActionStr}: unpacking file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, TplUnpackIO);
        Terminal.WriteLine($"{options.ActionStr}: done unpacking {taskCount} TPL file{Plural(taskCount)}.");
        // 
        static void TplUnpackIO(Options options, OSPath inputPath, OSPath outputPath)
        {
            TplEntryInfo[] tplEntryInfosNumbered;

            // input path is file
            // output path is file, convert to folder
            string outputDir = options.FormatOutputDirectory(outputPath.FileName);
            OSPath tplTextureOutputDir = outputPath.Copy();
            tplTextureOutputDir.PushDirectory(outputDir);
            tplTextureOutputDir.ClearFileName();
            tplTextureOutputDir.ClearExtensions();
            // Get tpl entry info
            TplEntryInfo[] tplEntryInfos = GetTplEntryInfos(inputPath, tplTextureOutputDir);
            tplEntryInfosNumbered = new TplEntryInfo[tplEntryInfos.Length];
            // Mutate names of all entries
            int padLength = tplEntryInfos.Length.ToString().Length;
            for (int i = 0; i < tplEntryInfos.Length; i++)
            {
                // Extract data
                string crc32Name = tplEntryInfos[i].Crc32Name;
                TextureSequence textureSequence = tplEntryInfos[i].TextureSequence;

                // Skip if texture is null
                if (string.IsNullOrWhiteSpace(crc32Name) || textureSequence is null)
                    continue;

                // Add prefix to texture name
                string indexPrefix = i.PadLeft(padLength, '0');
                tplEntryInfosNumbered[i] = new TplEntryInfo
                {
                    Crc32Name = $"{indexPrefix}-{crc32Name}",
                    TextureSequence = textureSequence,
                };
            }
            // Save out data with mutated name
            SaveGxtexAndPng(options, tplEntryInfosNumbered, tplTextureOutputDir);

            // output path is file but instead of
            OSPath tplrefOutputFile = outputPath.Copy();
            tplrefOutputFile.PushDirectory(outputDir);
            tplrefOutputFile.SetExtensions(TplRef.Extension);
            // Save out .TPLREF
            if (CanWriteFileAndPrintResult(options, tplrefOutputFile, out Stream fs))
            {
                using var writer = new PlainTextWriter(fs, TplRef.Encoding);
                TplRef tplref = new();
                tplref.Textures = tplEntryInfosNumbered.GetCrc32Names();
                tplref.Serialize(writer);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.ActionAssetTplrefPack"/>
    /// </remarks>
    public static void TplrefPack(Options options)
    {
        options.OverrideSearchPatternIfUnset($"*.{TplRef.Extension}");
        Terminal.WriteLine($"{options.ActionStr}: packing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, TplrefPackIO);
        Terminal.WriteLine($"{options.ActionStr}: done packing {taskCount} file{Plural(taskCount)} into TPL.");

        static void TplrefPackIO(Options options, OSPath inputPath, OSPath outputPath)
        {
            // Abort if not allowed to write
            outputPath.SetExtensions(TplFile.extension);
            if (!CanWriteFileAndPrintResult(options, outputPath))
                return;

            // Read TPLREF
            using var reader = new PlainTextReader(inputPath);
            TplRef tplRef = new();
            tplRef.Deserialize(reader);

            // Split off into function so GMA can reuse code
            TplrefPackValue(options, inputPath, outputPath, tplRef);
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.ActionAssetGmarefPack"/>
    /// </remarks>
    public static void GmarefPack(Options options)
    {
        options.OverrideSearchPatternIfUnset($"*.{GmaRef.Extension}");
        Terminal.WriteLine($"{options.ActionStr}: packing GMA file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, GmarefPackIO);
        Terminal.WriteLine($"{options.ActionStr}: done packing {taskCount} file{Plural(taskCount)} into GMA and TPL.");

        static void GmarefPackIO(Options options, OSPath inputPath, OSPath outputPath)
        {
            // Abort if not allowed to write
            outputPath.SetExtensions(GmaFile.extension);
            if (!CanWriteFileAndPrintResult(options, outputPath))
                return;

            // Read GMAREF
            using var reader = new PlainTextReader(inputPath);
            GmaRef gmaref = new();
            gmaref.Deserialize(reader);

            // Get path to gma models
            OSPath assetLibDir = string.IsNullOrWhiteSpace(options.AssetLibraryRoot)
                ? new(inputPath.Directories)               // use folder we are in
                : new(options.AssetLibraryRoot + "/mdl/"); // use specified directory

            // Record textures used for model.
            List<string> textures = [];
            // Load GCMFX via GMAREF, then use data in GCMFX 
            GmaFile gma = new();
            gma.Value.Models = new Model[gmaref.GcmfModels.Length];
            for (int i = 0; i < gmaref.GcmfModels.Length; i++)
            {
                // Load GCMFX data
                string gcmfAssetName = gmaref.GcmfModels[i];
                OSPath gcmfAssetPath = assetLibDir.Copy();
                gcmfAssetPath.SetFileNameAndExtensions(gcmfAssetName);
                GcmfAssetFile gcmfAsset = new(gcmfAssetPath);

                // Create model from file
                Model model = new()
                {
                    Name = gcmfAsset.Value.Name,
                    Gcmf = gcmfAsset.Value.Gcmf,
                };

                // Assign possible textures
                for (int texIndex = 0; texIndex < gcmfAsset.Value.TevTextureReferences.Length; texIndex++)
                {
                    string textureName = gcmfAsset.Value.TevTextureReferences[texIndex];

                    // Only add if not present -- no duplicates
                    if (!textures.Contains(textureName))
                    {
                        // Assign new texture index and add to list
                        int newIndex = textures.Count;
                        model.Gcmf.TevLayers[texIndex].TplTextureIndex = (ushort)newIndex;
                        textures.Add(textureName);
                    }
                    // Exists, but must assign index to TEV layer
                    else
                    {
                        int existingIndex = textures.IndexOf(textureName);
                        model.Gcmf.TevLayers[texIndex].TplTextureIndex = (ushort)existingIndex;
                    }
                }

                // Assign model to GMA
                gma.Value.Models[i] = model;
            }

            // Write GMA file
            GmaFile gmaFile = new();
            gmaFile.Value = gma;
            gmaFile.WriteFile(outputPath);

            // Convert texture list to TplRef to reuse function and generate final TPL
            TplRef tplRef = new();
            tplRef.Textures = textures.ToArray();
            TplrefPackValue(options, inputPath.Copy(), outputPath.Copy(), tplRef);
        }
    }

    /// <summary>
    ///     Create a .GXTEX with custom mipmaps and preview .PNG from a source images.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.ActionAssetCustomMipmapGxtex"/>
    /// </remarks>
    public static void ImagesToCustomMipmapGxtex(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: converting image(s) to GameCube GX texture.");
        int taskCount = ParallelizeFileInFileOutTasks(options, ImageToGxTexture);
        Terminal.WriteLine($"{options.ActionStr}: done.");
    }

    /// <summary>
    ///     Create a .GXTEX and preview .PNG from a source image.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="CliActionDB.ActionAssetImageToGxtex"/>
    /// </remarks>
    public static void ImageToGxTexture(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: converting image to GameCube GX texture.");
        int taskCount = ParallelizeFileInFileOutTasks(options, ImageToGxTexture);
        Terminal.WriteLine($"{options.ActionStr}: done unpacking {taskCount} TPL file{Plural(taskCount)}.");
    }



    private static void TplrefPackValue(Options options, OSPath inputPath, OSPath outputPath, TplRef tplRef)
    {
        // Get path to tpl textures
        OSPath assetLibDir = string.IsNullOrWhiteSpace(options.AssetLibraryRoot)
            ? new(inputPath.Directories)               // use folder we are in
            : new(options.AssetLibraryRoot + "/tex/"); // use specified directory

        // Load GXTEXs
        int texCount = tplRef.Textures.Length;
        TextureSequenceDescription[] descs = new TextureSequenceDescription[texCount];
        GxTextureAsset[] gxTextures = new GxTextureAsset[texCount];
        for (int i = 0; i < texCount; i++)
        {
            // Init description
            descs[i] = new TextureSequenceDescription();

            // Skip missing texture entries
            string textureName = tplRef.Textures[i];
            if (string.IsNullOrWhiteSpace(textureName))
                continue;

            // Get texture path
            OSPath texturePath = assetLibDir.Copy();
            texturePath.SetFileName(textureName);
            texturePath.SetExtensions(GxTextureAssetFile.extension);
            // Make sure file exists
            if (!File.Exists(texturePath))
            {
                string msg = $"Could not find {inputPath} file #{i} \"{texturePath}\".";
                Terminal.WriteLine(msg);
                continue;
                // TODO: I suspect a null reference shortly after this
                // because of continue... did not have time to test.
            }
            // Load texture
            gxTextures[i] = new GxTextureAssetFile(texturePath);
            // Update texture description
            descs[i] = gxTextures[i].GetDescription();
        }

        // HACK BUT GOOD? TODO: maybe put in GFZ.TPL class?
        // Hack up a TPL. First, write out descriptions and padding. Reuse existing code.
        // This is done to bypass lossy conversions here of BIN > TEX > BIN, especially CMPR.
        TplFile tplFile = new();
        tplFile.Value.TextureSequenceDescriptions = descs;
        tplFile.Value.TextureSequences = [];
        using var writer = new EndianBinaryWriter(File.Create(outputPath), TplFile.endianness);
        tplFile.Serialize(writer);
        // Now keep using writer and just write out texture data raw and update desc pointers
        for (int i = 0; i < gxTextures.Length; i++)
        {
            if (gxTextures[i] is null || gxTextures[i].Data is null)
                continue;

            descs[i].TextureSequencePtr = writer.GetPositionAsPointer();
            writer.Write(gxTextures[i].Data);
        }
        // Go back to start, write description data again to update pointers to textures.
        writer.SeekBegin();
        tplFile.Serialize(writer);
        // Done! B)
    }

    /// <summary>
    ///     Create a .GXTEX and preview .PNG from a source image or images.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="_">Input image path.</param>
    /// <param name="outputPath">Output .GXTEX and .PNG path.</param>
    private static void ImageToGxTexture(Options options, OSPath _, OSPath outputPath)
    {
        // Load main texture, mipmap paths
        var images = GetMainTextureAndMipmapImages(options);
        var mainImage = images[0];

        // Get output texture size for main texture
        var resizeOptions = options.GetResizeOptions();
        resizeOptions.Size = options.GetResizeSize(mainImage);

        // Create texture + texture sequence
        int texCount = 1 + GetMipmapCount(options, resizeOptions.Size.Width, resizeOptions.Size.Height);
        TextureSequenceElement[] elements = new TextureSequenceElement[texCount];
        for (int i = 0; i < texCount; i++)
        {
            // Get correct image for mipmap based on mode
            int textureIndex = GetMipmapByIndex(i, images.Length, options.MipmapMode);
            // Clone images and resize it
            var imageClone = images[textureIndex].Clone();
            imageClone.Mutate(img => img.Resize(resizeOptions));
            // Convert to texture
            Texture texture = ImageToTexture(imageClone);
            byte[] rawData = texture.GetRawBytes(options.TextureFormat);
            elements[i] = new TextureSequenceElement()
            {
                IsValid = true,
                Texture = texture,
                RawTextureData = rawData,
            };
            // Update future size for mipmaps
            resizeOptions.Size = new Size(resizeOptions.Size.Width >> 1, resizeOptions.Size.Height >> 1);
        }

        // Add elements to sequence, save
        TextureSequence textureSequence = new(elements, options.TextureFormat);
        SaveGxtexAndPng(options, outputPath, resizeOptions.Sampler, textureSequence);
    }

    private static int GetMipmapCount(Options options, int width, int height)
    {
        int mipmapCountMax = Texture.GetMaxMipmapCount(width, height);
        int mipmapCount = options.MipmapCount < 0 ? mipmapCountMax : Math.Clamp(options.MipmapCount, 0, mipmapCountMax);
        return mipmapCount;
    }

    private static OSPath[] GetMipmapPaths(Options options)
    {
        string[] mipmapPaths = options.MipmapFiles
            .Trim('\"') // remove any quotes
            .Split(';');// split on semicolon
        List<OSPath> validPaths = [];
        for (int i = 0; i < mipmapPaths.Length; i++)
        {
            // Skip any empty/whitespace strings
            if (string.IsNullOrWhiteSpace(mipmapPaths[i]))
                continue;
            // Remove any whitespace at either end
            mipmapPaths[i] = mipmapPaths[i].Trim();
            // Should be done...?
            validPaths.Add(new(mipmapPaths[i]));
        }
        return [.. validPaths];
    }

    private static Image<Rgba32>[] GetMainTextureAndMipmapImages(Options options)
    {
        // Get textures as images
        OSPath inputPath = new(options.InputPath);
        OSPath[] mipmapPaths = GetMipmapPaths(options);
        // Load images
        Image<Rgba32>[] images = new Image<Rgba32>[1 + mipmapPaths.Length];
        images[0] = (Image<Rgba32>)Image.Load(inputPath);
        for (int i = 1; i < images.Length; i++)
        {
            OSPath mipmapPath = mipmapPaths[i - 1];
            images[i] = (Image<Rgba32>)Image.Load(mipmapPath);
        }
        return images;
    }

    private static int GetMipmapByIndex(int index, int arraySize, MipmapGenerationMode mipmapGenerationMode)
    {
        return (mipmapGenerationMode) switch
        {
            MipmapGenerationMode.Last => (index >= arraySize) ? arraySize - 1 : index,
            MipmapGenerationMode.Wrap => index % arraySize,
            MipmapGenerationMode.PingPong => PingPong(index, arraySize),
            _ => throw new NotImplementedException($"{mipmapGenerationMode}"),
        };
    }

    private static int PingPong(int index, int arraySize)
    {
        if (arraySize < 2)
            return 0;

        int max = arraySize * 2 - 2;
        index %= max;
        if (index % max < arraySize)
        {
            return index % max;
        }
        else
        {
            return max - index;
        }
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
    private static TplEntryInfo[] GetTplEntryInfos(OSPath inputPath, OSPath outputPath)
    {
        // Load TPL file
        Tpl tpl = new TplFile(inputPath);

        // Iterate over all texture sequence (each sequence is main texture + optional mipmaps)
        int numTextures = tpl.TextureSequences.Length;
        TplEntryInfo[] texInfos = new TplEntryInfo[numTextures];

        for (int i = 0; i < numTextures; i++)
        {
            // Get texture sequence
            TextureSequence textureSequence = tpl.TextureSequences[i];
            // Skip if bleh
            if (textureSequence is null ||
                textureSequence.Description.IsGarbageEntry ||
                textureSequence.Description.IsNull)
                continue;

            // Output name is the hash of each texture in sequence
            StringBuilder builder = new();
            foreach (var textureEntry in textureSequence.Elements)
                builder.Append($"{textureEntry.Crc32Text}-");
            string textureCrc32sName = builder.ToString()[..^1]; // removes last dash

            // WHEN BUILDING LIBRARY WITH SHARED FOLDER
            // Many images are the same, but lower mipmaps are bit-inaccurate, and so duplicates
            // of the same image are made due to different CRCs. This function weeds those out.
            // Function mutates name if neighbour exists.
            textureCrc32sName = GetSameCrc32FileNameOrMipmapBitNeighbourFileName(outputPath.Directories, textureCrc32sName);

            texInfos[i] = new()
            {
                Crc32Name = textureCrc32sName,
                TextureSequence = textureSequence,
            };
        }

        // To be used to map GMA texture indexes to specific image files.
        return texInfos;
    }

    private static void SaveGxtexAndPng(Options options, TplEntryInfo[] tplEntryInfos, OSPath outputPath)
    {
        var resampler = options.Resampler;

        foreach (var tplEntryInfo in tplEntryInfos)
        {
            // Skip null entries
            if (tplEntryInfo.TextureSequence is null)
                continue;
            // Assign potentially corrected texture, and create output path with it too
            OSPath targetOutputPath = outputPath.Copy();
            targetOutputPath.SetFileName(tplEntryInfo.Crc32Name);
            // Output images
            SaveGxtexAndPng(options, targetOutputPath, resampler, tplEntryInfo.TextureSequence);
        }
    }

    /// <summary>
    ///     Creates a .GXTEX and preview .PNG from a <paramref name="textureSequence"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="outputPath"></param>
    /// <param name="resampler"></param>
    /// <param name="textureSequence"></param>
    private static void SaveGxtexAndPng(Options options, OSPath outputPath, IResampler resampler, TextureSequence textureSequence)
    {
        // Prepare output paths
        OSPath imageOutputPath = outputPath.Copy();
        OSPath gxtexOutputPath = outputPath.Copy();
        imageOutputPath.SetExtensions("png");
        gxtexOutputPath.SetExtensions("gxtex");

        // PNG
        {
            bool doWriteWrite = CheckWillFileWrite(options, imageOutputPath, out FileResult result);
            PrintFileWriteResult(result, imageOutputPath, options.ActionStr);
            if (doWriteWrite)
            {
                WriteTextureSequenceAsPNG(textureSequence, imageOutputPath, resampler);
            }
        }

        // GXTEX
        {
            bool doWriteWrite = CheckWillFileWrite(options, gxtexOutputPath, out FileResult result);
            PrintFileWriteResult(result, gxtexOutputPath, options.ActionStr);
            if (doWriteWrite)
            {
                WriteTextureSequenceAsGxTexture(textureSequence, gxtexOutputPath, resampler);
            }
        }
    }

    /// <summary>
    ///     Finds the texture by name in <paramref name="directory"/>> for this texture that shares
    ///     the same main texture CRC32 and higher mipmaps, but ignores lower mipmap CRC32s as they
    ///     are sometimes bit-different, but functionally identical. If no match found, returns the
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
        // mipmap, which is often 3rd from last, and on CMPR ones where compression settings
        // could be messing with that, depending on PC/CPU or other factors.

        int minLength = (8 * 3) + 2; // 3 CRC32s with 2 dashes
        bool isImageWith3OrMoreMipmaps = crc32FileName.Length > minLength;
        if (isImageWith3OrMoreMipmaps && Directory.Exists(directory))
        {
            // Find files with same starting name
            string partialCrc32FileName = $"{crc32FileName[..^minLength]}*"; // add * for wildcard
            string[] matches = Directory.GetFiles(directory, partialCrc32FileName, SearchOption.TopDirectoryOnly);
            foreach (var match in matches)
            {
                // Check to see if these are compatible. It will often times compare the same image 
                // names as the image we are checking is identical to an existing one. However, 
                // this will also weed out those with just 1 CRC32 mismatch.
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
    ///     Writes single texture sequence (texture with mipmaps) as single PNG.
    /// </summary>
    /// <param name="textureSequence"></param>
    /// <param name="fullOutputPath"></param>
    /// <param name="resampler"></param>
    private static void WriteTextureSequenceAsPNG(TextureSequence textureSequence, string fullOutputPath, IResampler resampler)
    {
        // Prepare image buffer. Twice width to fit mipmaps if they exist.
        int width = textureSequence.Length > 1 ? textureSequence.Description.Width * 2 : textureSequence.Description.Width;
        int height = textureSequence.Description.Height;
        Image<Rgba32> image = new(width, height, new(0, 0, 0, 0)); // transparent, alpha images are drawn on top of this
        // Where to draw within the larger texture, changes with each write (so not to overlap)
        Point offset = new(0, 0);

        // Always process main texture
        var mainTexture = TextureToImage(textureSequence.Elements[0].Texture);
        // Apply main texture to blank image
        image.Mutate(c => c.DrawImage(mainTexture, 1f));
        // Set offset for next iteration
        offset.X = mainTexture.Width;

        // Do a check where 4:1 ratio (wide/tall) textures actually have bad lowest mipmaps
        bool isHighRatio = image.Width / image.Height > 4 || image.Height / image.Width > 4;

        // Get or generate mipmaps
        for (int i = 1; i < textureSequence.Length; i++)
        {
            // Get texture data
            TextureSequenceElement textureData = textureSequence.Elements[i];
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
    ///     Writes single texture sequence (texture with mipmaps) as single <see cref="GxTextureAsset"/>.
    /// </summary>
    /// <param name="textureSequence"></param>
    /// <param name="fullOutputPath"></param>
    /// <param name="resampler"></param>
    private static void WriteTextureSequenceAsGxTexture(TextureSequence textureSequence, string fullOutputPath, IResampler resampler)
    {
        // Break outy some data
        var description = textureSequence.Description;
        var textureEncoding = TextureEncoding.GetEncoding(description.TextureFormat);

        // Get main texture if CMPR, will need to fix texture
        bool isCMPR = description.TextureFormat == TextureFormat.CMPR;
        Image<Rgba32> mainTexture = isCMPR
            ? TextureToImage(textureSequence.Elements[0].Texture)
            : new Image<Rgba32>(1, 1);

        // Load up texture data or create it if needed
        byte actualTextureCount = 0;
        var textureSequenceData = new List<byte>(textureSequence.AddressRange.Size);
        // Iterate over each texture/mipmap in sequence
        for (int i = 0; i < textureSequence.Length; i++)
        {
            TextureSequenceElement textureSequenceElement = textureSequence.Elements[i];
            if (textureSequenceElement.IsValid)
            {
                textureSequenceData.AddRange(textureSequenceElement.RawTextureData);
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
                // Add data to array
                byte[] mipmapData = mipmapTexture.GetRawBytes(description.TextureFormat);
                textureSequenceData.AddRange(mipmapData);
            }

            // If we get this far, we know we have a real texture encoded
            actualTextureCount++;
        }

        // Prepare container
        GxTextureAssetFile gxTextureFile = new()
        {
            Value = new()
            {
                Width = description.Width,
                Height = description.Height,
                Format = description.TextureFormat,
                TextureCount = actualTextureCount,
                DataLength = textureSequenceData.Count,
                Data = [.. textureSequenceData],
            }
        };
        // Write out texture
        EnsureDirectoriesExist(fullOutputPath);
        gxTextureFile.WriteFile(fullOutputPath);
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
        GmaFile gmaFile = new(inputPath);
        Gma gma = gmaFile.Value;

        //　Record names of generated files for .gmaref
        List<string> gcmfAssetNames = [];

        // Iterate over all models in GMA
        int numModels = gma.Models.Length;
        for (int i = 0; i < numModels; i++)
        {
            // Get model data
            string name = gma.Models[i].Name;
            Gcmf gcmf = gma.Models[i].Gcmf;

            // Get this GMA's texture references.
            // If no textures provided, do not get texture names
            string[] tevTextureReferences = gmaTextures.Length > 0 ? new string[gcmf.TevLayers.Length] : [];
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
            modelOutputPath.SetFileName($"{name}-{gcmf.CRC32:x8}");
            modelOutputPath.SetExtensions("gcmfx");
            //
            gcmfAssetNames.Add(modelOutputPath.FileNameAndExtensions);

            // GCMFX
            // Create standalone GCMF with reference to textures!
            {
                bool doWriteWrite = CheckWillFileWrite(options, modelOutputPath, out FileResult result);
                PrintFileWriteResult(result, modelOutputPath, options.ActionStr);
                if (doWriteWrite)
                {
                    GcmfAssetFile gcmfAssetFile = new()
                    {
                        Value = new()
                        {
                            Name = name,
                            TevTextureReferences = tevTextureReferences,
                            Gcmf = gcmf,
                        }
                    };
                    EnsureDirectoriesExist(modelOutputPath);
                    gcmfAssetFile.WriteFile(modelOutputPath);
                }
            }
        }

        // GMAREF
        // Create GMA ref file (plaintext)
        {
            // GMA file
            string fileName = Path.GetFileNameWithoutExtension(gmaFile.FileName);
            string fileDirectories = Path.GetDirectoryName(inputPath)!;
            // Construct path for GAMREF
            OSPath gmarefOutputPath = outputPath.Copy();
            gmarefOutputPath.SetFileName(fileName);
            gmarefOutputPath.SetExtensions(GmaRef.Extension);
            // Preserve subdirectories
            if (OSPath.MatchExclusiveSubdirectories(options.InputPath, fileDirectories, out string subdirectories))
                gmarefOutputPath.PushDirectories(subdirectories);
            // Write file if able
            bool doWriteWrite = CheckWillFileWrite(options, gmarefOutputPath, out FileResult result);
            PrintFileWriteResult(result, gmarefOutputPath, options.ActionStr);
            if (doWriteWrite)
            {
                // Create .GMAREF file
                EnsureDirectoriesExist(gmarefOutputPath);
                using var writer = new PlainTextWriter(File.Create(gmarefOutputPath), GmaRef.Encoding);
                // Write a reference to each GCMF for this GMA file as a .GMAREF
                GmaRef gmaRef = new();
                gmaRef.GcmfModels = [.. gcmfAssetNames];
                gmaRef.Serialize(writer);
            }
        }
    }

}
