using Manifold.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;
using static Manifold.GFZCLI.GfzCliImageUtilities;
using static Manifold.GFZCLI.Program;
using GameCube.GFZ.LineREL;
using GameCube.GFZ;
using GameCube.GFZ.FMI;

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
            outputFile.SetExtensions(".json");

            // 
            var fileWrite = () =>
            {
                // Read data
                Fmi fmi = new Fmi();
                using EndianBinaryReader reader = new EndianBinaryReader(File.OpenRead(inputFile), Fmi.endianness);
                fmi.Deserialize(reader);

                // write data
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
