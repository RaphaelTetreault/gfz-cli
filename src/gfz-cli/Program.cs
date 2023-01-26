using CommandLine;
using GameCube.GFZ.CarData;
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

using static Manifold.GFZCLI.MultithreadFileTools;
using GameCube.GFZ.Camera;

namespace Manifold.GFZCLI
{
    internal class Program
    {
        static readonly object lock_ConsoleWrite = new();
        const ConsoleColor FileNameColor = ConsoleColor.Cyan;
        const ConsoleColor OverwriteFileColor = ConsoleColor.DarkYellow;
        const ConsoleColor WriteFileColor = ConsoleColor.Green;

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
                Konsole.WriteLine(msg, ConsoleColor.Black, ConsoleColor.Red);
                Konsole.WriteLine();
                args = new string[] { "--help" };
            }

            // Run program with options
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);

            // If user did not pass any arguments, pause application so they can read Console.
            // This will happen when users double-click application.
            if (noArgumentsPassed)
            {
                string msg = "Press ENTER to continue.";
                Konsole.WriteLine(msg, ConsoleColor.Black, ConsoleColor.Red);
                Console.Read();
            }
        }

        public static void RunOptions(Options options)
        {
            if (options.Verbose)
            {
                options.PrintState();
                Console.WriteLine();
            }
            VerboseConsole.IsVerbose = options.Verbose;

            // Everything else from here
            CarDataBinToTsv(options);
            CarDataTsvToBin(options);
            //
            LzDecompress(options);
            LzCompress(options);
            //
            TplUnpack(options);
            TplPack(options);
            //
            LiveCameraStageBinToTsv(options);
            LiveCameraStageTsvToBin(options);
        }

        public static void CarDataBinToTsv(Options options)
        {
            string inputFilePath = options.CarDataBinPath;
            if (string.IsNullOrEmpty(inputFilePath))
                return;

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException($"File at path '{inputFilePath}' does not exist.");

            // Decompress LZ if not decompressed yet
            string extension = Path.GetExtension(inputFilePath);
            bool hasNoExtension = string.IsNullOrEmpty(extension);
            if (!hasNoExtension)
            {
                // Find path to potentially uncompressed CarData file
                string fileName = Path.GetFileNameWithoutExtension(inputFilePath);
                string fileDir = Path.GetDirectoryName(inputFilePath);
                string uncompressedFilePath = Path.Combine(fileDir, fileName);
                // Check to see if it exists. 
                bool uncompressedFileExists = File.Exists(uncompressedFilePath);
                if (!uncompressedFileExists)
                {
                    // If not, decompressed file we were asked to convert
                    string outputFilePath = GetOutputPath(options, inputFilePath);
                    LzDecompressFile(options, inputFilePath, outputFilePath);
                }
                // Update path to newly decompressed file
                inputFilePath = uncompressedFilePath;
            }

            using (var reader = new EndianBinaryReader(File.OpenRead(inputFilePath), CarData.endianness))
            {
                var carData = new CarData();
                reader.Read(ref carData);

                // Post message if we can't overwrite output file, stop there.
                string outputPath = inputFilePath + ".tsv";
                if (File.Exists(outputPath) && !options.OverwriteFiles)
                {
                    Konsole.Write($"CarData: Skip creating TSV file: ");
                    Konsole.Write(outputPath, FileNameColor);
                    Konsole.Write(". ");
                    Konsole.Write("File already exists.", OverwriteFileColor);
                    Konsole.Write($" Use --{Options.Args.OverwriteFiles} if");
                    Konsole.Write($" you would like to overwrite files automatically.");
                    Konsole.WriteLine();
                    return;
                }

                using (var writer = new StreamWriter(File.Create(outputPath)))
                {
                    carData.Serialize(writer);
                    Konsole.Write($"CarData: Created TSV file: ");
                    Konsole.Write(outputPath, FileNameColor);
                    Konsole.WriteLine();
                }
            }
        }
        public static void CarDataTsvToBin(Options options)
        {
            string inputFilePath = options.CarDataTsvPath;
            if (string.IsNullOrEmpty(inputFilePath))
                return;

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException($"File at path '{inputFilePath}' does not exist.");

            // Open CarData
            using (var reader = new StreamReader(File.OpenRead(inputFilePath)))
            {
                // Read TSV as CarData
                var carData = new CarData();
                carData.Deserialize(reader);

                // Update path to decompressed file
                string fileName = Path.GetFileNameWithoutExtension(inputFilePath);
                string fileDir = Path.GetDirectoryName(inputFilePath);
                inputFilePath = Path.Combine(fileDir, fileName);

                // Post message if we can't overwrite output file, stop there.
                string outputPath = inputFilePath + ".lz";
                if (File.Exists(outputPath) && !options.OverwriteFiles)
                {
                    Konsole.Write($"CarData: Skip creating BIN file: ");
                    Konsole.Write(outputPath, FileNameColor);
                    Konsole.Write(". ");
                    Konsole.Write("File already exists.", OverwriteFileColor);
                    Konsole.Write($" Use --{Options.Args.OverwriteFiles} if");
                    Konsole.Write($" you would like to overwrite files automatically.");
                    Konsole.WriteLine();
                    return;
                }

                // Save out file (this file is not yet compressed)
                using (var writer = new EndianBinaryWriter(File.Create(outputPath), CarData.endianness))
                {
                    carData.Serialize(writer);
                    Konsole.Write($"CarData: Created BIN file: ");
                    Konsole.Write(outputPath, FileNameColor);
                    Konsole.WriteLine();
                }
                // Force format to GX since Cardata only exists as a file there.
                options.SerializationFormat = "gx";
                string outputFilePath = GetOutputPath(options, inputFilePath);
                LzCompressFile(options, inputFilePath, outputFilePath);
                //LzUtility.CompressAvLzToDisk(filePath, GameCube.AmusementVision.GxGame.FZeroGX, true);
            }
        }

        public static void LzDecompress(Options options)
        {
            //string path = options.LzDecompressTarget;
            //if (string.IsNullOrEmpty(path))
            //    return;

            if (!options.LzDecompress)
                return;

            // Force checking for LZ files IF there is nothing defined for saerch pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*.lz";

            DoFileTasks(options, LzDecompressFile);
        }
        public static void LzDecompressFile(Options options, string inputFilePath, string outputFilePath)
        {
            bool decompressedFileExists = File.Exists(outputFilePath);
            bool doWriteFile = !decompressedFileExists || options.OverwriteFiles;
            if (doWriteFile)
            {
                using (var stream = LzUtility.CompressAvLz(inputFilePath, options.AvGame))
                    using (var writer = File.Create(outputFilePath))
                        writer.Write(stream.ToArray());
            }

            //bool writeSuccess = LzUtility.DecompressAvLzToDisk(inputFilePath, options.OverwriteFiles);

            string inputFileName = Path.GetFileName(inputFilePath);
            string outputFileName = Path.GetFileNameWithoutExtension(inputFileName);
            bool isOverwritingFile = decompressedFileExists && doWriteFile;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            lock (lock_ConsoleWrite)
            {
                if (doWriteFile)
                {
                    Konsole.Write($"LZ: Decompress file: ");
                    Konsole.Write(inputFileName, FileNameColor);
                    Konsole.Write(" - ");
                    Konsole.Write(writeMsg, writeColor);
                    Konsole.Write($" file: ");
                    Konsole.Write(outputFileName, FileNameColor);
                    Konsole.WriteLine();
                }
                else
                {
                    Konsole.Write($"LZ: Skip decompressing file: ");
                    Konsole.Write(inputFileName, FileNameColor);
                    Konsole.WriteLine();
                }
            }
        }
        public static void LzCompress(Options options)
        {
            string lzcPath = options.LzCompressTarget;
            if (string.IsNullOrEmpty(lzcPath))
                return;

            DoFileTasks(options, LzCompressFile);
        }
        public static void LzCompressFile(Options options, string filePath, string _outFile)
        {
            bool fileExists = File.Exists(filePath);
            bool writeSuccess = LzUtility.CompressAvLzToDisk(filePath, options.AvGame, options.OverwriteFiles);

            //if (!options.Verbose)
            //    return;

            string inputFileName = Path.GetFileName(filePath);
            string outputFileName = $"{inputFileName}.lz";
            bool isOverwritingFile = fileExists && writeSuccess;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            lock (lock_ConsoleWrite)
            {
                if (writeSuccess)
                {
                    Konsole.Write($"LZ: Compress file: ");
                    Konsole.Write(inputFileName, FileNameColor);
                    Konsole.Write($" - ");
                    Konsole.Write(writeMsg, writeColor);
                    Konsole.Write($" file: ");
                    Konsole.Write(outputFileName, FileNameColor);
                    Konsole.WriteLine();
                }
                else
                {
                    Konsole.Write($"LZ: Skip compressing file: ");
                    Konsole.Write(inputFileName, FileNameColor);
                    Konsole.WriteLine();
                }
            }
        }

        public static void TplUnpack(Options options)
        {
            string path = options.TplUnpack;
            if (string.IsNullOrEmpty(path))
                return;

            DoFileTasks(options, TplUnpackFile);
        }
        public static void TplUnpackFile(Options options, string filePath, string _outFile)
        {
            // Deserialize the TPL
            Tpl tpl = new Tpl();
            using (var reader = new EndianBinaryReader(File.OpenRead(filePath), Tpl.endianness))
            {
                tpl.Deserialize(reader);
                tpl.FileName = Path.GetFileNameWithoutExtension(filePath);
            }

            // File name, useful for some later stuff.
            string tplFileName = Path.GetFileNameWithoutExtension(filePath);
            // Create folder named the same thing as the TPL input file
            string directory = Path.GetDirectoryName(filePath);
            directory = Path.Combine(directory, tpl.FileName);
            Directory.CreateDirectory(directory);

            // Iterate over texture and mipmaps, save to disk
            int tplIndex = 0;
            //int tplDigits = tpl.TextureSeries.LengthToFormat();
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

                    // Copy contents of GameCube texture into ImageSharp representation
                    var texture = textureEntry.Texture;
                    Image<Rgba32> image = new Image<Rgba32>(texture.Width, texture.Height);

                    for (int y = 0; y < texture.Height; y++)
                    {
                        for (int x = 0; x < texture.Width; x++)
                        {
                            TextureColor pixel = texture[x, y];
                            image[x, y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
                        }
                    }

                    //var format = PngFormat.Instance;
                    var encoder = new PngEncoder();
                    encoder.CompressionLevel = PngCompressionLevel.BestCompression;
                    string textureHash = textureSeries.MD5TextureHashes[entryIndex];
                    string fileName = $"{tplIndex}-{mipmapIndex}-{texture.Format}-{textureHash}.png";
                    string outputPath = Path.Combine(directory, fileName);

                    bool fileExists = File.Exists(outputPath);
                    bool skipFileWrite = fileExists && !options.OverwriteFiles;
                    if (skipFileWrite)
                    {
                        lock (lock_ConsoleWrite)
                        {
                            Konsole.Write($"TPL: Skip unpacking file: ");
                            Konsole.Write(fileName);
                            Konsole.WriteLine();
                        }
                        continue;
                    }

                    // Save to disk
                    image.Save(outputPath, encoder);

                    // Output message
                    bool isOverwritingFile = fileExists;
                    var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
                    var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";
                    lock (lock_ConsoleWrite)
                    {
                        Konsole.Write($"TPL: Unpacking file: ");
                        Konsole.Write(tplFileName, FileNameColor);
                        Konsole.Write($" Texture {tplIndex},");
                        Konsole.Write($" Mipmap {mipmapIndex}. ");
                        Konsole.Write(writeMsg, writeColor);
                        Konsole.Write($" file: ");
                        Konsole.Write(outputPath, FileNameColor);
                        Konsole.WriteLine();
                    }
                }
            }
        }
        public static void TplPack(Options options)
        {
            string path = options.TplPack;
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
                    Konsole.WriteLine($"Wrote file: {filePath}");
                }
            }
        }


        public static void LiveCameraStageBinToTsv(Options options)
        {
            string path = options.LiveCameraStageBinToTsvPath;
            if (string.IsNullOrEmpty(path))
                return;

            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*livecam_stage_*.bin";

            DoFileTasks(options, LiveCameraStageBinToTsvFile);
        }
        public static void LiveCameraStageBinToTsvFile(Options options, string filePath, string _outFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string outputFileName = $"{fileName}.tsv";
            string directory = Path.GetDirectoryName(filePath);
            string outputFilePath = Path.Combine(directory, outputFileName);
            bool outputFileExists = File.Exists(outputFilePath);
            bool isOverwritingFile = outputFileExists && options.OverwriteFiles;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            bool isCreatingFile = !outputFileExists || isOverwritingFile;
            if (isCreatingFile)
            {
                // Deserialize the file
                LiveCameraStage liveCameraStage = new LiveCameraStage();
                using (var reader = new EndianBinaryReader(File.OpenRead(filePath), LiveCameraStage.endianness))
                {
                    liveCameraStage.Deserialize(reader);
                    liveCameraStage.FileName = fileName;
                }

                // Write it to the stream
                using (var textWriter = new StreamWriter(File.Create(outputFilePath)))
                {
                    liveCameraStage.Serialize(textWriter);
                }
            }

            lock (lock_ConsoleWrite)
            {
                if (isCreatingFile)
                {
                    Konsole.Write($"Live Camera Stage: Create TSV file: ");
                    Konsole.Write(fileName, FileNameColor);
                    Konsole.Write($" - ");
                    Konsole.Write(writeMsg, writeColor);
                    Konsole.Write($" file: ");
                    Konsole.Write(outputFileName, FileNameColor);
                    Konsole.WriteLine();
                }
                else
                {
                    Konsole.Write($"Live Camera Stage: Skip creating TSV file: ");
                    Konsole.Write(fileName, FileNameColor);
                    Konsole.WriteLine();
                }
            }
        }



        public static void LiveCameraStageTsvToBin(Options options)
        {
            string path = options.LiveCameraStageTsvToBinPath;
            if (string.IsNullOrEmpty(path))
                return;

            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*livecam_stage_*.tsv";

            DoFileTasks(options, LiveCameraStageTsvToBinFile);
        }

        public static void LiveCameraStageTsvToBinFile(Options options, string filePath, string _outFile)
        {
            string inputFileName = Path.GetFileNameWithoutExtension(filePath);
            string outputFileName = $"{inputFileName}.bin";
            string inputDirectory = Path.GetDirectoryName(filePath);
            string outputFilePath = Path.Combine(inputDirectory, outputFileName);
            bool outputFileExists = File.Exists(outputFilePath);
            bool isOverwritingFile = outputFileExists && options.OverwriteFiles;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            bool isCreatingFile = !outputFileExists || isOverwritingFile;
            if (isCreatingFile)
            {
                LiveCameraStage liveCameraStage = new LiveCameraStage();
                using (var textReader = new StreamReader(File.OpenRead(filePath)))
                {
                    liveCameraStage.Deserialize(textReader);
                    liveCameraStage.FileName = inputFileName;
                }

                using (var writer = new EndianBinaryWriter(File.Create(outputFilePath), LiveCameraStage.endianness))
                {
                    liveCameraStage.Serialize(writer);
                }
            }

            lock (lock_ConsoleWrite)
            {
                if (isCreatingFile)
                {
                    Konsole.Write($"Live Camera Stage: Create TSV file: ");
                    Konsole.Write(inputFileName, FileNameColor);
                    Konsole.Write($" - ");
                    Konsole.Write(writeMsg, writeColor);
                    Konsole.Write($" file: ");
                    Konsole.Write(outputFileName, FileNameColor);
                    Konsole.WriteLine();
                }
                else
                {
                    Konsole.Write($"Live Camera Stage: Skip creating TSV file: ");
                    Konsole.Write(inputFileName, FileNameColor);
                    Konsole.WriteLine();
                }
            }
        }
    }
}