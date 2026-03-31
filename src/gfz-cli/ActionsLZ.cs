using GameCube.AmusementVision.LZ;
using GameCube.GFZ.LZ;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

public static class ActionsLZ
{
    public static readonly GfzCliAction ActionLZCompress = new()
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

    public static readonly GfzCliAction ActionLZDecompress = new()
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

        Terminal.WriteLine($"{options.ActionStr}: decompressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, LzDecompressFile);
        Terminal.WriteLine($"{options.ActionStr}: done decompressing {taskCount} file{Plural(taskCount)}.");
    }

    public static void LzDecompressFile(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Remove extension
        outputFile.PopExtension();
        if (CanWriteFileAndPrintResult(options, outputFile))
        {
            using var stream = LzUtility.DecompressAvLz(inputFile);
            using var writer = File.Create(outputFile);
            writer.Write(stream.ToArray());
        }
    }

    public static void LzCompress(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: compressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, LzCompressFile);
        Terminal.WriteLine($"{options.ActionStr}: compressed {taskCount} file{(taskCount != 1 ? 's' : "")}.");
    }

    public static void LzCompressFile(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Don't mutate incoming reference
        outputFile = outputFile.Copy();
        outputFile.PushExtension("lz");
        if (CanWriteFileAndPrintResult(options, outputFile))
        {
            LzHeaderType lzHeaderType = Lz.GfzGameCodeToLzHeaderType(options.GameCode);
            using var stream = LzUtility.CompressAvLz(inputFile, lzHeaderType);
            using var writer = File.Create(outputFile);
            writer.Write(stream.ToArray());
        }
    }
}
