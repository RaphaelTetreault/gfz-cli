using GameCube.GFZ.GMA;
using GameCube.GFZ.Stage;
using GameCube.GFZ.TPL;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

/// <summary>
///     Round-trip IO actions (deserialize file, reserialize file).
/// </summary>
public static class ActionsIO
{
    public static void InOutGMA(Options options) => InOutFiles<Gma>(options, "*.gma");
    public static void InOutTPL(Options options) => InOutFiles<Tpl>(options, "*.tpl");
    public static void InOutScene(Options options) => InOutFiles<Scene>(options, "COLI_COURSE???");

    public static void InOutFiles<TFile>(Options options, string searchPattern)
        where TFile : IBinaryFileType, IBinarySerializable, new()
    {
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = searchPattern;

        string typeName = typeof(TFile).Name;
        Terminal.WriteLine($"IO {typeName}: in-out re-serialization of file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, InOutFile<TFile>);
        Terminal.WriteLine($"IO {typeName}: in-out re-serialization of {taskCount} file{Plural(taskCount)}.");
    }
    public static void InOutFile<TFile>(Options options, OSPath inputFile, OSPath outputFile)
        where TFile : IBinaryFileType, IBinarySerializable, new()
    {
        // Mutate name
        outputFile.SetFileName(outputFile.FileName + "_copy");
        string designator = $"IO {typeof(TFile).Name}";

        // Read in file, write out file
        void fileWrite()
        {
            // In
            TFile source = new();
            source.FileName = inputFile.FileName;
            using EndianBinaryReader reader = new(File.OpenRead(inputFile), source.Endianness);
            reader.Read(ref source);

            // Out
            using EndianBinaryWriter writer = new(File.OpenWrite(outputFile), source.Endianness);
            writer.Write(source);
        }
        var info = new FileWriteInfo()
        {
            InputFilePath = inputFile,
            OutputFilePath = outputFile,
            PrintPrefix = designator,
            PrintActionDescription = "re-serializing file",
        };
        FileWriteOverwriteHandler(options, fileWrite, info);
    }


    public static void PatchSceneComment(Options options)
    {
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        if (hasNoSearchPattern)
            options.SearchPattern = "COLI_COURSE???";

        Terminal.WriteLine($"PATCH: patch scene file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, PatchSceneComment);
        Terminal.WriteLine($"PATCH: patch {taskCount} scene file{Plural(taskCount)}.");
    }
    public static void PatchSceneComment(Options options, OSPath inputFile, OSPath _)
    {
        // Read in file, write out file
        void filePatch()
        {
            using EndianBinaryWriter writer = new(File.OpenWrite(inputFile), Scene.endianness);
            writer.JumpToAddress(0x130);
            writer.WritePadding(0xF0, 0x20);
        }
        var info = new FileWriteInfo()
        {
            InputFilePath = inputFile,
            OutputFilePath = inputFile,
            PrintPrefix = "PATCH",
            PrintActionDescription = "patching scene",
        };
        FileWriteOverwriteHandler(options, filePatch, info);
    }
}
