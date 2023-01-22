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
        }

        public static void CarDataBinToTsv(Options options)
        {
            string filePath = options.CarDataBinPath;
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File at path '{filePath}' does not exist.");

            // Decompress LZ if not decompressed yet
            string extension = Path.GetExtension(filePath);
            bool hasNoExtension = string.IsNullOrEmpty(extension);
            if (!hasNoExtension)
            {
                // Find path to potentially uncompressed CarData file
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDir = Path.GetDirectoryName(filePath);
                string uncompressedFilePath = Path.Combine(fileDir, fileName);
                // Check to see if it exists. 
                bool uncompressedFileExists = File.Exists(uncompressedFilePath);
                if (!uncompressedFileExists)
                {
                    // If not, decompressed file we were asked to convert
                    LzDecompressFile(options, filePath);
                }
                // Update path to newly decompressed file
                filePath = uncompressedFilePath;
            }

            using (var reader = new EndianBinaryReader(File.OpenRead(filePath), CarData.endianness))
            {
                var carData = new CarData();
                reader.Read(ref carData);

                // Post message if we can't overwrite output file, stop there.
                string outputPath = filePath + ".tsv";
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
            string filePath = options.CarDataTsvPath;
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File at path '{filePath}' does not exist.");

            // Open CarData
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                // Read TSV as CarData
                var carData = new CarData();
                carData.Deserialize(reader);

                // Update path to decompressed file
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDir = Path.GetDirectoryName(filePath);
                filePath = Path.Combine(fileDir, fileName);

                // Post message if we can't overwrite output file, stop there.
                string outputPath = filePath + ".lz";
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
                LzCompressFile(options, filePath);
                //LzUtility.CompressAvLzToDisk(filePath, GameCube.AmusementVision.GxGame.FZeroGX, true);
            }
        }

        public static void LzDecompress(Options options)
        {
            string path = options.LzDecompressTarget;
            if (string.IsNullOrEmpty(path))
                return;

            // Force checking for LZ files IF there is nothing defined for saerch pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*.lz";

            DoFileTasks(options, path, LzDecompressFile);
        }
        public static void LzDecompressFile(Options options, string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string path = Path.Combine(dir, fileName);
            bool decompressedFileExists = File.Exists(path);

            bool writeSuccess = LzUtility.DecompressAvLzToDisk(filePath, options.OverwriteFiles, out string outputFilePath);

            // Sanity check
            bool outputFilePathsMatch = path == outputFilePath;
            if (!outputFilePathsMatch)
                throw new Exception("Mismatched string error. Comparision failed.");

            //if (!options.Verbose)
            //    return;

            string inputFileName = Path.GetFileName(filePath);
            string outputFileName = Path.GetFileNameWithoutExtension(inputFileName);
            bool isOverwritingFile = decompressedFileExists && writeSuccess;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            lock (lock_ConsoleWrite)
            {
                if (writeSuccess)
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

            DoFileTasks(options, lzcPath, LzCompressFile);
        }
        public static void LzCompressFile(Options options, string path)
        {
            bool fileExists = File.Exists(path);
            bool writeSuccess = LzUtility.CompressAvLzToDisk(path, options.AvGame, options.OverwriteFiles);

            //if (!options.Verbose)
            //    return;

            string inputFileName = Path.GetFileName(path);
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

            DoFileTasks(options, path, TplUnpackFile);
        }
        public static void TplUnpackFile(Options options, string filePath)
        {
            string tplFileName = Path.GetFileNameWithoutExtension(filePath);

            // Deserialize the TPL
            Tpl tpl = new Tpl();
            using (var reader = new EndianBinaryReader(File.OpenRead(filePath), Tpl.endianness))
            {
                tpl.Deserialize(reader);
                tpl.FileName = Path.GetFileNameWithoutExtension(filePath);
            }

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

        // NEW MAGIC SAUCE
        // These functions let me easily write multi-threaded code when operating on multiple files.

        public delegate void FileTask(Options options, string filePath);
        public static void DoFileTasks(Options options, string path, FileTask fileTask)
        {
            // Get the file or all files at 'path'
            string[] filePaths = GetFileOrFiles(options, path);

            // For each file, queue it as a task - multithreaded
            List<Task> tasks = new List<Task>();
            foreach (var _filePath in filePaths)
            {
                var action = () => { fileTask(options, _filePath); };
                var task = Task.Factory.StartNew(action);
                tasks.Add(task);
            }

            // Wait for tasks to finish before returning
            var tasksFinished = Task.WhenAll(tasks);
            tasksFinished.Wait();
        }
        public static string[] GetFilesInDirectory(Options options, string path)
        {
            string[] files = new string[0];

            bool directoryExists = Directory.Exists(path);
            if (directoryExists)
            {
                bool isInvalidSearchOption = string.IsNullOrEmpty(options.SearchPattern);
                if (isInvalidSearchOption)
                {
                    string msg =
                        $"Invalid '{nameof(options.SearchPattern)}' provided for a directory input argument. " +
                        $"Make sure to use --{Options.Args.SearchPattern} when providing directory paths.";
                    throw new ArgumentException(msg);
                }
                files = Directory.GetFiles(path, options.SearchPattern, options.SearchOption);
            }
            return files;
        }
        public static string[] GetFileOrFiles(Options options, string path)
        {
            // Make sure path is valid as either a file or folder
            bool fileExists = File.Exists(path);
            bool dirExists = Directory.Exists(path);
            if (!fileExists && !dirExists)
            {
                string msg = $"Target file or folder '{path}' does not exist.";
                throw new ArgumentException(msg);
            }

            // Get files in directory if it is a directory
            string[] files = GetFilesInDirectory(options, path);
            bool isDirectory = files.Length > 0;
            if (!isDirectory)
            {
                // Since we know 'path' is either a file or directory, and it
                // isn't a directory, return 'path' as the file.
                files = new string[] { path };
            }

            return files;
        }
    }
}