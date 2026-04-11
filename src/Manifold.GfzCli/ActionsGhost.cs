using GameCube.GFZ.Ghosts;
using Manifold.IO;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for working with ghost GCI save files.
/// </summary>
public class ActionsGhost
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public static void ExtractGhostFromGci(Options options)
    {
        //string[] files = GetInputFiles(options);
        Terminal.WriteLine("Ghost: extracting ghost data.");
        int binCount = ParallelizeFileInFileOutTasks(options, ExtractGhostDataFromGci);
        Terminal.WriteLine($"Ghost: done extracting ghost data from {binCount} file{Plural(binCount)}.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    private static void ExtractGhostDataFromGci(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Copy value over
        GhostDataGCI ghostGci = new();
        GhostDataBIN ghostBin = new();
        using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), GhostDataGCI.endianness))
        {
            ghostGci.Deserialize(reader);
            ghostBin.Value = ghostGci.GhostData;
            ghostBin.FileName = Path.GetFileNameWithoutExtension(inputFile);
        }

        // TODO: parameterize extensions
        outputFile.SetExtensions(GhostDataBIN.extension);

        // Write file
        bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
        PrintFileWriteResult(result, outputFile, options.ActionStr);
        if (doWriteFile)
        {
            ghostBin.WriteFile(outputFile);
        }
    }

}
