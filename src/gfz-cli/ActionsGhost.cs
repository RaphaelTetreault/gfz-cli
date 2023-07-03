using GameCube.GFZ.Ghosts;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public class ActionsGhost
    {
        public static void ReadGhosts(Options options)
        {
            //string[] files = GetInputFiles(options);
            Terminal.WriteLine("Ghost: reading ghost data.");
            int binCount = DoFileInFileOutTasks(options, ReadGhost);
            Terminal.WriteLine($"Ghost: done reading {binCount} file{Plural(binCount)}.");
        }

        private static void ReadGhost(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Read BIN Emblem data
            var ghost = new GhostData();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), GhostData.endianness))
            {
                ghost.Deserialize(reader);
                ghost.FileName = Path.GetFileNameWithoutExtension(inputFile);
            }

            // TODO: parameterize extensions
            outputFile.AppendExtension(".bin");
            var fileWrite = () =>
            {
                using var writer = new EndianBinaryWriter(File.Create(outputFile), GhostData.endianness);
                writer.Write(ghost);
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "GHOST",
                PrintActionDescription = "writing file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

    }
}
