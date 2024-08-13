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


namespace Manifold.GFZCLI
{
    public static class ActionsAssetLibrary
    {
        private const string Designator = "Asset Library";

        public static void CreateGmaTplLibrary(Options options)
        {
            // Assert that destination is a folder.
            bool isFile = File.Exists(options.OutputPath);
            if (isFile)
            {
                string msg = $"";
                throw new ArgumentException();
            }

            Terminal.WriteLine($"{Designator}: generating asset library.");
            CreateGmaTplLibrary(options, new(options.InputPath), new(options.OutputPath + "/"));
            //int taskCount = DoFileInFileOutTasks(options, CreateGmaTplLibrary);
            Terminal.WriteLine($"{Designator}: done.");
            //Terminal.WriteLine($"Asset Library: done unpacking {taskCount} TPL file{(taskCount != 1 ? 's' : "")}.");
        }

        public static void CreateGmaTplLibrary(Options options, OSPath inputPath, OSPath outputPath)
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

            // Image resampler
            IResampler resampler = options.Resampler;

            OSPath tplOutputPath = outputPath.Copy();
            OSPath gmaOutputPath = outputPath.Copy();
            tplOutputPath.PushDirectory("tex");
            gmaOutputPath.PushDirectory("mdl");

            // Create TPL alongside GMAs
            foreach (var assetFile in gmaFiles)
            {
                // Get path to TPL
                OSPath tplPath = new(assetFile);
                tplPath.SetExtensions("tpl");
                //
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

        // Runs an action on all texture bundles
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
                    var info = new FileWriteInfo()
                    {
                        InputFilePath = tpl.FileName,
                        OutputFilePath = imageOutputPath,
                        PrintDesignator = Designator,
                        PrintActionDescription = $"extracting texture ({i + 1}/{numTextures}) from",
                    };
                    void Task() => WriteTextureBundleAsPNG(textureBundle, imageOutputPath, resampler);
                    FileWriteOverwriteHandler(options, Task, info);
                }

                // GXTEX
                {
                    var info = new FileWriteInfo()
                    {
                        InputFilePath = tpl.FileName,
                        OutputFilePath = gxtexOutputPath,
                        PrintDesignator = Designator,
                        PrintActionDescription = $"extracting texture ({i + 1}/{numTextures}) from",
                    };
                    void Task() => WriteTextureBundleAsGxTexture(textureBundle, gxtexOutputPath, resampler);
                    FileWriteOverwriteHandler(options, Task, info);
                }
            }

            return textureNames;
        }

        // Writes single texture bundle as single PNG
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

        // 
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
            var data = new List<byte>(textureBundle.AddressRange.Size);
            for (int i = 0; i < textureBundle.Length; i++)
            {
                TextureBundleElement textureData = textureBundle.Elements[i];

                if (textureData.IsValid)
                {
                    data.AddRange(textureData.RawTextureData);
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
                    var generatedMipmap = mainTexture.Clone(c => c.Resize(resizeWidth, resizeHeight, resampler));
                    var mipmapTexture = ImageToTexture(generatedMipmap);
                    // Write texture data to memory
                    using var memory = new MemoryStream();
                    using var memoryWriter = new EndianBinaryWriter(memory, Tpl.endianness);
                    Texture.WriteDirectColorTexture(memoryWriter, mipmapTexture, description.TextureFormat);
                    memoryWriter.Flush();
                    // Add data to array
                    byte[] mipmapData = memory.ToArray();
                    data.AddRange(mipmapData);
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
                DataLength = data.Count,
                Data = data.ToArray(),
            };
            // Write out texture
            EnsureDirectoriesExist(fullOutputPath);
            using var writer = new EndianBinaryWriter(File.Create(fullOutputPath), GxTexture.endianness);
            writer.Write(gxTex);
        }

        //
        private static void WriteModels(Options options, OSPath inputPath, OSPath outputPath, string[] gmaTextures)
        {
            // Load GMA file
            Gma gma = BinarySerializableIO.LoadFile<Gma>(inputPath);
            gma.FileName = inputPath;

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
                modelOutputPath.SetExtensions("gcmf");

                // Create standalone GCMF with reference to textures!
                {
                    var info = new FileWriteInfo()
                    {
                        InputFilePath = gma.FileName,
                        OutputFilePath = modelOutputPath,
                        PrintDesignator = Designator,
                        PrintActionDescription = $"extracting GCMF model ({i + 1}/{numModels}) from",
                    };
                    void FileWriteGcmfAsset()
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
                    FileWriteOverwriteHandler(options, FileWriteGcmfAsset, info);
                }
            }
        }
    }
}