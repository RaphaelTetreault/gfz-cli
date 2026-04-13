using GameCube.AmusementVision.LZ;
using GameCube.GFZ.CarData;
using Manifold.IO;
using Manifold.Text.Tables;
using System;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for managing GFZ file ./game/cardata.
/// </summary>
/// <remarks>
///     CarData standalone file exists only for F-Zero GX. See <see cref="ActionsREL.PatchSetCarData"/>
///     for applying CarData stats to the Machine Select screen.
/// </remarks>
public static class ActionsCarData
{
    /// <summary>
    ///     Create a TSV from CarData binary (compressed or uncompressed).
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentException">Thrown if serialization format is AX.</exception>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionCarDataToTSV"/>
    /// </remarks>
    public static void CarDataToTsv(Options options)
    {
        // Stop if desired file format is AX
        bool isInvalidFormat = options.SerializeFormat == GameCube.GFZ.Stage.SerializeFormat.AX;
        if (isInvalidFormat)
        {
            string msg = $"Cannot convert F-Zero AX cardata file '{options.InputPath}'";
            throw new ArgumentException(msg);
        }

        // Perform the action
        ParallelizeFileInFileOutTasks(options, CarDataBinToTsv);

        static void CarDataBinToTsv(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Read file
            // Decompress LZ if not decompressed yet
            bool isLzCompressed = inputFile.IsOfExtension(".lz");
            // Open the file if decompressed, decompress file stream otherwise
            var carData = new CarData();
            using (Stream fileStream = isLzCompressed ? Lz.Decompress(inputFile) : File.OpenRead(inputFile))
            using (var reader = new EndianBinaryReader(fileStream, CarDataFile.endianness))
                carData.Deserialize(reader);

            // Write TSV file
            outputFile.SetExtensions(".tsv");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                TableCollection tableCollection = [];
                Table[] table = carData.CreateTables();
                tableCollection.Add(table);
                tableCollection.ToFile(outputFile, TableEncodingTSV.Encoding);
            }
        }
    }

    /// <summary>
    ///     Create a CarData.lz file from CarData TSV spreadsheet.
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentException">Thrown if serialization format is AX.</exception>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionCarDataToTSV"/>
    /// </remarks>
    public static void CarDataFromTsv(Options options)
    {
        // Stop if desired file format is AX
        bool isInvalidFormat = options.SerializeFormat == GameCube.GFZ.Stage.SerializeFormat.AX;
        if (isInvalidFormat)
        {
            string msg = $"Cannot convert '{options.InputPath}' for use in F-Zero AX.";
            throw new ArgumentException(msg);
        }

        // Perform the action
        ParallelizeFileInFileOutTasks(options, CarDataTsvToBin);

        static void CarDataTsvToBin(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Get CarData TSV
            var carData = new CarData();
            using (var reader = new StreamReader(File.OpenRead(inputFile)))
                carData.Deserialize(reader);

            // Write CarData.lz file
            outputFile.SetExtensions(".lz");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                // UNCOMPRESSED
                // Save out file (this file is not yet compressed)
                using var writer = new EndianBinaryWriter(new MemoryStream(), CarDataFile.endianness);
                // Write data to stream in memory
                carData.Serialize(writer);

                // COMPRESSED
                // Create new file (actual output file)
                using var cardataFile = File.Create(outputFile);
                // Compress memory stream into file stream
                Lz.Pack(writer.BaseStream, cardataFile, options.GameCode);
            }
        }
    }

}
