using GameCube.GFZ.LZ;
using System.IO;
using static Manifold.GFZCLI.MultiFileUtility;
using static Manifold.GFZCLI.Program;

namespace Manifold.GFZCLI
{
    public static class ActionsLZ
    {

        public static void LzDecompress(Options options)
        {
            // Force checking for .LZ only IF there is no defined search pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*.lz";

            Terminal.WriteLine($"LZ: decompressing file(s).");
            int taskCount = DoFileInFileOutTasks(options, LzDecompressFile);
            Terminal.WriteLine($"LZ: done decompressing {taskCount} file{S(taskCount)}.");
        }

        public static void LzDecompressFile(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Remove extension
            outputFile.PopExtension();

            // 
            var fileWrite = () =>
            {
                // TODO: add LZ function in library to read from inputFilePath, decompress, save to outputFilePath
                using (var stream = LzUtility.DecompressAvLz(inputFile))
                {
                    using (var writer = File.Create(outputFile))
                    {
                        writer.Write(stream.ToArray());
                    }
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "LZ",
                PrintActionDescription = "decompressing file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        public static void LzCompress(Options options)
        {
            Terminal.WriteLine("LZ: Compressing file(s).");
            int taskCount = DoFileInFileOutTasks(options, LzCompressFile);
            Terminal.WriteLine($"LZ: done compressing {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }

        public static void LzCompressFile(Options options, FilePath inputFile, FilePath outputFile)
        {
            outputFile.AppendExtension(".lz");

            var fileWrite = () =>
            {
                // TODO: add LZ function in library to read from inputFile, compress, save to outputFile
                using (var stream = LzUtility.CompressAvLz(inputFile, options.AvGame))
                {
                    using (var writer = File.Create(outputFile))
                    {
                        writer.Write(stream.ToArray());
                    }
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "LZ",
                PrintActionDescription = "compressing input file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
    }
}
