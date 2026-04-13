using GameCube.AmusementVision.LZ;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

public static class ActionsLZ
{
    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionLZDecompress"/>
    /// </remarks>
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

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionLZCompress"/>
    /// </remarks>
    public static void LzCompress(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: compressing file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, LzCompressFile);
        Terminal.WriteLine($"{options.ActionStr}: compressed {taskCount} file{(taskCount != 1 ? 's' : "")}.");
    }



    public static void LzDecompressFile(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Remove extension
        outputFile.PopExtension();
        if (CanWriteFileAndPrintResult(options, outputFile))
        {
            using var stream = Lz.Decompress(inputFile);
            using var writer = File.Create(outputFile);
            writer.Write(stream.ToArray());
        }
    }

    public static void LzCompressFile(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Don't mutate incoming reference
        outputFile = outputFile.Copy();
        outputFile.PushExtension("lz");
        if (CanWriteFileAndPrintResult(options, outputFile))
        {
            LzHeaderType lzHeaderType = Lz.GfzGameCodeToLzHeaderType(options.GameCode);
            using var stream = Lz.Compress(inputFile, lzHeaderType);
            using var writer = File.Create(outputFile);
            writer.Write(stream.ToArray());
        }
    }
}
