using GameCube.GFZ.GMA;
using GameCube.GFZ.Stage;
using GameCube.GFZ.TPL;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    // IN/OUT Actions
    public static class ActionsIO
    {
        public static void InOutGMA(Options options) => InOutFiles<Gma>(options, "*.gma");
        public static void InOutTPL(Options options) => InOutFiles<Tpl>(options, "*.tpl");
        public static void InOutScene(Options options) => InOutFiles<Scene>(options, "COLI_COURSE*");


        public static void InOutFiles<TFile>(Options options, string searchPattern)
            where TFile : IBinaryFileType, IBinarySerializable, new()
        {
            // Force checking for .LZ only IF there is no defined search pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = searchPattern;

            string typeName = typeof(TFile).Name;
            Terminal.WriteLine($"IO {typeName}: in-out re-serialization of file(s).");
            int taskCount = DoFileInFileOutTasks(options, InOutFile<TFile>);
            Terminal.WriteLine($"IO {typeName}: in-out re-serialization of {taskCount} file{Plural(taskCount)}.");
        }
        public static void InOutFile<TFile>(Options options, FilePath inputFile, FilePath outputFile)
            where TFile : IBinaryFileType, IBinarySerializable, new()
        {
            // Mutate name
            outputFile.SetName(outputFile.Name + "_copy");
            string designator = $"IO {typeof(TFile).Name}";

            // Read in file, write out file
            void fileWrite()
            {
                // In
                TFile source = new();
                using EndianBinaryReader reader = new(File.OpenRead(inputFile), source.Endianness);
                reader.Read(ref source);

                // Out
                using EndianBinaryWriter writer = new(File.OpenWrite(outputFile), source.Endianness);
                writer.Write(source);
            }
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintDesignator = designator,
                PrintActionDescription = "re-serializing file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
    }
}
