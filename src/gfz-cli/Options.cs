using CommandLine;
using GameCube.AmusementVision;
using GameCube.GFZ.Stage;

namespace Manifold.GFZCLI
{
    public class Options
    {
        //internal static class ArgsShort
        //{
        //    public const char SearchPattern = 'p';
        //    public const char SearchSubdirectories = 's';
        //}

        internal static class Args
        {
            public const string Verbose = "verbose";
            public const string OverwriteFiles = "overwrite";
            public const string SearchPattern = "search-pattern";
            public const string SearchSubdirectories = "search-subdirs";
            public const string SerializationFormat = "format";

            public const string LzDecompressTarget = "lzd";
            public const string LzCompressTarget = "lzc";

            public const string TplUnpack = "tpl-unpack";
            public const string TplUnpackMipmaps = "tpl-unpack-mipmaps";
            public const string TplUnpackSaveCorruptedTextures = "tpl-unpack-corrupted-cmpr";
            public const string TplPack = "tpl-pack";

            public const string CarDataBinPath = "cardata-bin-to-tsv";
            public const string CarDataTsvPath = "cardata-tsv-to-bin";

            public const string LiveCameraStageBinToTsvPath = "live-camera-stage-bin-to-tsv";
            public const string LiveCameraStageTsvToBinPath = "live-camera-stage-tsv-to-bin";
        }

        internal static class Help
        {
            public const string Verbose =
                "Output all messages to console.\n" +
                "\tEnabled only when called.";

            public const string OverwriteFiles =
                "Allow output files to overwrite existing files.\n" +
                "\tEnabled only when called.";

            public const string SearchPattern =
                "The search pattern used to find files.\n" +
                "\tEx: \"*.tpl.lz\" (find all compressed TPL files in any directory, if permitted.)\n" +
                "\tEx: \"st??.gma\" (find GMA files with 2 digit stage index in same directory.)";
            public const string SearchSubdirectories =
                "Whether or not to search subdirectories for files when using the directory mode.\n" +
                "\tEnabled only when called.";

            public const string SerializationFormat =
                "The format used when serializing.\n" +
                "\tOptions: \"ax\", \"gx\". Set to \"gx\" by default.";


            public const string LzDecompressTarget =
                "The target path to decompress. Can be file or directory.";
            public const string LzCompressTarget =
                "The target path to compress. Can be file or directory\n" +
                "\tUse --" + Args.SerializationFormat + " to set output format.";


            public const string TplUnpack =
                "Creates a folder containing the image contents of target .tpl file(s).\n" +
                "\tInput path can be a file or directory.\n" +
                "\tOutput folder is created in the same directory as the file.";
            public const string TplPack =
                "TODO";
            public const string TplUnpackMipmaps =
                "Export mipmap textures.\n" +
                "\t--" + Args.TplUnpack + " must be called.";
            public const string TplUnpackSaveCorruptedTextures =
                "Export corrupted CMPR mipmap textures.\n" +
                "\t--" + Args.TplUnpack + " must be called.\n" +
                "\t--" + Args.TplUnpackMipmaps + " must be called.";


            public const string CarDataToTSV =
                "Creates a TSV spreadsheet from the values of cardata.lz binary.\n" +
                "\tApplies only to F-Zero GX. File location: ./game/cardata.lz";
            public const string CarDataFromTSV =
                "Creates a binary from the values of a cardata TSV spreadsheet.";
        }

        [Option('a', "action", HelpText = "TODO", Required = true)]
        public string ActionStr { get; set; } = string.Empty;
        public GfzCliAction Action
        {
            get
            {
                string sanitized = ActionStr.Replace('-', '_');
                Enum.TryParse(sanitized, true, out GfzCliAction action);
                return action;
            }
        }


        [Option('i', "input-path", HelpText = "TODO", Required = true)]
        public string InputPath { get; set; } = string.Empty;

        [Option('o', "output-path", HelpText = "TODO")]
        public string OutputPath { get; set; } = string.Empty;


        [Option(Args.Verbose, HelpText = Help.Verbose)]
        public bool Verbose { get; set; }

        [Option(Args.OverwriteFiles, HelpText = Help.OverwriteFiles)]
        public bool OverwriteFiles { get; set; }

        [Option(Args.SearchPattern, HelpText = Help.SearchPattern)]
        public string SearchPattern { get; set; } = string.Empty;

        [Option(Args.SearchSubdirectories, HelpText = Help.SearchSubdirectories)]
        public bool SearchSubdirectories { get; set; }

        [Option(Args.SerializationFormat, HelpText = Help.SerializationFormat)]
        public string SerializationFormat { get; set; } = string.Empty;


        // TPL
        [Option(Args.TplUnpack, HelpText = Help.TplUnpack)]
        public string TplUnpack { get; set; } = string.Empty;

        [Option(Args.TplUnpackMipmaps, HelpText = Help.TplUnpackMipmaps)]
        public bool TplUnpackMipmaps { get; set; }

        [Option(Args.TplUnpackSaveCorruptedTextures, HelpText = Help.TplUnpackSaveCorruptedTextures)]
        public bool TplUnpackSaveCorruptedTextures { get; set; }

        // TEMP: disable PACK for release
        //[Option(Args.TplPack,  HelpText = Help.TplPack)]
        public string TplPack { get; set; } = string.Empty;
        // TODO: implement/parse image-sharp enum for texture output types (use n64-mksprite impl.)


        // CARDATA
        [Option(Args.CarDataBinPath, HelpText = Help.CarDataToTSV)]
        public string CarDataBinPath { get; set; } = string.Empty;

        [Option(Args.CarDataTsvPath, HelpText = Help.CarDataFromTSV)]
        public string CarDataTsvPath { get; set; } = string.Empty;


        // LIVE CAMERA STAGE
        [Option(Args.LiveCameraStageBinToTsvPath, HelpText = "TODO")]
        public string LiveCameraStageBinToTsvPath { get; set; } = string.Empty;

        [Option(Args.LiveCameraStageTsvToBinPath, HelpText = "TODO")]
        public string LiveCameraStageTsvToBinPath { get; set; } = string.Empty;


        public SearchOption SearchOption => SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        public SerializeFormat SerializeFormat => GetSerializeFormat(SerializationFormat);
        public GxGame AvGame => GetAvFormat(SerializeFormat);


        public void PrintState()
        {
            //Console.WriteLine("Options:");
            //Console.WriteLine($"{nameof(Verbose)}: {Verbose}");
            //Console.WriteLine($"{nameof(InputPath)}: {InputPath}");
            //Console.WriteLine($"{nameof(OutputPath)}: {OutputPath}");
            //Console.WriteLine($"{nameof(SearchSubdirectories)}: {SearchSubdirectories}");
            //Console.WriteLine($"{nameof(SearchOption)}: {SearchOption}");
            //Console.WriteLine($"{nameof(SearchPattern)}: {SearchPattern}");
        }

        private static SerializeFormat GetSerializeFormat(string serializeFormat)
        {
            serializeFormat = serializeFormat.ToLower();
            switch (serializeFormat)
            {
                case "ax":
                    return SerializeFormat.AX;

                case "gx":
                case "":
                    return SerializeFormat.GX;

                default:
                    string msg = $"No {nameof(SerializationFormat)} matches input \"{serializeFormat}\"";
                    throw new ArgumentException(msg);
            }
        }
        private static GxGame GetAvFormat(SerializeFormat serializeFormat)
        {
            switch (serializeFormat)
            {
                case SerializeFormat.AX: return GxGame.FZeroAX;
                case SerializeFormat.GX: return GxGame.FZeroGX;
                default:
                    string msg = $"No {nameof(SerializationFormat)} \"{serializeFormat}\" defined.";
                    throw new ArgumentException(msg);
            }
        }


        //public bool IsAction(GfzCliAction action)
        //{
        //    bool isAction = Action == action;
        //    return isAction;
        //}

        public bool IsNotAction(GfzCliAction action)
        {
            bool isAction = Action != action;
            return isAction;
        }

        //public bool IsNotAction(params GfzCliAction[] actions)
        //{
        //    foreach (var action in actions)
        //    {
        //        bool isAction = IsAction(action);
        //        if (isAction)
        //            return false;
        //    }

        //    return true;
        //}

    }
}