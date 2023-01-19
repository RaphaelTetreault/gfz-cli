using CommandLine;
using GameCube.AmusementVision;
using GameCube.GFZ.Stage;

namespace Manifold.GFZCLI
{
    public class Options
    {
        internal static class Args
        {

            public const string OverwriteFiles      = "overwrite";
            public const string Verbose             = "verbose";
            public const string InputPath           = "input-path";
            public const string OutputPath          = "output-path";
            public const string SearchSubdirectories = "search-subdirs";
            public const string SearchPattern       = "search-pattern";
            public const string CarDataBinPath      = "cardata-bin-to-tsv";
            public const string CarDataTsvPath      = "cardata-tsv-to-bin";
            public const string SerializationFormat = "format";
            public const string LzDecompressTarget  = "lzd";
            public const string LzCompressTarget    = "lzc";
            public const string TplUnpack           = "tpl-unpack";
            public const string TplUnpackSaveCorruptedTextures = "tpl-unpack-corrupted-cmpr";
            public const string TplUnpackMipmaps    = "tpl-unpack-mipmaps";
            public const string TplPack             = "tpl-pack";
        }

        internal static class Help
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


            public const string Format =
                "The format used when serializing. Either AX or GX.";
            public const string LzDecompressTarget =
                "File or directory to decompress.";
            public const string LzCompressTarget =
                "File or directory to compress.";


            public const string TplUnpack = "TPL Unpack";
            public const string TplPack = "TPL Pack";
        }


        [Option(Args.OverwriteFiles, Required = false, HelpText = "TODO")]
        public bool OverwriteFiles { get; set; }

        [Option(Args.Verbose, Required = false, HelpText = Help.Verbose)]
        public bool Verbose { get; set; }

        [Option('i', Args.InputPath, Required = false, HelpText = Help.InputPath)]
        public string InputPath { get; set; } = string.Empty;

        [Option('o', Args.OutputPath, Required = false, HelpText = Help.OutputPath)]
        public string OutputPath { get; set; } = string.Empty;


        [Option('s', Args.SearchSubdirectories, Required = false, HelpText = Help.SearchSubdirectories)]
        public bool SearchSubdirectories { get; set; }

        [Option('p', Args.SearchPattern, Required = false, HelpText = Help.SearchPattern)]
        public string SearchPattern { get; set; } = string.Empty;


        [Option(Args.CarDataBinPath, Required = false, HelpText = Help.CarDataToTSV)]
        public string CarDataBinPath { get; set; } = string.Empty;

        [Option(Args.CarDataTsvPath, Required = false, HelpText = Help.CarDataFromTSV)]
        public string CarDataTsvPath { get; set; } = string.Empty;


        [Option(Args.SerializationFormat, Required = false, HelpText = Help.CarDataFromTSV)]
        public string SerializationFormat { get; set; } = string.Empty;


        [Option(Args.LzDecompressTarget, Required = false, HelpText = Help.CarDataFromTSV)]
        public string LzDecompressTarget { get; set; } = string.Empty;
        [Option(Args.LzCompressTarget, Required = false, HelpText = Help.CarDataFromTSV)]
        public string LzCompressTarget { get; set; } = string.Empty;


        [Option(Args.TplUnpack, Required = false, HelpText = Help.TplUnpack)]
        public string TplUnpack { get; set; } = string.Empty;

        [Option(Args.TplUnpackSaveCorruptedTextures, Required = false, HelpText = "TODO")]
        public bool TplUnpackSaveCorruptedTextures { get; set; }

        [Option(Args.TplUnpackMipmaps, Required = false, HelpText = "TODO")]
        public bool TplUnpackMipmaps { get; set; }

        [Option(Args.TplPack, Required = false, HelpText = Help.TplPack)]
        public string TplPack { get; set; } = string.Empty;

        // TODO: implement/parse image-sharp enum for texture output types (use n64-mksprite impl.)


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