using GameCube.GCI;
using GameCube.GFZ.Replay;
using Manifold.IO;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

public class ActionsGCI
{
    public static void RenameGCI(Options options)
    {
        Terminal.WriteLine($"{options.ActionStr}: converting emblems from BIN files.");
        int fileCount = ParallelizeFileInFileOutTasks(options, RenameGciFile);
        Terminal.WriteLine($"{options.ActionStr}: done renaming {fileCount} file{Plural(fileCount)}.");

        static void RenameGciFile(Options options, OSPath inputFile, OSPath outputFile)
        {
            inputFile.ThrowIfFileDoesNotExist();

            // TODO: Used to be able to use generic GCI type, cannot anymore because
            //       typing is generic. Maybe this func can be generic???
            ReplayGCI gci = new();
            // TODO: would really benefit from helper func like with binaryfile wrapper, new(input)
            //       to read and then something to save file
            using var reader = new EndianBinaryReader(File.OpenRead(inputFile), ReplayGCI.endianness);
            gci.Deserialize(reader);
            reader.JumpToZero();

            // Get file name...?
            string name = GetName(gci.UniqueID, reader);
            outputFile.SetFileName(name);

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
            PrintFileWriteResult(result, outputFile, options.ActionStr);
            if (doWriteFile)
            {

                Terminal.WriteLine($"Renaming {inputFile} to {outputFile}.");
            }
        }
    }

    public static string GetName(ushort uniqueID, EndianBinaryReader reader)
    {
        switch (uniqueID)
        {
            case ReplayGCI.UID:
                var replay = new ReplayGCI();
                replay.Deserialize(reader);
                return "replay";

            default:
                return "UNHANDLED";
        }
    }

}
