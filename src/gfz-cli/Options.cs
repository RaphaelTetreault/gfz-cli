using CommandLine;
using CommandLine.Text;
using GameCube.AmusementVision;
using GameCube.GFZ.Stage;
using System.Text.Json.Serialization;

namespace Manifold.GFZCLI
{
    public class Options :
        ITplOptions
    {
        internal static class ArgsShort
        {
            //public const char Action = 'a';
            //public const char InputPath = 'i';
            //public const char OutputPath = 'o';

            public const char OverwriteFiles = 'o';
            public const char SearchPattern = 'p';
            public const char SearchSubdirectories = 's';
            public const char SerializationFormat = 'f';
        }

        internal static class Sets
        {
            public const string TPL = "tpl";
        }

        internal static class Args
        {
            //public const string Verbose = "verbose";

            public const string Action = "action";
            public const string InputPath = "input-path";
            public const string OutputPath = "output-path";

            public const string OverwriteFiles = "overwrite";
            public const string SearchPattern = "search-pattern";
            public const string SearchSubdirectories = "search-subdirs";
            public const string SerializationFormat = "format";

            public const string TplUnpackMipmaps = "tpl-unpack-mipmaps";
            public const string TplUnpackSaveCorruptedTextures = "tpl-unpack-corrupted-cmpr";
        }

        internal static class Help
        {
            public const string Verbose =
                "Output all messages to console.\n" +
                "\tEnabled only when called.";

            public const string Action =
                "The action to perform. (action: description)\n" +
                "\tcardata-bin-to-tsv: create cardata TSV from input binary.\n" +
                "\tcardata-tsv-to-bin: create cardata binary from input TSV.\n" +
                "\temblem-to-image: convert .gci and .bin emblem files into images.\n" +
                //"\timage-to-emblem: WIP.\n" +
                "\tlive-camera-stage-bin-to-tsv: create livecam_stage TSV from input file.\n" +
                "\tlive-camera-stage-tsv-to-bin: create livecam_stage binary from TSV file.\n" +
                "\tlz-decompress: decompress .lz archive.\n" +
                "\tlz-compress: compress file to .lz archive.\n" +
                "\ttpl-unpack: unpack .tpl archive into folder of it's textures."; //\n" +
            public const string InputPath =
                "The input path to a file or folder for the specified action. Most actions support both.";
            public const string OutputPath =
                "Optional. The output path. Can be a full file path (for single file actions) " +
                "or destination directory (for multi file actions).";

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

            public const string TplUnpackMipmaps =
                "tpl-unpack (option): Export mipmap textures.";
            public const string TplUnpackSaveCorruptedTextures =
                "tpl-unpack (option): Export corrupted CMPR mipmap textures.";
        }

        // VALUES
        [Value(0, MetaName = Args.Action, HelpText = Help.Action, Required = true)]
        //public GfzCliAction Action { get; set; }
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

        [Value(1, MetaName = Args.InputPath, HelpText = Help.InputPath, Required = true)]
        public string InputPath { get; set; } = string.Empty;

        [Value(2, MetaName = Args.OutputPath, HelpText = Help.OutputPath, Required = false)]
        public string OutputPath { get; set; } = string.Empty;


        // GENERAL OPTIONS
        [Option(ArgsShort.OverwriteFiles, Args.OverwriteFiles, HelpText = Help.OverwriteFiles)]
        public bool OverwriteFiles { get; set; }

        [Option(ArgsShort.SearchPattern, Args.SearchPattern, HelpText = Help.SearchPattern)]
        public string SearchPattern { get; set; } = string.Empty;

        [Option(ArgsShort.SearchSubdirectories, Args.SearchSubdirectories, HelpText = Help.SearchSubdirectories)]
        public bool SearchSubdirectories { get; set; }

        [Option(ArgsShort.SerializationFormat, Args.SerializationFormat, HelpText = Help.SerializationFormat)]
        public string SerializationFormat { get; set; } = string.Empty;


        //// TPL OPTIONS
        //[Option(Args.TplUnpackMipmaps, HelpText = Help.TplUnpackMipmaps, SetName = Sets.TPL)]
        //public bool TplUnpackMipmaps { get; set; }

        //[Option(Args.TplUnpackSaveCorruptedTextures, HelpText = Help.TplUnpackSaveCorruptedTextures, SetName = Sets.TPL)]
        //public bool TplUnpackSaveCorruptedTextures { get; set; }

        public SearchOption SearchOption => SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        public SerializeFormat SerializeFormat => GetSerializeFormat(SerializationFormat);
        public AvGame AvGame => GetAvFormat(SerializeFormat);


        // TPL
        public bool TplUnpackMipmaps { get; set; }
        public bool TplUnpackSaveCorruptedTextures { get; set; }


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
        private static AvGame GetAvFormat(SerializeFormat serializeFormat)
        {
            switch (serializeFormat)
            {
                case SerializeFormat.AX: return AvGame.FZeroAX;
                case SerializeFormat.GX: return AvGame.FZeroGX;
                default:
                    string msg = $"No {nameof(SerializationFormat)} \"{serializeFormat}\" defined.";
                    throw new ArgumentException(msg);
            }
        }
    }
}