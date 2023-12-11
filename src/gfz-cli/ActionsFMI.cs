using GameCube.GFZ.FMI;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

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

            Terminal.WriteLine("FMI: converting FMI to plain text files.");
            int binCount = DoFileInFileOutTasks(options, FmiToPlaintext);
            Terminal.WriteLine($"FMI: done converting {binCount} file{Plural(binCount)}.");
        }
        public static void FmiFromPlaintext(Options options)
        {
            // Default search
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = $"*.fmi.txt";

            Terminal.WriteLine("FMI: converting FMI from plain text files.");
            int binCount = DoFileInFileOutTasks(options, FmiFromPlaintext);
            Terminal.WriteLine($"FMI: done converting {binCount} file{Plural(binCount)}.");
        }

        private static void FmiToPlaintext(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Set output extensions
            outputFile.SetExtensions(".fmi.txt");

            var fileWrite = () =>
            {
                // Read data
                FmiFile fmiFile = new FmiFile();
                using EndianBinaryReader reader = new(File.OpenRead(inputFile), FmiFile.endianness);
                fmiFile.Deserialize(reader);

                // Write to file
                using PlainTextWriter writer = new(outputFile);
                fmiFile.Value.Serialize(writer);
                writer.Flush();
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "FMI",
                PrintActionDescription = $"converting FMI binary to plain text using",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        private static void FmiFromPlaintext(Options options, FilePath inputFile, FilePath outputFile)
        {
            // Set output extension
            outputFile.SetExtensions(".fmi");

            var fileWrite = () =>
            {
                // Read data
                FmiFile fmiFile = new();
                using PlainTextReader reader = new(inputFile);
                fmiFile.Value.Deserialize(reader);

                // Write to file
                using EndianBinaryWriter writer = new(File.Create(outputFile), FmiFile.endianness);
                fmiFile.Value.Serialize(writer);
                writer.Flush();
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = "FMI",
                PrintActionDescription = $"converting FMI plain text to binary using",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

    }
}
