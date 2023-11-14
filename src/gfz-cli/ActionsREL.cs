using GameCube.GFZ;
using GameCube.GFZ.REL;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsREL
    {
        public static void DecryptLine__(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*line__.bin";

            DoFileInFileOutTasks(options, DecryptLine);
        }

        public static void EncryptLine__(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*line__.rel";

            DoFileInFileOutTasks(options, EncryptLine);
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

        public static void DecryptLine(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Step 1: Decrypt line__.bin into line__.rel.lz
            CryptEnemyLine(options, inputFile, outputFile, false, "rel.lz");
            
            // Step 2: Get path to line__.rel.lz
            FilePath lzInputFile = new FilePath(outputFile);
            lzInputFile.SetExtensions("rel.lz");
            FilePath lzOutputFile = new FilePath(lzInputFile);

            // Step 3: Decompress line__.rel.lz into line__.rel
            ActionsLZ.LzDecompressFile(options, lzInputFile, lzOutputFile);
        }

        public static void EncryptLine(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Step 1: Compress line__.rel to line__.rel.lz
            ActionsLZ.LzCompressFile(options, inputFile, outputFile);

            // Step 2: Get path to line__.rel.lz
            FilePath lzInputFile = new FilePath(outputFile);
            lzInputFile.PushExtension("lz");
            FilePath lzOutputFile = new FilePath(lzInputFile);

            // Step 3: Encrypt line_rel.lz into line__.bin
            CryptEnemyLine(options, lzInputFile, lzOutputFile, true, "bin");
        }

    }
}
