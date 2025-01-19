using GameCube.GFZ.Ghosts;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

/// <summary>
///     Actions for working with ghost GCI save files.
/// </summary>
public class ActionsGhost
{
    public static readonly GfzCliAction ActionGciExtractGhost = new()
    {
        Description = "Extract raw ghost data from GCI save file.",
        Action = ExtractGhostFromGci,
        ActionID = CliActionID.gci_extract_ghost,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

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
        // 
        var gci = new GhostDataGCI();
        GhostData ghost;
        using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), GhostDataGCI.endianness))
        {
            gci.Deserialize(reader);
            ghost = gci.GhostData;
            ghost.FileName = Path.GetFileNameWithoutExtension(inputFile);
        }

        // TODO: parameterize extensions
        outputFile.SetExtensions(GhostData.fileExtension);
        var fileWrite = () =>
        {
            using var writer = new EndianBinaryWriter(File.Create(outputFile), GhostData.endianness);
            writer.Write(ghost);
        };
        var info = new FileWriteInfo()
        {
            InputFilePath = inputFile,
            OutputFilePath = outputFile,
            PrintPrefix = "GHOST",
            PrintActionDescription = "writing file",
        };
        FileWriteOverwriteHandler(options, fileWrite, info);
    }

}
