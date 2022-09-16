using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Manifold.GFZCLI
{
    public class Options
    {
        private static class Help
        {
            public const string Verbose =
                "Set output to verbose messages.";

            public const string InputPath =
                "Input path to source image file.";

            public const string OutputPath =
                "Output path to source image file.";

            public const string SearchSubdirectories =
                "(true|false) Whether or not to search subdirectories for files when using the directory mode.";

            public const string SearchPattern =
                "The search pattern used to find files.\n\tEx: \"*.png\"";

            public const string RemoveAllExtensions =
                "Removes all extensions from input file. True by default.\n\tEx: \"spr.ci8.png\" -> \"spr.sprite\" ";


            public const string ImportCarDataToTSV =
                "Creates a TSV representing the values of cardata.bin";
            public const string ExportCarDataFromTSV =
                "Creates a TSV representing the values of cardata.bin";
        }


        [Option("verbose", Required = false, HelpText = Help.Verbose)]
        public bool Verbose { get; set; }

        [Option('i', "inputPath", Required = false, HelpText = Help.InputPath)]
        public string InputPath { get; set; } = string.Empty;

        [Option('o', "outputPath", Required = false, HelpText = Help.OutputPath)]
        public string OutputPath { get; set; } = string.Empty;


        [Option('s', "searchSubdirs", Required = false, HelpText = Help.SearchSubdirectories)]
        public bool SearchSubdirectories { get; set; }

        [Option('p', "searchPattern", Required = false, HelpText = Help.SearchPattern)]
        public string SearchPattern { get; set; } = string.Empty;


        [Option("cardata-bin-to-tsv", Required = false, HelpText = Help.ImportCarDataToTSV)]
        public string CarDataBinPath { get; set; } = string.Empty;

        [Option("cardata-tsv-to-bin", Required = false, HelpText = Help.ExportCarDataFromTSV)]
        public string CarDataTsvPath { get; set; } = string.Empty;




        public SearchOption SearchOption => SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;


        public void PrintState()
        {
            Console.WriteLine("Options:");
            Console.WriteLine($"{nameof(Verbose)}: {Verbose}");
            Console.WriteLine($"{nameof(InputPath)}: {InputPath}");
            Console.WriteLine($"{nameof(OutputPath)}: {OutputPath}");
            Console.WriteLine($"{nameof(SearchSubdirectories)}: {SearchSubdirectories}");
            Console.WriteLine($"{nameof(SearchOption)}: {SearchOption}");
            Console.WriteLine($"{nameof(SearchPattern)}: {SearchPattern}");
        }

    }
}