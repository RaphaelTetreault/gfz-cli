using CommandLine;
using GameCube.GFZ.CarData;
using GameCube.GFZ.Emblem;
using GameCube.GFZ.LZ;
using GameCube.GFZ.TPL;
using Manifold;
using Manifold.IO;
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using GameCube.GX.Texture;
using SixLabors.ImageSharp.Formats.Png;

using static Manifold.GFZCLI.MultiFileUtility;
using GameCube.GFZ.Camera;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Manifold.GFZCLI
{
    internal class Program
    {
        static readonly object lock_ConsoleWrite = new();
        const ConsoleColor FileNameColor = ConsoleColor.Cyan;
        const ConsoleColor OverwriteFileColor = ConsoleColor.DarkYellow;
        const ConsoleColor WriteFileColor = ConsoleColor.Green;
        static readonly string[] Help = new string[] { "--help" };

        public static void Main(string[] args)
        {
            // Initialize text capabilities
            var encodingProvider = System.Text.CodePagesEncodingProvider.Instance;
            System.Text.Encoding.RegisterProvider(encodingProvider);

            // If user did not pass any arguments, tell them how to use application.
            // This will happen when users double-click application.
            bool noArgumentsPassed = args.Length == 0;
            if (noArgumentsPassed)
            {
                string msg = "You must call this program using arguments via the Console/Terminal. ";
                Terminal.WriteLine(msg, ConsoleColor.Black, ConsoleColor.Red);
                Terminal.WriteLine();
                args = Help;
            }

            // Run program with options
            var parseResult = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(ExecuteAction);

            // If user did not pass any arguments, pause application so they can read Console.
            // This will happen when users double-click application.
            if (noArgumentsPassed)
            {
                string msg = "Press ENTER to continue.";
                Terminal.Write(msg, ConsoleColor.Black, ConsoleColor.Red);
                Terminal.WriteLine();
                Console.Read();
            }
        }

        public static void ExecuteAction(Options options)
        {
            switch (options.Action)
            {
                // CARDATA
                case GfzCliAction.cardata_bin_to_tsv: CarDataBinToTsv(options); break;
                case GfzCliAction.cardata_tsv_to_bin: CarDataTsvToBin(options); break;
                // EMBLEM
                case GfzCliAction.emblem_to_image: EmblemToImage(options); break;
                case GfzCliAction.image_to_emblem: ImageToEmblem(options); break;
                // LIVE CAMERA STAGE
                case GfzCliAction.live_camera_stage_bin_to_tsv: LiveCameraStageBinToTsv(options); break;
                case GfzCliAction.live_camera_stage_tsv_to_bin: LiveCameraStageTsvToBin(options); break;
                // LZ
                case GfzCliAction.lz_compress: LzCompress(options); break;
                case GfzCliAction.lz_decompress: LzDecompress(options); break;
                // TPL
                case GfzCliAction.tpl_unpack: TplUnpack(options); break;
                //case GfzCliAction.tpl_pack: TplPack(options); break;

                // UNSET
                case 0:
                    {
                        string msg = $"Could not parse action) '{options.ActionStr}'.";
                        Terminal.WriteLine(msg);
                        Terminal.WriteLine();
                        //PrintAllGfzCliActions();
                        Parser.Default.ParseArguments<Options>(Help).WithParsed(ExecuteAction);
                    }
                    break;

                default:
                    {
                        string msg = $"Unimplemented command {options.Action}.";
                        throw new NotImplementedException(msg);
                    }
            }
        }

        public static void CarDataBinToTsv(Options options)
        {
            // Manage input
            var inputFile = new FileDescription(options.InputPath);
            inputFile.ThrowIfDoesNotExist();
            // Manage output
            var outputFile = new FileDescription(GetOutputPath(options, options.InputPath));
            outputFile.Extension = ".tsv";

            // Read file
            // Decompress LZ if not decompressed yet
            bool isLzCompressed = inputFile.IsExtension(".lz");
            // Open the file if decompressed, decompress file stream otherwise
            var carData = new CarData();
            using (Stream fileStream = isLzCompressed ? LzUtility.DecompressAvLz(inputFile) : File.OpenRead(inputFile))
            using (var reader = new EndianBinaryReader(fileStream, CarData.endianness))
                carData.Deserialize(reader);

            //
            var fileWrite = () =>
            {
                using (var writer = new StreamWriter(File.Create(outputFile)))
                    carData.Serialize(writer);
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "CarData",
                PrintActionDescription = "creating cardata TSV from file",
                PrintMoreInfoOnSkip =
                    $"Use --{IGfzCliOptions.Args.OverwriteFiles} if you would like to overwrite files automatically.",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
        public static void CarDataTsvToBin(Options options)
        {
            // Manage input
            var inputFile = new FileDescription(options.InputPath);
            inputFile.ThrowIfDoesNotExist();
            // Manage output
            var outputFile = new FileDescription(GetOutputPath(options, options.InputPath));
            outputFile.Extension = ".lz";

            // Get CarData
            var carData = new CarData();
            using (var reader = new StreamReader(File.OpenRead(inputFile)))
                carData.Deserialize(reader);

            // 
            var fileWrite = () =>
            {
                // Save out file (this file is not yet compressed)
                using (var writer = new EndianBinaryWriter(new MemoryStream(), CarData.endianness))
                {
                    // Write data to stream in memory
                    carData.Serialize(writer);
                    // Create file new file
                    using (var fs = File.Create(outputFile))
                    {
                        // Force format to GX since "cardata.lz" is a GX exclusive standalone file.
                        options.SerializationFormatStr = "gx";
                        // Compress memory stream to file stream
                        GameCube.AmusementVision.LZ.Lz.Pack(writer.BaseStream, fs, options.AvGame);
                    }
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "CarData",
                PrintActionDescription = "creating cardata BIN from file",
                PrintMoreInfoOnSkip =
                    $"Use --{IGfzCliOptions.Args.OverwriteFiles} if you would like to overwrite files automatically.",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        public static void LzDecompress(Options options)
        {
            // Force checking for LZ files IF there is no defined search pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*.lz";

            Terminal.WriteLine("LZ: decompressing file(s).");
            int taskCount = DoFileIOTasks(options, LzDecompressFile);
            Terminal.WriteLine($"LZ: done decompressing {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }
        public static void LzDecompressFile(Options options, string inputFilePath, string outputFilePath)
        {
            // Remove extension
            outputFilePath = StripFileExtensions(outputFilePath);

            // 
            var fileWrite = () =>
            {
                // TODO: add LZ function in library to read from inputFilePath, decompress, save to outputFilePath
                using (var stream = LzUtility.DecompressAvLz(inputFilePath))
                {
                    using (var writer = File.Create(outputFilePath))
                    {
                        writer.Write(stream.ToArray());
                    }
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFilePath,
                OutputFilePath = outputFilePath,
                PrintDesignator = "LZ",
                PrintActionDescription = "decompressing file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
        public static void LzCompress(Options options)
        {
            Terminal.WriteLine("LZ: Compressing file(s).");
            int taskCount = DoFileIOTasks(options, LzCompressFile);
            Terminal.WriteLine($"LZ: done compressing {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }
        public static void LzCompressFile(Options options, string inputFilePath, string outputFilePath)
        {
            outputFilePath += ".lz";

            // 
            var fileWrite = () =>
            {
                // TODO: add LZ function in library to read from inputFilePath, compress, save to outputFilePath
                using (var stream = LzUtility.CompressAvLz(inputFilePath, options.AvGame))
                {
                    using (var writer = File.Create(outputFilePath))
                    {
                        writer.Write(stream.ToArray());
                    }
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFilePath,
                OutputFilePath = outputFilePath,
                PrintDesignator = "LZ",
                PrintActionDescription = "compressing input file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        public static void TplUnpack(Options options)
        {
            // Force search TPL files IF there is no defined search pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*.tpl";

            Terminal.WriteLine("TPL: unpacking file(s).");
            int taskCount = DoFileIOTasks(options, TplUnpackFile);
            Terminal.WriteLine($"TPL: done unpacking {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }
        public static void TplUnpackFile(Options options, string inputFilePath, string outputFilePath)
        {
            // Deserialize the TPL
            Tpl tpl = new Tpl();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFilePath), Tpl.endianness))
            {
                tpl.Deserialize(reader);
                tpl.FileName = Path.GetFileNameWithoutExtension(inputFilePath);
            }

            // Create folder named the same thing as the TPL input file
            string directory = Path.GetDirectoryName(outputFilePath);
            string outputDirectory = Path.Combine(directory, tpl.FileName);
            Directory.CreateDirectory(outputDirectory);

            // Prepare image encoder
            var encoder = new PngEncoder();
            encoder.CompressionLevel = PngCompressionLevel.BestCompression;

            // Iterate over texture and mipmaps, save to disk
            int tplIndex = 0;
            foreach (var textureSeries in tpl.TextureSeries)
            {
                tplIndex++;

                if (textureSeries is null)
                    continue;

                int mipmapIndex = 0;
                int entryIndex = -1;
                foreach (var textureEntry in textureSeries.Entries)
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
                    string textureHash = textureSeries.MD5TextureHashes[entryIndex];
                    string fileName = $"{tplIndex}-{mipmapIndex}-{texture.Format}-{textureHash}.png";
                    string textureOutputPath = Path.Combine(outputDirectory, fileName);
                    textureOutputPath = CleanPath(textureOutputPath);

                    //
                    var fileWrite = () =>
                    {
                        // Copy contents of GameCube texture into ImageSharp representation
                        var texture = textureEntry.Texture;
                        Image<Rgba32> image = TextureToImage(texture);
                        // Save to disk
                        image.Save(textureOutputPath, encoder);
                    };
                    var info = new FileWriteInfo()
                    {
                        InputFilePath = inputFilePath,
                        OutputFilePath = textureOutputPath,
                        PrintDesignator = "TPL",
                        PrintActionDescription = $"unpacking texture {tplIndex} mipmap {mipmapIndex} of file",
                    };
                    FileWriteOverwriteHandler(options, fileWrite, info);
                }
            }
        }
        public static void TplPack(Options options)
        {
            return;

            string path = options.InputPath;
            if (string.IsNullOrEmpty(path))
                return;

            // Ensure input file exists
            bool fileExists = File.Exists(path);
            if (!fileExists)
                throw new ArgumentException($"Target file '{path}' does not exist.");

            // TEMP: just do 1 file
            Image<Rgba32> image = Image.Load<Rgba32>(path);
            Texture texture = new Texture(image.Width, image.Height, TextureFormat.CMPR);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Rgba32 pixel = image[x, y];
                    texture[x, y] = new TextureColor(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            using (var writer = new EndianBinaryWriter(new MemoryStream(), Tpl.endianness))
            {
                var encoding = Encoding.EncodingCMPR;
                //var encoding = Encoding.EncodingRGBA8;
                //var encoding = Encoding.EncodingRGB565;
                //var encoding = Encoding.EncodingRGB5A3;
                //var encoding = Encoding.EncodingIA8;
                //var encoding = Encoding.EncodingIA4;
                var blocks = Texture.CreateTextureDirectColorBlocks(texture, encoding, out int bch, out int bcv);
                encoding.WriteTexture(writer, blocks);

                writer.BaseStream.Position = 0;
                using (var reader = new EndianBinaryReader(writer.BaseStream, Tpl.endianness))
                {
                    var blocksCopy = encoding.ReadBlocks<DirectBlock>(reader, encoding, blocks.Length);
                    Texture textureCopy = Texture.FromDirectBlocks(blocksCopy, bch, bcv);

                    // HACK - copy/paste garbage test
                    // Copy contents of GameCube texture into ImageSharp representation
                    Image<Rgba32> imageCopy = new Image<Rgba32>(textureCopy.Width, textureCopy.Height);
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
                    //var imageHash = GetMD5HashName(tempStream);

                    // Find where to save file
                    string directory = Path.GetDirectoryName(path);
                    string fileName = $"temp.png";
                    string filePath = Path.Combine(directory, fileName);
                    // Save to disk
                    imageCopy.SaveAsPng(filePath);
                    Terminal.WriteLine($"Wrote file: {filePath}");
                }
            }
        }

        public static void LiveCameraStageBinToTsv(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*livecam_stage_*.bin";

            Terminal.WriteLine("Live Camera Stage: converting file(s) to TSV.");
            int taskCount = DoFileIOTasks(options, LiveCameraStageBinToTsvFile);
            Terminal.WriteLine($"Live Camera Stage: done converting {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }
        public static void LiveCameraStageBinToTsvFile(Options options, string inputFilePath, string outputFilePath)
        {
            // Deserialize the file
            LiveCameraStage liveCameraStage = new LiveCameraStage();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFilePath), LiveCameraStage.endianness))
            {
                liveCameraStage.Deserialize(reader);
                liveCameraStage.FileName = Path.GetFileNameWithoutExtension(inputFilePath);
            }

            //
            var fileWrite = () =>
            {
                // Write it to the stream
                using (var textWriter = new StreamWriter(File.Create(outputFilePath)))
                {
                    liveCameraStage.Serialize(textWriter);
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFilePath,
                OutputFilePath = outputFilePath,
                PrintDesignator = "LiveCam Stage",
                PrintActionDescription = "creating livecam_stage TSV from file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
        public static void LiveCameraStageTsvToBin(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*livecam_stage_*.tsv";

            Terminal.WriteLine("Live Camera Stage: converting TSV file(s) to binaries.");
            int taskCount = DoFileIOTasks(options, LiveCameraStageTsvToBinFile);
            Terminal.WriteLine($"Live Camera Stage: done converting {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }
        public static void LiveCameraStageTsvToBinFile(Options options, string inputFilePath, string outputFilePath)
        {
            // Load file
            LiveCameraStage liveCameraStage = new LiveCameraStage();
            using (var textReader = new StreamReader(File.OpenRead(inputFilePath)))
            {
                liveCameraStage.Deserialize(textReader);
                liveCameraStage.FileName = Path.GetFileNameWithoutExtension(inputFilePath);
            }

            // 
            var fileWrite = () =>
                {
                    using (var writer = new EndianBinaryWriter(File.Create(outputFilePath), LiveCameraStage.endianness))
                    {
                        liveCameraStage.Serialize(writer);
                    }
                };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFilePath,
                OutputFilePath = outputFilePath,
                PrintDesignator = "LiveCam Stage",
                PrintActionDescription = "creating livecam_stage TSV from file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        public static void EmblemToImage(Options options)
        {
            Terminal.WriteLine("Emblem: converting emblems from BIN files.");
            int binCount = DoFileIOTasks(options, EmblemBinToImages);
            Terminal.WriteLine($"Emblem: done converting {binCount} file{(binCount != 1 ? 's' : "")}.");

            // In this case where no search pattern is set, find *FZE*.GCI (emblem) files.
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*fze*.dat.gci";

            Terminal.WriteLine("Emblem: converting emblems from GCI files.");
            int gciCount = DoFileIOTasks(options, EmblemGciToImage);
            Terminal.WriteLine($"Emblem: done converting {gciCount} file{(gciCount != 1 ? 's' : "")}.");
        }
        private static void EmblemGciToImage(Options options, string inputFilePath, string outputFilePath)
        {
            // Abort method if file does not use correct
            string extension = Path.GetExtension(inputFilePath);
            bool isGciFile = extension == EmblemGCI.Extension;
            if (!isGciFile)
                return;

            var inputFileInfo = new FileDescription(inputFilePath);
            var outputFileInfo = new FileDescription(outputFilePath);
            var x = inputFileInfo.FullPath;

            // Read GCI Emblem data
            var emblemGCI = new EmblemGCI();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFilePath), EmblemGCI.endianness))
            {
                emblemGCI.Deserialize(reader);
                emblemGCI.FileName = Path.GetFileNameWithoutExtension(inputFilePath);
            }

            // Prepare image encoder
            var encoder = new PngEncoder();
            encoder.CompressionLevel = PngCompressionLevel.BestCompression;

            // Strip twice to remove .dat.gci extension
            outputFilePath = StripFileExtensions(outputFilePath);
            outputFilePath = StripFileExtensions(outputFilePath);

            // Info for file write + console print
            var fileWriteInfo = new FileWriteInfo()
            {
                InputFilePath = inputFilePath,
                PrintDesignator = "Emblem",
                PrintActionDescription = "converting",
            };

            // BANNER
            {
                string textureOutputPath = $"{outputFilePath}-banner.png";
                fileWriteInfo.OutputFilePath = textureOutputPath;
                fileWriteInfo.PrintActionDescription = "emblem banner";
                WriteImage(options, encoder, emblemGCI.Emblem.Texture, fileWriteInfo);
            }
            // ICON
            {
                // Strip original file name, replace with GC game code
                string directory = Path.GetDirectoryName(outputFilePath);
                string fileName = $"{emblemGCI.GameCode}-icon.png";
                string textureOutputPath = Path.Combine(directory, fileName);
                fileWriteInfo.OutputFilePath = textureOutputPath;
                fileWriteInfo.PrintActionDescription = "emblem icon";
                WriteImage(options, encoder, emblemGCI.Emblem.Texture, fileWriteInfo);
            }
            // EMBLEM
            {
                string textureOutputPath = $"{outputFilePath}.png";
                fileWriteInfo.OutputFilePath = textureOutputPath;
                fileWriteInfo.PrintActionDescription = "emblem";
                WriteImage(options, encoder, emblemGCI.Emblem.Texture, fileWriteInfo);
            }
        }
        private static void EmblemBinToImages(Options options, string inputFilePath, string outputFilePath)
        {
            string extension = Path.GetExtension(inputFilePath);
            bool isBinFile = extension == EmblemBIN.Extension;
            if (!isBinFile)
                return;

            // Read BIN Emblem data
            var emblemBIN = new EmblemBIN();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFilePath), EmblemBIN.endianness))
            {
                emblemBIN.Deserialize(reader);
                emblemBIN.FileName = Path.GetFileNameWithoutExtension(inputFilePath);
            }

            // Prepare image encoder
            var encoder = new PngEncoder();
            encoder.CompressionLevel = PngCompressionLevel.BestCompression;

            // Info for file write + console print
            var fileWriteInfo = new FileWriteInfo()
            {
                InputFilePath = inputFilePath,
                OutputFilePath = outputFilePath,
                PrintDesignator = "Emblem",
            };

            // Write out each emblem in file
            int index = 0;
            int format = emblemBIN.Emblems.LengthToFormat();
            foreach (var emblem in emblemBIN.Emblems)
            {
                index++;
                string directory = Path.GetDirectoryName(outputFilePath);
                string fileName = $"{emblemBIN.FileName}-{index.PadLeft(format, '0')}.png";
                string textureOutputPath = Path.Combine(directory, fileName);
                fileWriteInfo.PrintActionDescription = $"converting emblem {index} of";
                WriteImage(options, encoder, emblem.Texture, fileWriteInfo);
            }
        }
        public static void ImageToEmblem(Options options)
        {
            Terminal.WriteLine("Emblem: converting image(s) to emblem.bin file.");            
            var emblems = DoFilesToValueTasks(options, ImageToEmblemBin);
            {
                string outputFilePath = CleanPath(options.OutputPath);

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
            }
            Terminal.WriteLine($"Emblem: done converting {emblems.Length} image{(emblems.Length != 1 ? 's' : "")}.");
        }
        public static Emblem ImageToEmblemBin(Options options, string inputFilePath)
        {
            // Make sure some option parameters are appropriate
            bool isTooLarge = IImageResizeOptions.IsSizeTooLarge(options, Emblem._Width, Emblem._Height);
            if (isTooLarge)
            {
                string msg =
                    $"Requested resize ({options.Width},{options.Height}) exceeds the maximum " +
                    $"bounds of an emblem ({Emblem._Width},{Emblem._Height}).";
                throw new ArgumentException(msg);
            }

            Image<Rgba32> image = Image.Load<Rgba32>(inputFilePath);

            // Resize image to fit inside bounds of 64x64
            ResizeOptions ResizeOptions = IImageResizeOptions.GetResizeOptions(options);
            // Emblem size is either 62x62 (1px alpha border, as intended) or 64x64 ("hacker" option)
            int emblemX = options.EmblemHasAlphaBorder ? Emblem._Width - 2 : Emblem._Width;
            int emblemY = options.EmblemHasAlphaBorder ? Emblem._Height - 2 : Emblem._Height;
            // Choose lowest dimensions as the default size (ie: preserve pixel-perfect if possible)
            int defaultX = Math.Min(emblemX, image.Width);
            int defaultY = Math.Min(emblemY, image.Height);
            // Set size override, then resize image
            ResizeOptions.Size = IImageResizeOptions.GetResizeSize(options, defaultX, defaultY);
            image.Mutate(ipc => ipc.Resize(ResizeOptions));

            // Create emblem, textures
            Emblem emblem = new Emblem();
            Texture imageTexture = ImageToTexture(image, TextureFormat.RGB5A3);
            Texture emblemTexture = new Texture(Emblem._Width, Emblem._Height, TextureColor.Clear, TextureFormat.RGB5A3);

            // Copy image texture to emblem center
            // Only works if image is 64px or less! Resize code block prepares this.
            int offsetX = (emblem.Width - image.Width) / 2;
            int offsetY = (emblem.Height - image.Height) / 2;
            Texture.Copy(imageTexture, emblemTexture, offsetX, offsetY);

            // Write some useful information to the terminal
            lock (lock_ConsoleWrite)
            {
                Terminal.Write($"Emblem: ");
                Terminal.Write($"processing image ");
                Terminal.Write(inputFilePath, FileNameColor);
                Terminal.Write($" ({image.Width},{image.Height}).");
                Terminal.WriteLine();
            }

            // Assign return
            emblem.Texture = emblemTexture;
            return emblem;
        }
        public static void ImageToEmblemGci(Options options, string inputFilePath, string outputFilePath)
        {
            // Remove extension
            outputFilePath = StripFileExtensions(outputFilePath);
            outputFilePath += ".gci";

            // Load image
            Image<Rgba32> image = Image.Load<Rgba32>(inputFilePath);
            //
            //Texture emblem = ;

            //  
            var fileWrite = () =>
            {
                // Save emblem
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFilePath,
                OutputFilePath = outputFilePath,
                PrintDesignator = "Emblem",
                PrintActionDescription = "decompressing file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }


        public static Image<Rgba32> TextureToImage(Texture texture)
        {
            Image<Rgba32> image = new Image<Rgba32>(texture.Width, texture.Height);

            for (int y = 0; y < texture.Height; y++)
            {
                for (int x = 0; x < texture.Width; x++)
                {
                    TextureColor pixel = texture[x, y];
                    image[x, y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
                }
            }

            return image;
        }
        public static Texture ImageToTexture(Image<Rgba32> image, TextureFormat textureFormat = TextureFormat.RGBA8)
        {
            var texture = new Texture(image.Width, image.Height, textureFormat);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Rgba32 pixel = image[x, y];
                    texture[x, y] = new TextureColor(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            return texture;
        }

        public static void WriteImage(Options options, IImageEncoder encoder, Texture texture, FileWriteInfo info)
        {
            var action = () =>
            {
                Image<Rgba32> image = TextureToImage(texture);
                image.Save(info.OutputFilePath, encoder);
            };

            FileWriteOverwriteHandler(options, action, info);
        }
        public static void FileWriteOverwriteHandler(Options options, Action fileWrite, FileWriteInfo info)
        {
            bool outputFileExists = File.Exists(info.OutputFilePath);
            bool doWriteFile = !outputFileExists || options.OverwriteFiles;
            bool isOverwritingFile = outputFileExists && doWriteFile;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            lock (lock_ConsoleWrite)
            {
                Terminal.Write($"{info.PrintDesignator}: ");
                if (doWriteFile)
                {
                    Terminal.Write(info.PrintActionDescription);
                    Terminal.Write(" ");
                    Terminal.Write(info.InputFilePath, FileNameColor);
                    Terminal.Write(". ");
                    Terminal.Write(writeMsg, writeColor);
                    Terminal.Write(" file ");
                    Terminal.Write(info.OutputFilePath, FileNameColor);
                }
                else
                {
                    Terminal.Write("skip ");
                    Terminal.Write(info.PrintActionDescription);
                    Terminal.Write(" ");
                    Terminal.Write(info.InputFilePath, FileNameColor);
                    Terminal.Write(" since ");
                    Terminal.Write(info.OutputFilePath, FileNameColor);
                    Terminal.Write(" already exists.");
                }
                Terminal.WriteLine();
            }

            if (doWriteFile)
            {
                fileWrite.Invoke();
            }
        }

    }
}