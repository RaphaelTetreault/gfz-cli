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
        public static void FmiToPlainText(Options options)
        {
            // Default search
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*.fmi";

            Terminal.WriteLine("FMI: converting FMI to JSON files.");
            int binCount = DoFileInFileOutTasks(options, FmiToJson);
            Terminal.WriteLine($"FMI: done converting {binCount} file{Plural(binCount)}.");
        }
        public static void FmiFromPlaintext(Options options)
        {
            // Default search
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*rainp.fmi.txt";

            Terminal.WriteLine("FMI: converting FMI from plain text files.");
            int binCount = DoFileInFileOutTasks(options, FmiFromPlaintext);
            Terminal.WriteLine($"FMI: done converting {binCount} file{Plural(binCount)}.");
        }

        private static void FmiToJson(Options options, FilePath inputFile, FilePath outputFile)
        {
            //
            outputFile.SetExtensions(".fmi.txt");

            // 
            var fileWrite = () =>
            {
                // Read data
                FmiFile fmiFile = new FmiFile();
                using EndianBinaryReader reader = new(File.OpenRead(inputFile), FmiFile.endianness);
                fmiFile.Deserialize(reader);

                // write to file
                using PlainTextWriter writer = new(outputFile);
                fmiFile.Value.Serialize(writer);
                writer.Flush();
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "FMI",
                PrintActionDescription = $"converting FMI binary to plain text for",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        private static void FmiFromPlaintext(Options options, FilePath inputFile, FilePath outputFile)
        {
            //
            //outputFile.SetExtensions(".fmi");

            // 
            var fileWrite = () =>
            {
                // Read data
                FmiFile fmiFile = new FmiFile();
                using PlainTextReader reader = new(File.OpenRead(inputFile));
                fmiFile.Value.Deserialize(reader);

                // write to file
                //using StreamWriter writer = new StreamWriter(outputFile);
                //fmiFile.Value.Serialize(writer);
                //writer.Flush();
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "FMI",
                PrintActionDescription = $"converting FMI plain text to binary for",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

    }
}
