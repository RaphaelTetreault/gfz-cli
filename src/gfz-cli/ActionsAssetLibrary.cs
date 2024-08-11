﻿using GameCube.GFZ.GMA;
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
                    FilePath sourceFilePath = new FilePath(gmaFile);
                    sourceFilePath.SetExtensions("tpl");
                    WritePackedTextureSeries(options, tpl, sourceFilePath, outputPath, resampler);

                    // TEMP
                    break;
                }

            }

            foreach (var tplFile in tplFiles)
            {
                FilePath sourceFilePath = new FilePath(tplFile);
                sourceFilePath.SetExtensions("tpl");
                Tpl tpl = BinarySerializableIO.LoadFile<Tpl>(sourceFilePath);
                WritePackedTextureSeries(options, tpl, sourceFilePath, outputPath, resampler);

                break;
            }
        }

        // TODO: merge these 2 functions together... and figure the rest out.

        public static void WritePackedTextureSeries(Options options, Tpl tpl, FilePath inputPath, FilePath outputPath, IResampler resampler)
        {
            // Iterate over all texture in texture series
            int numTextures = tpl.TextureSeries.Length;
            for (int i = 0; i < numTextures; i++)
            {
                // Get texture series
                TextureSeries textureSeries = tpl.TextureSeries[i];
                // Skip if bleh
                if (textureSeries is null ||
                    textureSeries.Description.IsGarbageEntry ||
                    textureSeries.Description.IsNull)
                    continue;

                // Output name is the hash of each texture in series
                StringBuilder builder = new();
                foreach (var textureEntry in textureSeries.Entries)
                    builder.Append($"{textureEntry.CRC32}-");
                string name = builder.ToString()[..^1]; // removes last dash

                // Previous function
                FilePath fullOutputPath = outputPath.Copy();
                fullOutputPath.SetName(name);
                fullOutputPath.SetExtensions("png");

                // Function which runs if file is output
                void TextureWrite()
                {
                    // Prepare image buffer. Twice width to fit mipmaps if they exist.
                    int width = textureSeries.Length > 1 ? textureSeries.Description.Width * 2 : textureSeries.Description.Width;
                    int height = textureSeries.Description.Height;
                    Image<Rgba32> image = new(width, height, new(0, 0, 0));
                    // Where to draw within the larger texture, changes with each write (so not to overlap)
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

                // Information about this file write
                var info = new FileWriteInfo()
                {
                    InputFilePath = inputPath, // source tpl
                    OutputFilePath = fullOutputPath, // image output
                    PrintDesignator = Designator,
                    PrintActionDescription = $"extracting texture ({i+1}/{numTextures}) from",
                };
                FileWriteOverwriteHandler(options, TextureWrite, info);
            }
        }

    }
}