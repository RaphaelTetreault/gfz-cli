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

            outputFile.AppendExtension(".bin");
            var fileWrite = () =>
            {
                // 0: ghost does not appear. Can win if time is better than timestamp in file.
                // 1: timestamp seconds? When behind, estimate is borked AF.
                // UNKNOWN
                //ghost.Mutate(0, 0); // interpolation mode? 1-4
                //ghost.Mutate(1, 0); // UNK lap data. Seems almost like "which lap", but 02 not on final dat

                // ROTATION
                //ghost.Mutate(2, 0);
                //ghost.Mutate(3, 0);
                //ghost.Mutate(4, 0);
                //ghost.Mutate(5, 0);
                //ghost.Mutate(6, 0);
                //ghost.Mutate(7, 0);

                // POSITION
                //  8+ 9: pos x
                // 10+11: pos y
                // 12+13: pos z
                //ghost.Mutate(8, 0);
                //ghost.Mutate(9, 0);
                //ghost.Mutate(10, 0);
                //ghost.Mutate(11, 0);
                //ghost.Mutate(12, 0);
                //ghost.Mutate(13, 0);
                
                // INTERPOLATION - maybe fixed point duration?
                //ghost.Mutate(14, 0);
                //ghost.Mutate(15, 0);

                using (var writer = new EndianBinaryWriter(File.Create(outputFile), GhostData.endianness))
                {
                    writer.Write(ghost);
                }
                File.Copy(outputFile, "D:\\boot-gfzj\\files\\staff_ghost\\" + outputFile.Name + ".dat", true);
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
