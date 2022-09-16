using CommandLine;
using GameCube.GFZ.CarData;
using GameCube.GFZ.LZ;
using Manifold;
using Manifold.IO;
using System;
using System.IO;


namespace Manifold.GFZCLI
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
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
            CarDataToTSV(options);
            CarDataToBIN(options);
            //
            LzDecompress(options);
            LzCompress(options);
        }

        public static void CarDataToTSV(Options options)
        {
            string filePath = options.CarDataBinPath;
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File at path {filePath} does not exist.");

            // Decompress LZ if not decompressed yet
            string extension = Path.GetExtension(filePath);
            if (!string.IsNullOrEmpty(extension))
            {
                // Save decompressed file
                LzUtility.DecompressAvLzToDisk(filePath, true);
                VerboseConsole.WriteLine($"Decompressed {filePath} for conversion.");
                // Update path to decompressed file
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDir = Path.GetDirectoryName(filePath);
                filePath = Path.Combine(fileDir, fileName);
            }

            using (var reader = new EndianBinaryReader(File.OpenRead(filePath), CarData.endianness))
            {
                var carData = new CarData();
                reader.Read(ref carData);

                string outputPath = filePath + ".tsv";
                using (var writer = new StreamWriter(File.Create(outputPath)))
                {
                    carData.Serialize(writer);
                    Console.WriteLine($"Created file: {outputPath}");
                }
            }
        }
        public static void CarDataToBIN(Options options)
        {
            string filePath = options.CarDataTsvPath;
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File at path {filePath} does not exist.");

            // Open CarData
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                var carData = new CarData();
                carData.Deserialize(reader);

                // Save out 
                string outputPath = filePath + ".bin";
                using (var writer = new EndianBinaryWriter(File.Create(outputPath), CarData.endianness))
                {
                    carData.Serialize(writer);
                    Console.WriteLine($"Created file: {outputPath}");
                }
                LzUtility.CompressAvLz(outputPath, GameCube.AmusementVision.GxGame.FZeroGX);
            }
        }

        public static void LzDecompress(Options options)
        {
            string path = options.LzDecompressTarget;
            if (string.IsNullOrEmpty(path))
                return;

            bool fileExists = File.Exists(path);
            bool dirExists = Directory.Exists(path);
            if (!fileExists && !dirExists)
                throw new ArgumentException($"Provided file or folder path target {path} does not exist.");

            if (fileExists)
            {
                LzUtility.DecompressAvLzToDisk(path, true);
                Console.WriteLine($"Decompressed: {path}");
            }
            else if (dirExists)
            {
                var searchOption = options.SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var enumerable = LzUtility.DecompressAvLzDirectoryToDisk(path, true, searchOption);
                foreach (var file in enumerable)
                    Console.WriteLine($"Decompressed: {file.filePath}");
            }
            else
            {
                Assert.IsTrue(false, "This code should never run.");
            }
        }
        public static void LzCompress(Options options)
        {
            string path = options.LzCompressTarget;
            if (string.IsNullOrEmpty(path))
                return;

            bool fileExists = File.Exists(path);
            bool dirExists = Directory.Exists(path);
            if (!fileExists && !dirExists)
                throw new ArgumentException($"Provided file or folder path target {path} does not exist.");
            bool hasSearchPattern = !string.IsNullOrEmpty(options.SearchPattern);
            if (dirExists && !hasSearchPattern)
                throw new ArgumentException($"Cannot compress target folder {path} without specifying a search pattern.");

            if (fileExists)
            {
                LzUtility.CompressAvLzToDisk(path, options.AvGame, true);
                Console.WriteLine($"Compressed: {path}");
            }
            else if (dirExists)
            {
                var searchOption = options.SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var enumerable = LzUtility.CompressAvLzDirectoryToDisk(path, options.AvGame, true, searchOption, options.SearchPattern);
                foreach (var file in enumerable)
                    Console.WriteLine($"Compressed: {file.filePath}");
            }
            else
            {
                Assert.IsTrue(false, "This code should never run.");
            }
        }

    }
}