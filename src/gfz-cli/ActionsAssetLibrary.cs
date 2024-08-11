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
using System.IO.Compression;
using System.Text;


namespace Manifold.GFZCLI
{
    public static class ActionsAssetLibrary
    {
        private const string Marker = "Asset Library";

        public static void CreateGmaTplLibrary(Options options)
        {
            // Assert that destination is a folder.
            bool isFile = File.Exists(options.OutputPath);
            if (isFile)
            {
                string msg = $"";
                throw new ArgumentException();
            }

            Terminal.WriteLine($"{Marker}: generating asset library.");
            CreateGmaTplLibrary(options, new(options.InputPath), new(options.OutputPath + "/"));
            //int taskCount = DoFileInFileOutTasks(options, CreateGmaTplLibrary);
            Terminal.WriteLine($"{Marker}: done.");
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

            // Tasks
            foreach (var gmaFile in gmaFiles)
            {
                // Check: does GMA have a TPL file beside it in the directory?
                bool hasTpl = tplFiles.Contains(gmaFile);
                //FilePath[] outputTplTextures = Array.Empty<FilePath>();
                if (hasTpl)
                {
                    // Load file
                    Tpl tpl = BinarySerializableIO.LoadFile<Tpl>(gmaFile + "tpl");
                    // Remove file from future processing
                    tplFiles.Remove(gmaFile);
                    // Create textures
                    //outputTplTextures = new FilePath[tpl.TextureSeries.Length];

                    //
                    WritePackedTextureSeries(options, tpl, outputPath, resampler);

                    // TEMP
                    break;
                }

            }

            foreach (var tplFile in tplFiles)
            {
                Tpl tpl = BinarySerializableIO.LoadFile<Tpl>(tplFile + "tpl");
                WritePackedTextureSeries(options, tpl, outputPath, resampler);
            }
        }

        // TODO: merge these 2 functions together... and figure the rest out.

        public static void WritePackedTextureSeries(Options options, Tpl tpl, FilePath outputPath, IResampler resampler)
        {
            // Iterate over all texture series
            for (int i = 0; i < tpl.TextureSeries.Length; i++)
            {
                // Get texture series
                TextureSeries textureSeries = tpl.TextureSeries[i];
                // Skip if bleh
                if (textureSeries is null ||
                    textureSeries.Description.IsGarbageEntry ||
                    textureSeries.Description.IsNull)
                    continue;
                // Write texture as image (for the humans)
                WritePackedTextureSeries(options, outputPath, textureSeries, resampler);
            }
        }

        public static void WritePackedTextureSeries(Options options, FilePath outputPath, TextureSeries textureSeries, IResampler resampler)
        {
            // Output name is the hash of the main texture (for now)
            StringBuilder builder = new StringBuilder();
            foreach(var textureEntry in textureSeries.Entries)
                builder.Append($"{textureEntry.CRC32}-");
            string name = builder.ToString()[..^1];

            FilePath fullOutputPath = outputPath.Copy();
            fullOutputPath.SetName(name);
            fullOutputPath.SetExtensions("png");

            void WritePackedTexture()
            {
                // Prepare image buffer
                int width = textureSeries.Length > 1 ? textureSeries.Description.Width * 2 : textureSeries.Description.Width;
                int height = textureSeries.Description.Height;
                Image<Rgba32> image = new(width, height, new(0, 0, 0));
                // Where to draw within the larger texture
                Point offset = new(0, 0);

                // Always process main texture
                var mainTexture = TextureToImage(textureSeries.Entries[0].Texture);
                image.Mutate(c => c.DrawImage(mainTexture, 1f));
                offset.X = mainTexture.Width;

                // Get or generate mipmaps
                for (int i = 1; i < textureSeries.Length; i++)
                {
                    // Get texture data
                    TextureData textureData = textureSeries[i];
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
                        // If truly corrupted, then break loop
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

                //
                EnsureDirectoriesExist(fullOutputPath);
                // Write out texture
                PngEncoder imageEncoder = new()
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                };
                // Write that bad boy
                image.Save(fullOutputPath, imageEncoder);
            }

            // Finally, write out texture
            var info = new FileWriteInfo()
            {
                InputFilePath = "fuck",
                OutputFilePath = fullOutputPath,
                PrintDesignator = "guh",
                PrintActionDescription = $"xxx",
            };
            FileWriteOverwriteHandler(options, WritePackedTexture, info);
        }

    }
}