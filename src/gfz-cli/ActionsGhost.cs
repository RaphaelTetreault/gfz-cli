using GameCube.GFZ.Ghosts;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

public class ActionsGhost
{
    public static void ExtractGhostFromGci(Options options)
    {
        //string[] files = GetInputFiles(options);
        Terminal.WriteLine("Ghost: extracting ghost data.");
        int binCount = DoFileInFileOutTasks(options, ExtractGhostDataFromGci);
        Terminal.WriteLine($"Ghost: done extracting ghost data from {binCount} file{Plural(binCount)}.");
    }

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
