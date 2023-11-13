using GameCube.GFZ;
using GameCube.GFZ.LZ;
using GameCube.GFZ.REL;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsREL
    {
        public static void DecryptEnemyLine__(Options options)
        {
            // 
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*line__.bin";

            DoFileInFileOutTasks(options, DecryptEnemyLine);
        }
        public static void EncryptEnemyLine__(Options options)
        {
            // 
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*line__.rel";

            DoFileInFileOutTasks(options, EncryptEnemyLine);
        }

        public static void CryptEnemyLine(Options options, FilePath inputFile, FilePath outputFile, bool doEncrypt, string extension)
        {
            // Remove extension
            outputFile.PopExtension();
            outputFile.SetExtensions(extension);

            // 
            var fileWrite = () =>
            {
                GameCode gameCode = options.GetGameCode();
                var lookup = EnemyLineLookup.GetInfo(gameCode);
                using var stream = EnemyLineUtility.Crypt(inputFile, lookup);
                using var writer = File.Create(outputFile);
                writer.Write(stream.ToArray());
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "REL",
                PrintActionDescription = doEncrypt ? "encrypting file" : "decrypting file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
        public static void DecryptEnemyLine(Options options, FilePath inputFile, FilePath outputFile)
            => CryptEnemyLine(options, inputFile, outputFile, false, "rel2");
        public static void EncryptEnemyLine(Options options, FilePath inputFile, FilePath outputFile)
            => CryptEnemyLine(options, inputFile, outputFile, true, "bin2");

    }
}
