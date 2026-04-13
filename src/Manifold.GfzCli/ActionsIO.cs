using GameCube.GFZ.GMA;
using GameCube.GFZ.Stage;
using GameCube.GFZ.TPL;
using Manifold.IO;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Round-trip IO actions (deserialize file, reserialize file).
/// </summary>
public static class ActionsIO
{
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionIOGma"/>
    /// </remarks>
    public static void InOutGMA(Options options) => InOutFiles<GmaFile>(options, "*.gma");

    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionIOTpl"/>
    /// </remarks>
    public static void InOutTPL(Options options) => InOutFiles<TplFile>(options, "*.tpl");

    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionIOScene"/>
    /// </remarks>
    public static void InOutScene(Options options) => InOutFiles<SceneFile>(options, "COLI_COURSE???");


    public static void InOutFiles<TFile>(Options options, string searchPattern)
        where TFile : IBinaryFileType, IBinarySerializable, new()
    {
        options.OverrideSearchPatternIfUnset(searchPattern);

        string typeName = typeof(TFile).Name;
        Terminal.WriteLine($"IO {typeName}: in-out re-serialization of file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, InOutFile);
        Terminal.WriteLine($"IO {typeName}: in-out re-serialization of {taskCount} file{Plural(taskCount)}.");

        static void InOutFile(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Mutate name
            outputFile.SetFileName(outputFile.FileName + "_copy");

            // Read in file, write out file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
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
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionIOSceneNullComment"/>
    /// </remarks>
    public static void PatchSceneNullComment(Options options)
    {
        options.OverrideSearchPatternIfUnset("COLI_COURSE???");
        Terminal.WriteLine($"PATCH: patch scene file(s).");
        int taskCount = ParallelizeFileInFileOutTasks(options, PatchSceneComment);
        Terminal.WriteLine($"PATCH: patch {taskCount} scene file{Plural(taskCount)}.");

        static void PatchSceneComment(Options options, OSPath inputFile, OSPath _)
        {
            // Read in file, edit
            bool doWriteFile = CheckWillFileWrite(options, inputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, inputFile, options.ActionStr);
            if (doWriteFile)
            {
                using EndianBinaryWriter writer = new(File.OpenWrite(inputFile), SceneFile.endianness);
                writer.JumpToAddress(0x130);
                writer.WritePadding(0xF0, 0x20);
            }
        }
    }

}
