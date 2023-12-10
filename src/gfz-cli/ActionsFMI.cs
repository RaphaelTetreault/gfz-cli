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
            outputFile.SetExtensions(".fmi.fmi");

            // 
            var fileWrite = () =>
            {
                // Read data
                FmiFile fmiFile = new();
                using PlainTextReader reader = new(inputFile);
                fmiFile.Value.Deserialize(reader);

                foreach (var line in reader.Lines)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine();

                Console.WriteLine("Emitters");
                foreach (var emitter in fmiFile.Value.Emitters)
                {
                    Console.WriteLine(emitter.Position);
                    Console.WriteLine(emitter.TargetOffset);
                    Console.WriteLine(emitter.Scale);
                    Console.WriteLine(emitter.AccelColor);
                    Console.WriteLine(emitter.BoostColor);
                }
                Console.WriteLine("Positions");
                for (int i = 0; i < fmiFile.Value.Positions.Length; i++)
                {
                    Console.WriteLine(fmiFile.Value.Names[i]);
                    var pos = fmiFile.Value.Positions[i];
                    Console.WriteLine(pos.Position);
                    Console.WriteLine(pos.PositionType);
                }
                Console.WriteLine();

                //// write to file
                //using EndianBinaryWriter writer = new(File.Create(outputFile), FmiFile.endianness);
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
