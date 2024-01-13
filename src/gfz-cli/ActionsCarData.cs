using GameCube.GFZ.CarData;
using GameCube.GFZ.LZ;
using Manifold.IO;
using Manifold.Text.Tables;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsCarData
    {
        public static void CarDataToTsv(Options options)
        {
            // Stop if desired file format is AX
            bool isInvalidFormat = options.SerializeFormat == GameCube.GFZ.Stage.SerializeFormat.AX;
            if (isInvalidFormat)
            {
                string msg = $"Cannot convert F-Zero AX cardata file '{options.InputPath}'";
                throw new System.ArgumentException(msg);
            }

            // Perform the action
            DoFileInFileOutTasks(options, CarDataBinToTsvTask);
        }

        public static void CarDataBinToTsvTask(Options options, FilePath inputFile, FilePath outputFile)
        {
            outputFile.SetExtensions(".tsv");

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
                TableCollection tables = new TableCollection();
                carData.AddToTables(tables);

                //using var writer = new StreamWriter(File.Create(outputFile));
                tables.ToFile(outputFile, TableEncodingTSV.Encoding);
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

        public static void CarDataFromTsv(Options options)
        {
            // Stop if desired file format is AX
            bool isInvalidFormat = options.SerializeFormat == GameCube.GFZ.Stage.SerializeFormat.AX;
            if (isInvalidFormat)
            {
                string msg = $"Cannot convert '{options.InputPath}' for use in F-Zero AX.";
                throw new System.ArgumentException(msg);
            }

            // Perform the action
            DoFileInFileOutTasks(options, CarDataTsvToBin);
        }

        public static void CarDataTsvToBin(Options options, FilePath inputFile, FilePath outputFilePath)
        {
            outputFilePath.SetExtensions(".lz");

            // Get CarData
            var carData = new CarData();
            using (var reader = new StreamReader(File.OpenRead(inputFile)))
                carData.Deserialize(reader);

            var fileWrite = () =>
            {
                // UNCOMPRESSED
                // Save out file (this file is not yet compressed)
                using var writer = new EndianBinaryWriter(new MemoryStream(), CarData.endianness);
                // Write data to stream in memory
                carData.Serialize(writer);

                // COMPRESSED
                // Create new file (actual output file)
                using var outputFile = File.Create(outputFilePath);
                // Compress memory stream into file stream
                GameCube.AmusementVision.LZ.Lz.Pack(writer.BaseStream, outputFile, options.AvGame);
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFilePath,
                PrintDesignator = "CarData",
                PrintActionDescription = "creating cardata BIN from file",
                PrintMoreInfoOnSkip =
                    $"Use --{IGfzCliOptions.Args.OverwriteFiles} if you would like to overwrite files automatically.",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
    }
}
