namespace Manifold.GfzCli;

using GameCube.GFZ.Asset;
using GameCube.GFZ.GMA;
using GameCube.GFZ.Stage;
using Manifold.IO;
using System.Collections.Generic;
using System.Linq;
using static Manifold.GfzCli.GfzCliUtilities;


public static class ActionsLog
{
    private const string SceneSearchPattern = "COLI_COURSE???";
    private const string GmaSearchPattern = "*.gma";

    public static readonly GfzCliAction ActionLogStage = new()
    {
        Description = "Create all possible analysis .TSVs of COLI_COURSE stage files.",
        Action = LogStageAll,
        ActionID = CliActionID.log_stage_all,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionLogGma = new()
    {
        Description = "Create all possible analysis .TSVs of GMA model files.",
        Action = LogGmaAll,
        ActionID = CliActionID.log_gma_all,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    // TODO: for each one individually
    public static readonly GfzCliAction ActionLogStageTrackKeyables = new()
    {
        Description = "Create a .tsv log of track keyables from COLI_COURSE stage files.",
        Action = (Options options) => Log(options, StageTableLogger.LogTrackKeyablesAll, SceneSearchPattern),
        ActionID = CliActionID.log_stage_track_keyables,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Directory,
        IsOutputOptional = false,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static void LogStageAll(Options options)
    {
        foreach (TableLogger.LogFuncFile<SceneFile> logFuncFile in StageTableLogger.AllLogFunctionFiles)
            Log(options, logFuncFile, SceneSearchPattern);
    }
    public static void LogGmaAll(Options options)
    {
        foreach (TableLogger.LogFuncFile<GmaFile> logFuncFile in GmaTableLogger.AllLogFunctionFiles)
            Log(options, logFuncFile, GmaSearchPattern);
    }

    public static void Log<TBinarySerializable>(Options options, TableLogger.LogFuncFile<TBinarySerializable> logFuncFile, string searchPattern = "")
        where TBinarySerializable : IBinarySerializable, IBinaryFileType, new()
    {
        // Allow search pattern override if requested and unset
        if (!string.IsNullOrWhiteSpace(searchPattern))
            options.OverrideSearchPatternIfUnset(searchPattern);

        // Create output path for analysis
        OSPath outputFile = new(options.OutputPath);
        outputFile.SetFileNameAndExtensions(logFuncFile.FileName);
        if (CanWriteFileAndPrintResult(options, outputFile))
        {
            EnsureDirectoriesExist(outputFile);
            IEnumerable<TBinarySerializable> scenes = BinarySerializableIO.LoadFile<TBinarySerializable>(options.GetInputFiles());
            logFuncFile.AnalysisFunction.Invoke(scenes.ToArray(), outputFile);
        }
    }
}
