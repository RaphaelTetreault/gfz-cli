using CommandLine;
using GameCube.AmusementVision;
using GameCube.GFZ;
using GameCube.GFZ.LZ;
using GameCube.GFZ.Stage;
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


            public const string CarDataToTSV =
                "Creates a TSV spreadsheet from the values of cardata.lz binary.";
            public const string CarDataFromTSV =
                "Creates a binary from the values of cardata.tsv spreadsheet.";
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


        [Option("cardata-bin-to-tsv", Required = false, HelpText = Help.CarDataToTSV)]
        public string CarDataBinPath { get; set; } = string.Empty;

        [Option("cardata-tsv-to-bin", Required = false, HelpText = Help.CarDataFromTSV)]
        public string CarDataTsvPath { get; set; } = string.Empty;


        [Option("format", Required = false, HelpText = Help.CarDataFromTSV)]
        public string SerializationFormat { get; set; } = string.Empty;


        [Option("lzd", Required = false, HelpText = Help.CarDataFromTSV)]
        public string LzDecompressTarget { get; set; } = string.Empty;
        [Option("lzc", Required = false, HelpText = Help.CarDataFromTSV)]
        public string LzCompressTarget { get; set; } = string.Empty;


        public SearchOption SearchOption => SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        public SerializeFormat SerializeFormat => GetSerializeFormat(SerializationFormat);
        public GxGame AvGame => GetAvFormat(SerializeFormat);


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


        private static SerializeFormat GetSerializeFormat(string serializeFormat)
        {
            serializeFormat = serializeFormat.ToLower();
            switch (serializeFormat)
            {
                case "ax":
                    return SerializeFormat.AX;

                case "gx":
                default:
                    return SerializeFormat.GX;
            }
        }
        private static GxGame GetAvFormat(SerializeFormat serializeFormat)
        {
            switch (serializeFormat)
            {
                case SerializeFormat.AX: return GxGame.FZeroAX;
                case SerializeFormat.GX: return GxGame.FZeroGX;
                default: throw new ArgumentException();
            }
        }

    }
}