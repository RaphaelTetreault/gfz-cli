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
using static Manifold.GFZCLI.Options;

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
                Konsole.WriteLine(msg, ConsoleColor.Black, ConsoleColor.Red);
                Konsole.WriteLine();
                args = Help;
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
            switch (options.Action)
            {
                // CARDATA
                case GfzCliAction.cardata_bin_to_tsv: CarDataBinToTsv(options); break;
                case GfzCliAction.cardata_tsv_to_bin: CarDataTsvToBin(options); break;
                //
                case GfzCliAction.live_camera_stage_bin_to_tsv: LiveCameraStageBinToTsv(options); break;
                case GfzCliAction.live_camera_stage_tsv_to_bin: LiveCameraStageTsvToBin(options); break;
                // LZ
                case GfzCliAction.lz_compress: LzCompress(options); break;
                case GfzCliAction.lz_decompress: LzDecompress(options); break;
                // TPL
                case GfzCliAction.tpl_pack: TplPack(options); break;

                // UNSET
                case 0:
                    {
                        string msg = $"Could not parse (-a|--action) '{options.ActionStr}'.";
                        Konsole.WriteLine(msg);
                        Konsole.WriteLine();
                        //PrintAllGfzCliActions();
                        Parser.Default.ParseArguments<Options>(Help).WithParsed(RunOptions);
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
            // Get IO paths
            string inputFilePath = options.InputPath;
            string outputFilePath = GetOutputPath(options, inputFilePath);
            outputFilePath = StripFileExtension(outputFilePath);
            outputFilePath = AppendExtension(outputFilePath, "tsv");

            // Check to make sure file exists
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException($"File at path '{inputFilePath}' does not exist.");

            // Decompress LZ if not decompressed yet
            string extension = Path.GetExtension(inputFilePath);
            bool isLzCompressed = extension.ToLower() == ".lz";
            // Open the file if decompressed, decompress file stream otherwise
            using (Stream fileStream = isLzCompressed ? LzUtility.DecompressAvLz(inputFilePath) : File.OpenRead(inputFilePath))
            {
                using (var reader = new EndianBinaryReader(fileStream, CarData.endianness))
                {
                    var carData = new CarData();
                    carData.Deserialize(reader);

                    //
                    bool doWriteFile = !File.Exists(outputFilePath) || options.OverwriteFiles;
                    if (doWriteFile)
                    {
                        using (var writer = new StreamWriter(File.Create(outputFilePath)))
                            carData.Serialize(writer);
                    }

                    Konsole.Write("CarData: ");
                    if (doWriteFile)
                    {
                        Konsole.Write("Created TSV file: ");
                        Konsole.Write(outputFilePath, FileNameColor);
                    }
                    else
                    {
                        Konsole.Write("Skip creating TSV file: ");
                        Konsole.Write(outputFilePath, FileNameColor);
                        Konsole.Write(". ");
                        Konsole.Write("File already exists.", OverwriteFileColor);
                        Konsole.Write(" Use --");
                        Konsole.Write(Options.Args.OverwriteFiles);
                        Konsole.Write(" if you would like to overwrite files automatically.");
                    }
                    Konsole.WriteLine();
                }
            }
        }
        public static void CarDataTsvToBin(Options options)
        {
            // Get IO paths
            string inputFilePath = options.InputPath;
            string outputFilePath = GetOutputPath(options, inputFilePath);
            outputFilePath = StripFileExtension(outputFilePath);

            // Check to make sure file exists
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException($"File at path '{inputFilePath}' does not exist.");

            // Get CarData
            var carData = new CarData();
            using (var reader = new StreamReader(File.OpenRead(inputFilePath)))
                carData.Deserialize(reader);

            // Write file, if possible
            bool doWriteFile = !File.Exists(outputFilePath) || options.OverwriteFiles;
            if (doWriteFile)
            {
                // Save out file (this file is not yet compressed)
                using (var writer = new EndianBinaryWriter(new MemoryStream(), CarData.endianness))
                {
                    // Write data to stream in memory
                    carData.Serialize(writer);
                    // Create file new file
                    using (var fs = File.Create(outputFilePath))
                    {
                        // Force format to GX since "cardata.lz" is a GX exclusive standalone file.
                        options.SerializationFormat = "gx";
                        // Compress memory stream to file stream
                        GameCube.AmusementVision.LZ.Lz.Pack(writer.BaseStream, fs, options.AvGame);
                    }
                }
            }

            Konsole.Write("CarData: ");
            if (doWriteFile)
            {
                Konsole.Write("Created BIN file: ");
                Konsole.Write(outputFilePath, FileNameColor);
            }
            else
            {
                Konsole.Write($"Skip creating BIN file: ");
                Konsole.Write(outputFilePath, FileNameColor);
                Konsole.Write(". ");
                Konsole.Write("File already exists.", OverwriteFileColor);
                Konsole.Write(" Use --");
                Konsole.Write(Options.Args.OverwriteFiles);
                Konsole.Write(" if you would like to overwrite files automatically.");
            }
            Konsole.WriteLine();
        }

        public static void LzDecompress(Options options)
        {
            Konsole.WriteLine("Decompressing LZ files.");

            // Force checking for LZ files IF there is nothing defined for saerch pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*.lz";

            DoFileTasks(options, LzDecompressFile);
        }
        public static void LzDecompressFile(Options options, string inputFilePath, string outputFilePath)
        {
            // Remove extension
            outputFilePath = StripFileExtension(outputFilePath);

            // Decompress file and save it - if allowed
            bool decompressedFileExists = File.Exists(outputFilePath);
            bool doWriteFile = !decompressedFileExists || options.OverwriteFiles;
            if (doWriteFile)
            {
                // TODO: add LZ function in library to read from inputFilePath, decompress, save to outputFilePath
                using (var stream = LzUtility.DecompressAvLz(inputFilePath))
                using (var writer = File.Create(outputFilePath))
                    writer.Write(stream.ToArray());
            }

            bool isOverwritingFile = decompressedFileExists && doWriteFile;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            lock (lock_ConsoleWrite)
            {
                Konsole.Write("LZ: ");
                if (doWriteFile)
                {
                    Konsole.Write("Decompress file ");
                    Konsole.Write(inputFilePath, FileNameColor);
                    Konsole.Write(". ");
                    Konsole.Write(writeMsg, writeColor);
                    Konsole.Write(" file: ");
                    Konsole.Write(outputFilePath, FileNameColor);
                }
                else
                {
                    Konsole.Write("Skip decompressing file ");
                    Konsole.Write(inputFilePath, FileNameColor);
                    Konsole.Write(" since ");
                    Konsole.Write(outputFilePath, FileNameColor);
                    Konsole.Write(" already exists.");
                }
                Konsole.WriteLine();
            }
        }
        public static void LzCompress(Options options)
        {
            DoFileTasks(options, LzCompressFile);
        }
        public static void LzCompressFile(Options options, string inputFilePath, string outputFilePath)
        {
            outputFilePath = AppendExtension(outputFilePath, "lz");

            // Compress file and save it - if allowed
            bool compressedFileExists = File.Exists(outputFilePath);
            bool doWriteFile = !compressedFileExists || options.OverwriteFiles;
            if (doWriteFile)
            {
                // TODO: add LZ function in library to read from inputFilePath, compress, save to outputFilePath
                using (var stream = LzUtility.CompressAvLz(inputFilePath, options.AvGame))
                using (var writer = File.Create(outputFilePath))
                    writer.Write(stream.ToArray());
            }

            bool isOverwritingFile = compressedFileExists && doWriteFile;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            lock (lock_ConsoleWrite)
            {
                Konsole.Write("LZ: ");
                if (doWriteFile)
                {
                    Konsole.Write("Compress file: ");
                    Konsole.Write(inputFilePath, FileNameColor);
                    Konsole.Write(". ");
                    Konsole.Write(writeMsg, writeColor);
                    Konsole.Write(" file: ");
                    Konsole.Write(outputFilePath, FileNameColor);
                }
                else
                {
                    Konsole.Write("Skip compressing file ");
                    Konsole.Write(inputFilePath, FileNameColor);
                    Konsole.Write(" since ");
                    Konsole.Write(outputFilePath, FileNameColor);
                    Konsole.Write(" already exists.");
                }
                Konsole.WriteLine();
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



        public static void PrintAllGfzCliActions()
        {
            foreach (string enumName in Enum.GetNames<GfzCliAction>())
            {
                string lower = enumName.ToLower();
                string noGap = lower.Replace('_', '-');
                Konsole.Write('\t');
                Konsole.WriteLine(noGap);
            }
        }
    }
}