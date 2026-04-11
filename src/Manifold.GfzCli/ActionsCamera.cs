using GameCube.GFZ.Camera;
using Manifold.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Manifold.GfzCli.GfzCliUtilities;
using static Manifold.GfzCli.GfzCliImageUtilities;

namespace Manifold.GfzCli;

public static class ActionsCamera
{
    const string prefix = "CAM";

    /// <summary>
    ///     Create TSV from livecam binary.
    /// </summary>
    /// <param name="options"></param>
    public static void LivecamToTsv(Options options)
    {
        options.OverrideSearchPatternIfUnset("livecam_stage*.bin");
        Terminal.WriteLine($"{prefix}: converting livecam*.bin to TSV spreadsheet.");
        int binCount = ParallelizeFileInFileOutTasks(options, LivecamToTsv);
        Terminal.WriteLine($"{prefix}: done converting {binCount} file{Plural(binCount)}.");
    }

    /// <summary>
    ///     Create livecam BIN file from livecam TSV spreadsheet.
    /// </summary>
    /// <param name="options"></param>
    public static void LivecamFromTsv(Options options)
    {
        options.OverrideSearchPatternIfUnset("livecam_stage*.tsv");
        Terminal.WriteLine($"{prefix}: converting livecam.tsv to binary file.");
        int binCount = ParallelizeFileInFileOutTasks(options, LivecamFromTsv);
        Terminal.WriteLine($"{prefix}: done converting {binCount} file{Plural(binCount)}.");
    }

    /// <summary>
    ///     Create a TSV from CarData binary (compressed or uncompressed).
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    public static void LivecamToTsv(Options options, OSPath inputFile, OSPath outputFile)
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

    /// <summary>
    ///     Create a CarData.lz file from CarData TSV spreadsheet.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    public static void LivecamFromTsv(Options options, OSPath inputFile, OSPath outputFile)
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
