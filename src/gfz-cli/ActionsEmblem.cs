﻿using GameCube.GFZ.Emblem;
using GameCube.GX.Texture;
using Manifold.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;
using static Manifold.GFZCLI.GfzCliImageUtilities;
using static Manifold.GFZCLI.Program;
using GameCube.GCI;

namespace Manifold.GFZCLI
{
    public static class ActionsEmblem
    {

        public static void EmblemBinToImage(Options options)
        {
            Terminal.WriteLine("Emblem: converting emblems from BIN files.");
            int binCount = DoFileInFileOutTasks(options, EmblemBinToImages);
            Terminal.WriteLine($"Emblem: done converting {binCount} file{Plural(binCount)}.");
        }

        public static void EmblemGciToImage(Options options)
        {
            // In this case where no search pattern is set, find *FZE*.GCI (emblem) files.
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*fze*.dat.gci";

            Terminal.WriteLine("Emblem: converting emblems from GCI files.");
            int gciCount = DoFileInFileOutTasks(options, EmblemGciToImage);
            Terminal.WriteLine($"Emblem: done converting {gciCount} file{Plural(gciCount)}.");
        }

        private static void EmblemBinToImages(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Read BIN Emblem data
            var emblemBIN = new EmblemBIN();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), EmblemBIN.endianness))
            {
                emblemBIN.Deserialize(reader);
                emblemBIN.FileName = Path.GetFileNameWithoutExtension(inputFile);
            }

            // Prepare image encoder
            var encoder = new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression
            };
            outputFile.AppendDirectory(emblemBIN.FileName);
            outputFile.SetExtensions(".png");

            // Info for file write + console print
            var fileWriteInfo = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                PrintDesignator = "Emblem",
            };

            // Write out each emblem in file
            int formatLength = emblemBIN.Emblems.LengthToFormat();
            for (int i = 0; i < emblemBIN.Emblems.Length; i++)
            {
                var emblem = emblemBIN.Emblems[i];
                int index = i + 1;
                string indexStr = index.PadLeft(formatLength, '0');
                outputFile.SetName($"{inputFile.Name}-{indexStr}");
                fileWriteInfo.PrintActionDescription = $"converting emblem {indexStr} of";
                fileWriteInfo.OutputFilePath = outputFile;
                EnsureDirectoriesExist(outputFile);
                WriteImage(options, encoder, emblem.Texture, fileWriteInfo);
            }
        }

        private static void EmblemGciToImage(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Read GCI Emblem data
            var emblemGCI = new EmblemGCI();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), EmblemGCI.endianness))
            {
                emblemGCI.Deserialize(reader);
                emblemGCI.FileName = Path.GetFileNameWithoutExtension(inputFile);
            }

            // Prepare image encoder
            var encoder = new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression
            };
            // Strip .dat.gci extensions
            outputFile.SetExtensions("png");

            // Info for file write + console print
            var fileWriteInfo = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                PrintDesignator = "Emblem",
            };

            // BANNER
            {
                FilePath textureOutput = new FilePath(outputFile);
                textureOutput.SetName($"{outputFile.Name}-banner");
                fileWriteInfo.OutputFilePath = textureOutput;
                fileWriteInfo.PrintActionDescription = "converting emblem banner";
                WriteImage(options, encoder, emblemGCI.Emblem.Texture, fileWriteInfo);
            }
            // ICON
            {
                // Strip original file name, replace with GC game code
                FilePath textureOutput = new FilePath(outputFile);
                textureOutput.SetName($"{emblemGCI.Header}-icon");
                fileWriteInfo.OutputFilePath = textureOutput;
                fileWriteInfo.PrintActionDescription = "converting emblem icon";
                WriteImage(options, encoder, emblemGCI.Emblem.Texture, fileWriteInfo);
            }
            // EMBLEM
            {
                fileWriteInfo.OutputFilePath = outputFile;
                fileWriteInfo.PrintActionDescription = "converting emblem";
                WriteImage(options, encoder, emblemGCI.Emblem.Texture, fileWriteInfo);
            }
        }

        public static void ImageToEmblemBIN(Options options)
        {
            Terminal.WriteLine("Emblem: converting image(s) to emblem.bin.");
            var emblems = ImageToEmblemBin(options);
            Terminal.WriteLine($"Emblem: done converting {emblems.Length} image{(emblems.Length != 1 ? 's' : "")}.");
        }

        public static void ImageToEmblemGCI(Options options)
        {
            // In this case where no search pattern is set, find *fz*.dat.gci (emblem) files.
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*fz*.dat.gci";

            Terminal.WriteLine("Emblem: converting image(s) to emblem.dat.gci.");
            int gciCount = DoFileInFileOutTasks(options, ImageToEmblemGci);
            Terminal.WriteLine($"Emblem: done converting {gciCount} image{(gciCount != 1 ? 's' : "")}.");
        }

        public static Emblem ImageToEmblemBin(Options options, FilePath inputFile)
        {
            // Make sure some option parameters are appropriate
            bool isTooLarge = IImageResizeOptions.IsSizeTooLarge(options, Emblem.Width, Emblem.Height);
            if (isTooLarge)
            {
                string msg =
                    $"Requested resize ({options.Width},{options.Height}) exceeds the maximum " +
                    $"bounds of an emblem ({Emblem.Width},{Emblem.Height}).";
                throw new ArgumentException(msg);
            }

            Image<Rgba32> image = Image.Load<Rgba32>(inputFile);

            // TODO: auto-magically handle 64x64 images

            // Resize image to fit inside bounds of 64x64
            ResizeOptions ResizeOptions = IImageResizeOptions.GetResizeOptions(options);
            // Emblem size is either 62x62 (1px alpha border, as intended) or 64x64 ("hacker" option)
            int emblemX = options.EmblemHasAlphaBorder ? Emblem.Width - 2 : Emblem.Width;
            int emblemY = options.EmblemHasAlphaBorder ? Emblem.Height - 2 : Emblem.Height;
            // Choose lowest dimensions as the default size (ie: preserve pixel-perfect if possible)
            int defaultX = Math.Min(emblemX, image.Width);
            int defaultY = Math.Min(emblemY, image.Height);
            // Set size override, then resize image
            ResizeOptions.Size = IImageResizeOptions.GetResizeSize(options, defaultX, defaultY);
            image.Mutate(ipc => ipc.Resize(ResizeOptions));

            // Create emblem, textures
            Emblem emblem = new Emblem();
            Texture imageTexture = ImageToTexture(image, TextureFormat.RGB5A3);
            Texture emblemTexture = new Texture(Emblem.Width, Emblem.Height, TextureColor.Clear, TextureFormat.RGB5A3);

            // Copy image texture to emblem center
            // Only works if image is 64px or less! Resize code block prepares this.
            int offsetX = (Emblem.Width - image.Width) / 2;
            int offsetY = (Emblem.Height - image.Height) / 2;
            Texture.Copy(imageTexture, emblemTexture, offsetX, offsetY);

            // Write some useful information to the terminal
            lock (lock_ConsoleWrite)
            {
                Terminal.Write($"Emblem: ");
                Terminal.Write($"processing image ");
                Terminal.Write(inputFile, FileNameColor);
                Terminal.Write($" ({image.Width},{image.Height}).");
                Terminal.WriteLine();
            }

            // Assign return
            emblem.Texture = emblemTexture;
            return emblem;
        }

        public static Emblem[] ImageToEmblemBin(Options options)
        {
            var emblems = DoFileInTypeOutTasks(options, ImageToEmblemBin);

            string outputFilePath = EnforceUnixSeparators(options.OutputPath);

            // Info for file write + console print
            var info = new FileWriteInfo()
            {
                InputFilePath = options.InputPath,
                OutputFilePath = outputFilePath,
                PrintDesignator = "Emblem",
                PrintActionDescription = "packaging path",
            };

            var fileWrite = () =>
            {
                using (var fileStream = File.Create(outputFilePath))
                {
                    using (var writer = new EndianBinaryWriter(fileStream, EmblemBIN.endianness))
                    {
                        var emblemBin = new EmblemBIN();
                        emblemBin.Emblems = emblems;
                        emblemBin.Serialize(writer);
                    }
                }
            };

            FileWriteOverwriteHandler(options, fileWrite, info);

            return emblems;
        }

        // TODO: should this be in Emblem class?
        private static string GetFileName(string fileName)
        {
            const int maxChars = 20;
            string fileName20Char = fileName.PadLeft(maxChars, '-');
            fileName20Char = fileName20Char.Substring(0, maxChars);

            // TODO: get strings from gamecode
            string name = $"XX-GFZX-fzX020-{fileName20Char}";
            return name;
        }

        public static void ImageToEmblemGci(Options options, FilePath inputFile, FilePath outputFile)
        {
            string fileName = GetFileName(outputFile.Name);
            outputFile.SetName(fileName);
            outputFile.SetExtensions(".dat.gci");

            // Load image
            Image<Rgba32> image = Image.Load<Rgba32>(inputFile);
            Image<Rgba32> imagePreview = image.Clone();
            //
            image.Mutate(ipc => ipc.Resize(Emblem.Width, Emblem.Height));
            imagePreview.Mutate(ipc => ipc.Resize(Gci<Emblem>.IconWidth, Gci<Emblem>.IconHeight));

            Texture emblemTexture = ImageToTexture(image);
            Texture emblemPreview = ImageToTexture(imagePreview);
            Texture banner = new Texture(Gci<Emblem>.BannerWidth, Gci<Emblem>.BannerHeight, Gci<Emblem>.DirectFormat);
            Texture[] icons = new Texture[] { emblemPreview };
            Emblem emblem = new Emblem(emblemTexture);
            EmblemGCI gci = new EmblemGCI();
            gci.SetFileData();
            gci.SetBanner();
            gci.SetIcons();
            gci.SetFileName();

            //  
            var fileWrite = () =>
            {
                // Save emblem
                using (var fileStream = File.Create(outputFile))
                {
                    using (var writer = new EndianBinaryWriter(fileStream, EmblemGCI.endianness))
                    {
                        gci.Serialize(writer);
                    }
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "Emblem",
                PrintActionDescription = "creating emblem",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
    }
}
