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
    public const string SceneSearchPattern = "COLI_COURSE???";
    public const string GmaSearchPattern = "*.gma";

    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionLogStageAll"/>
    /// </remarks>
    public static void LogStageAll(Options options)
    {
        foreach (TableLogger.LogFuncFile<SceneFile> logFuncFile in StageTableLogger.AllLogFunctionFiles)
            Log(options, logFuncFile, SceneSearchPattern);
    }

    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionLogGmaAll"/>
    /// </remarks>
    public static void LogGmaAll(Options options)
    {
        foreach (TableLogger.LogFuncFile<GmaFile> logFuncFile in GmaTableLogger.AllLogFunctionFiles)
            Log(options, logFuncFile, GmaSearchPattern);
    }

    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionLogStageTrackKeyablesAll"/>
    /// </remarks>
    public static void LogStageTrackKeyables(Options options)
        => Log(options, StageTableLogger.LogTrackKeyablesAll, SceneSearchPattern);


    private static void Log<TBinarySerializable>(Options options, TableLogger.LogFuncFile<TBinarySerializable> logFuncFile, string searchPattern = "")
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
            IEnumerable<TBinarySerializable> scenes = BinarySerializableIO.LoadFile<TBinarySerializable>(GetInputFiles(options));
            logFuncFile.AnalysisFunction.Invoke(scenes.ToArray(), outputFile);
        }
    }

}
