using GameCube.GFZ.CarData;
using GameCube.GFZ.LZ;
using Manifold.IO;
using Manifold.Text.Tables;
using System;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

/// <summary>
///     Actions for managing GFZ file ./game/cardata.
/// </summary>
/// <remarks>
///     CarData standalone file exists only for F-Zero GX. See <see cref="ActionsLineREL.PatchSetCarData"/>
///     for applying CarData stats to the Machine Select screen.
/// </remarks>
public static class ActionsCarData
{
    public static readonly GfzCliAction ActionCarDataToTSV = new()
    {
        Description = "Create a TSV from CarData binary (compressed or uncompressed).",
        Action = CarDataToTsv,
        ActionID = CliActionID.cardata_to_tsv,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionCardDataFromTSV = new()
    {
        Description = "Create a CarData.lz file from CarData TSV spreadsheet.",
        Action = CarDataFromTsv,
        ActionID = CliActionID.cardata_from_tsv,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };


    /// <summary>
    ///     Create a TSV from CarData binary (compressed or uncompressed).
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentException">Thrown if serialization format is AX.</exception>
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
    }

    /// <summary>
    ///     Create a TSV from CarData binary (compressed or uncompressed).
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    public static void CarDataBinToTsv(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Read file
        // Decompress LZ if not decompressed yet
        bool isLzCompressed = inputFile.IsOfExtension(".lz");
        // Open the file if decompressed, decompress file stream otherwise
        var carData = new CarData();
        using (Stream fileStream = isLzCompressed ? LzUtility.DecompressAvLz(inputFile) : File.OpenRead(inputFile))
        using (var reader = new EndianBinaryReader(fileStream, CarData.endianness))
            carData.Deserialize(reader);

        // Write TSV file
        outputFile.SetExtensions(".tsv");
        bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
        PrintFileWriteResult(result, outputFile, options.ActionStr);
        if (doWriteFile)
        {
            TableCollection tableCollection = new();
            tableCollection.Add(carData.CreateTables());
            tableCollection.ToFile(outputFile, TableEncodingTSV.Encoding);
        }
    }

    /// <summary>
    ///     Create a CarData.lz file from CarData TSV spreadsheet.
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentException">Thrown if serialization format is AX.</exception>
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
    }

    /// <summary>
    ///     Create a CarData.lz file from CarData TSV spreadsheet.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    public static void CarDataTsvToBin(Options options, OSPath inputFile, OSPath outputFile)
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
            using var writer = new EndianBinaryWriter(new MemoryStream(), CarData.endianness);
            // Write data to stream in memory
            carData.Serialize(writer);

            // COMPRESSED
            // Create new file (actual output file)
            using var cardataFile = File.Create(outputFile);
            // Compress memory stream into file stream
            GameCube.AmusementVision.LZ.Lz.Pack(writer.BaseStream, cardataFile, options.AvGame);
        }
    }
}
