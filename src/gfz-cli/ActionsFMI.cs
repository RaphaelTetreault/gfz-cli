using GameCube.GFZ.FMI;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;
using static Manifold.GFZCLI.GfzCliImageUtilities;
using static Manifold.GFZCLI.Program;
using System;

namespace Manifold.GFZCLI
{
    public static class ActionsFMI
    {
        public static void FmiToJson(Options options)
        {
            // Default search
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*.fmi";

            Terminal.WriteLine("FMI: converting FMI to JSON files.");
            int binCount = DoFileInFileOutTasks(options, FmiToJson);
            Terminal.WriteLine($"FMI: done converting {binCount} file{Plural(binCount)}.");
        }

        private static void FmiToJson(Options options, FilePath inputFile, FilePath outputFile)
        {
            //
            outputFile.SetExtensions(".test.txt");

            // 
            var fileWrite = () =>
            {
                // Read data
                FmiFile fmiFile = new FmiFile();
                using EndianBinaryReader reader = new(File.OpenRead(inputFile), FmiFile.endianness);
                fmiFile.Deserialize(reader);

                // write to file
                using StreamWriter writer = new StreamWriter(outputFile);
                fmiFile.Value.Serialize(writer);
                writer.Flush();
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "FMI",
                PrintActionDescription = $"converting FMI to JSON for",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

    }
}
