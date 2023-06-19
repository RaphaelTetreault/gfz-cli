using GameCube.GFZ.CarData;
using GameCube.GFZ.LZ;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsCarData
    {

        public static void CarDataBinToTsv(Options options) => DoFileInFileOutTasks(options, CarDataBinToTsvTask);

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

        public static void CarDataTsvToBin(Options options) => DoFileInFileOutTasks(options, CarDataTsvToBin);

        public static void CarDataTsvToBin(Options options, FilePath inputFile, FilePath outputFile)
        {
            outputFile.SetExtensions(".lz");

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
    }
}
