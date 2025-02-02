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
    public static readonly GfzCliAction ActionIOGma = new()
    {
        Description = "Round-trip serialize GMA files.",
        Action = InOutGMA,
        ActionID = CliActionID.io_gma,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionIOTpl = new()
    {
        Description = "Round-trip serialize TPL files.",
        Action = InOutTPL,
        ActionID = CliActionID.io_tpl,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionIOScene = new()
    {
        Description = "Round-trip serialize COLI_COURSE (scene) files.",
        Action = InOutScene,
        ActionID = CliActionID.io_scene,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPRS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    // TODO: probably belongs in ActionsColiCourse
    public static readonly GfzCliAction ActionIOScenePatch = new()
    {
        Description = "Patch COLI_COURSE (scene) auto-generate timestamp comment to help diff-ing.",
        Action = InOutScene,
        ActionID = CliActionID.io_scene_patch,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.PS,
        RequiredArguments = [],
        OptionalArguments = [],
    };


    public static void InOutGMA(Options options) => InOutFiles<Gma>(options, "*.gma");
    public static void InOutTPL(Options options) => InOutFiles<TplFile>(options, "*.tpl");
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
        // Read in file, edit
        bool doWriteFile = CheckWillFileWrite(options, inputFile, out ActionTaskResult result);
        PrintFileWriteResult(result, inputFile, options.ActionStr);
        if (doWriteFile)
        {
            using EndianBinaryWriter writer = new(File.OpenWrite(inputFile), Scene.endianness);
            writer.JumpToAddress(0x130);
            writer.WritePadding(0xF0, 0x20);
        }
    }
}
