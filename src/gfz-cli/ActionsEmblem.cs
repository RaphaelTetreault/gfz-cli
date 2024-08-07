using GameCube.GFZ.Emblem;
using GameCube.GFZ.GCI;
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

namespace Manifold.GFZCLI
{
    public static class ActionsEmblem
    {

        public static void EmblemsBinToImages(Options options)
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
                FilePath textureOutput = new(outputFile);
                textureOutput.SetName($"{outputFile.Name}-banner");
                fileWriteInfo.OutputFilePath = textureOutput;
                fileWriteInfo.PrintActionDescription = "converting emblem banner";
                WriteImage(options, encoder, emblemGCI.Banner, fileWriteInfo);
            }
            // ICON
            for (int i = 0; i < emblemGCI.Icons.Length; i++)
            {
                var icon = emblemGCI.Icons[i];
                // Strip original file name, replace with GC game code
                FilePath textureOutput = new(outputFile);
                textureOutput.SetName($"{emblemGCI.Header}-icon{i}");
                fileWriteInfo.OutputFilePath = textureOutput;
                fileWriteInfo.PrintActionDescription = $"converting emblem icon #{i}";
                WriteImage(options, encoder, icon, fileWriteInfo);
            }
            // EMBLEM
            {
                fileWriteInfo.OutputFilePath = outputFile;
                fileWriteInfo.PrintActionDescription = "converting emblem";
                WriteImage(options, encoder, emblemGCI.Emblem.Texture, fileWriteInfo);
            }
        }


        public static void EmblemsBinFromImages(Options options)
        {
            Terminal.WriteLine("Emblem: converting image(s) to emblem.bin.");
            var emblems = ImageToEmblemBin(options);
            Terminal.WriteLine($"Emblem: done converting {emblems.Length} image{(emblems.Length != 1 ? 's' : "")}.");
        }

        public static void EmblemGciFromImage(Options options)
        {
            // In this case where no search pattern is set, find *fze*.dat.gci (emblem) files.
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*fze*.dat.gci";

            Terminal.WriteLine("Emblem: converting image(s) to emblem.dat.gci.");
            int gciCount = DoFileInFileOutTasks(options, ImageToEmblemGci);
            Terminal.WriteLine($"Emblem: done converting {gciCount} image{Plural(gciCount)}.");
        }

        public static Emblem ImageToEmblemBin(Options options, FilePath inputFile)
        {
            // Make sure some option parameters are appropriate
            bool isTooLarge = IImageSharpOptions.IsSizeTooLarge(options, Emblem.Width, Emblem.Height);
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
            lock (lock_ConsoleWrite)
            {
                Terminal.Write($"Emblem: ");
                Terminal.Write($"processing image ");
                Terminal.Write(inputFile, FileNameColor);
                Terminal.Write($" ({image.Width},{image.Height}).");
                Terminal.WriteLine();
            }

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
                using var fileStream = File.Create(outputFilePath);
                using var writer = new EndianBinaryWriter(fileStream, EmblemBIN.endianness);
                EmblemBIN emblemBin = new();
                emblemBin.Emblems = emblems;
                emblemBin.Serialize(writer);
            };

            FileWriteOverwriteHandler(options, fileWrite, info);

            return emblems;
        }

        public static void ImageToEmblemGci(Options options, FilePath inputFile, FilePath outputFile)
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
            Texture[] icons = new[] { iconTexture };
            Emblem emblem = new(emblemTexture);
            EmblemGCI emblemGci = new(options.SerializationRegion);
            options.ThrowIfInvalidRegion();

            // Get name for output file
            string gciFileName = EmblemGCI.FormatGciFileName(GfzGciFileType.Emblem, emblemGci.Header, outputFile.Name, out string fileName);
            outputFile.SetName(gciFileName);

            // Assign data
            emblemGci.Emblem = emblem;
            emblemGci.SetBanner(banner);
            emblemGci.SetIcons(icons);
            emblemGci.SetFileName(fileName);

            // Write file
            var fileWrite = () =>
            {
                // Save emblem
                using var fileStream = File.Create(outputFile);
                using var writer = new EndianBinaryWriter(fileStream, EmblemGCI.endianness);
                emblemGci.Serialize(writer);
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


        private static ResizeOptions GetEmblemResizeOptions(Options options, int imageWidth, int imageHeight, int resizeWidth, int resizeHeight, bool resizeHasAlphaBorder)
        {
            // Resize image to fit inside bounds of image.
            // eg: emblem is 64x64
            ResizeOptions resizeOptions = IImageSharpOptions.GetResizeOptions(options);

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
            resizeOptions.Size = IImageSharpOptions.GetResizeSize(options, defaultX, defaultY);

            return resizeOptions;
        }
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
}
