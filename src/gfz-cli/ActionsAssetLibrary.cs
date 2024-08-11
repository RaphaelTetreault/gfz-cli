using GameCube.GFZ.GMA;
using GameCube.GFZ.TPL;
using GameCube.GX.Texture;
using Manifold.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.IO;


using static Manifold.GFZCLI.GfzCliUtilities;
using static Manifold.GFZCLI.GfzCliImageUtilities;
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

        public static void CreateGmaTplLibrary(Options options, FilePath inputPath, FilePath outputPath)
        {
            // Copy original argument
            string searchPattern = options.SearchPattern;
            // Get GMA file paths
            options.SearchPattern = "*.gma";
            //FilePath[] gmaFiles = FilePath.ToFilePaths(GetInputFiles(options));
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

            // Create TPL alongside GMAs
            foreach (var gmaFile in gmaFiles)
            {
                // Check: does GMA have a TPL file beside it in the directory?
                bool hasTpl = tplFiles.Contains(gmaFile);
                if (hasTpl)
                {
                    // Remove file from future processing
                    tplFiles.Remove(gmaFile);
                    //
                    // Get path to TPL
                    FilePath tplFilePath = new(gmaFile);
                    tplFilePath.SetExtensions("tpl");
                    // Load TPL file
                    Tpl tpl = BinarySerializableIO.LoadFile<Tpl>(tplFilePath);
                    tpl.FileName = tplFilePath;
                    // Write out textures
                    WriteTplTexturesAsPNG(options, tpl, outputPath, resampler);
                    WriteTplTexturesAsGxBlock(options, tpl, outputPath, resampler);
                    break;
                }
            }

            // Extract remaining TPLs without associated GMA
            foreach (var tplFile in tplFiles)
            {
                // Get path to TPL
                FilePath tplFilePath = new(tplFile);
                tplFilePath.SetExtensions("tpl");
                // Load TPL file
                Tpl tpl = BinarySerializableIO.LoadFile<Tpl>(tplFilePath);
                tpl.FileName = tplFilePath;
                // Write out textures
                WriteTplTexturesAsPNG(options, tpl, outputPath, resampler);
                WriteTplTexturesAsGxBlock(options, tpl, outputPath, resampler);
                break;
            }
        }

        // Runs an action on all texture bundles
        private static void TplTextureBundleTask(Options options, Tpl tpl, FilePath outputPath, string ext, Action<TextureBundle, string> task)
        {
            // Iterate over all texture bundle (each bundle is main texture + optional mipmaps)
            int numTextures = tpl.TextureBundles.Length;
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

                // Create final output path
                FilePath fullOutputPath = outputPath.Copy();
                fullOutputPath.SetName(name);
                fullOutputPath.SetExtensions(ext);

                // Information about this file write
                var info = new FileWriteInfo()
                {
                    InputFilePath = tpl.FileName, // I put path in here
                    OutputFilePath = fullOutputPath,
                    PrintDesignator = Designator,
                    PrintActionDescription = $"extracting texture ({i + 1}/{numTextures}) from",
                };
                void TextureBundleTask() { task(textureBundle, fullOutputPath); }
                FileWriteOverwriteHandler(options, TextureBundleTask, info);
            }
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

        // Writes all textures bundle in TPL to own PNG
        private static void WriteTplTexturesAsPNG(Options options, Tpl tpl, FilePath outputPath, IResampler resampler)
        {
            void Task(TextureBundle textureBundle, string fullOutputPath)
                => WriteTextureBundleAsPNG(textureBundle, fullOutputPath, resampler);

            TplTextureBundleTask(options, tpl, outputPath, "png", Task);
        }

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

        public static void WriteTplTexturesAsGxBlock(Options options, Tpl tpl, FilePath outputPath, IResampler resampler)
        {
            void Task(TextureBundle textureBundle, string fullOutputPath)
                => WriteTextureBundleAsGxTexture(textureBundle, fullOutputPath, resampler);

            TplTextureBundleTask(options, tpl, outputPath, "gxtex", Task);
        }

    }
}