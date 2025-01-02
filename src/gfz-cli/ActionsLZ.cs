using GameCube.GFZ.LZ;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

public static class ActionsLZ
{
    public static readonly GfzCliAction ActionLzCompress = new()
    {
        Description = "Compress files into an LZ file.",
        Action = LzCompress,
        ActionID = CliActionID.lz_compress,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.FOPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionLzDecompress = new()
    {
        Description = "Decompress an LZ file.",
        Action = LzDecompress,
        ActionID = CliActionID.lz_decompress,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static void LzDecompress(Options options)
    {
        // Force checking for .LZ only IF there is no defined search pattern
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = $"*.lz";

        Terminal.WriteLine($"LZ: decompressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, LzDecompressFile);
        Terminal.WriteLine($"LZ: done decompressing {taskCount} file{Plural(taskCount)}.");
    }

    public static void LzDecompressFile(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Remove extension
        outputFile.PopExtension();

        // 
        var fileWrite = () =>
        {
            // TODO: add LZ function in library to read from inputFilePath, decompress, save to outputFilePath
            using (var stream = LzUtility.DecompressAvLz(inputFile))
            {
                using (var writer = File.Create(outputFile))
                {
                    writer.Write(stream.ToArray());
                }
            }
        };
        var info = new FileWriteInfo()
        {
            InputFilePath = inputFile,
            OutputFilePath = outputFile,
            PrintPrefix = "LZ",
            PrintActionDescription = "decompressing file",
        };
        FileWriteOverwriteHandler(options, fileWrite, info);
    }

    public static void LzCompress(Options options)
    {
        Terminal.WriteLine("LZ: Compressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, LzCompressFile);
        Terminal.WriteLine($"LZ: done compressing {taskCount} file{(taskCount != 1 ? 's' : "")}.");
    }

    public static void LzCompressFile(Options options, OSPath inputFile, OSPath outputFile)
    {
        outputFile.PushExtension(".lz");

        var fileWrite = () =>
        {
            // TODO: add LZ function in library to read from inputFile, compress, save to outputFile
            using (var stream = LzUtility.CompressAvLz(inputFile, options.AvGame))
            {
                using (var writer = File.Create(outputFile))
                {
                    writer.Write(stream.ToArray());
                }
            }
        };
        var info = new FileWriteInfo()
        {
            InputFilePath = inputFile,
            OutputFilePath = outputFile,
            PrintPrefix = "LZ",
            PrintActionDescription = "compressing input file",
        };
        FileWriteOverwriteHandler(options, fileWrite, info);
    }
}
