namespace Manifold.GFZCLI;

using GameCube.GFZ.Stage;
using Manifold.IO;
using System.Collections.Generic;
using System.Linq;
using static Manifold.GFZCLI.GfzCliUtilities;


public static class ActionsLog
{
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
    public static readonly GfzCliAction ActionLogStageTrackKeyables = new()
    {
        Description = "Create a .tsv log of track keyables from COLI_COURSE stage files.",
        Action = (Options options) => Log(options, StageTableLogger.LogTrackKeyablesAll),
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
        foreach (TableLogger.LogFuncFile<Scene> logFuncFile in StageTableLogger.AllLogFunctionFiles)
            Log(options, logFuncFile);
    }



    public static void Log<TBinarySerializable>(Options options, TableLogger.LogFuncFile<TBinarySerializable> logFuncFile)
        where TBinarySerializable : IBinarySerializable, IBinaryFileType, new()
    {
        options.OverrideSearchPatternIfUnset("COLI_COURSE???");
        OSPath outputFile = new(options.OutputPath);
        outputFile.SetFileNameAndExtensions(logFuncFile.FileName);
        if (CanWriteFileAndPrintResult(options, outputFile))
        {
            IEnumerable<TBinarySerializable> scenes = BinarySerializableIO.LoadFile<TBinarySerializable>(options.GetInputFiles());
            logFuncFile.AnalysisFunction.Invoke(scenes.ToArray(), outputFile);
        }
    }


}
