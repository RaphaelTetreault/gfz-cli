using GameCube.GFZ.Camera;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     
/// </summary>
public static class ActionsCamera
{
    const string prefix = "CAM";

    /// <summary>
    ///     Create TSV from livecam binary.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionCameraLivecamToTSV"/>
    /// </remarks>
    public static void LivecamToTsv(Options options)
    {
        options.OverrideSearchPatternIfUnset("livecam_stage*.bin");
        Terminal.WriteLine($"{prefix}: converting livecam*.bin to TSV spreadsheet.");
        int binCount = ParallelizeFileInFileOutTasks(options, LivecamToTsvIO);
        Terminal.WriteLine($"{prefix}: done converting {binCount} file{Plural(binCount)}.");

        static void LivecamToTsvIO(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Load camera BIN
            LiveCameraStage lcs = new LiveCameraStageFile(inputFile);
            // Write TSV file
            outputFile.SetExtensions(".tsv");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                using var fs = new StreamWriter(File.Create(outputFile));
                lcs.Serialize(fs);
            }
        }
    }

    /// <summary>
    ///     Create livecam BIN file from livecam TSV spreadsheet.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionCameraLivecamFromTSV"/>
    /// </remarks>
    public static void LivecamFromTsv(Options options)
    {
        options.OverrideSearchPatternIfUnset("livecam_stage*.tsv");
        Terminal.WriteLine($"{prefix}: converting livecam.tsv to binary file.");
        int binCount = ParallelizeFileInFileOutTasks(options, LivecamFromTsvIO);
        Terminal.WriteLine($"{prefix}: done converting {binCount} file{Plural(binCount)}.");

        static void LivecamFromTsvIO(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Load camera TSV
            LiveCameraStage lcs = new();
            using var sr = new StreamReader(inputFile);
            lcs.Deserialize(sr);
            // Write BIN file
            outputFile.SetExtensions(".bin");
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {
                var lcsf = new LiveCameraStageFile() { Value = lcs };
                lcsf.WriteFile(outputFile);
            }
        }
    }

}
