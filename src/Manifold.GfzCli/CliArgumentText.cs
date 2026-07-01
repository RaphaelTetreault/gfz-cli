namespace Manifold.GfzCli;

public static class CliArgumentText
{
    // Required
    public const string Action = "action";
    public const string InputPath = "input-path";
    public const string OutputPath = "output-path";
    // Default
    public const string GameCode = "game";
    public const string OverwriteFiles = "overwrite";
    public const string SearchPattern = "search-pattern";
    public const string SearchSubdirectories = "search-subdirs";
    public const string GameFileFormat = "format";
    public const string Region = "region";
    //public const string Verbose = "verbose";

    // General
    public const string Backup = "backup";
    public const string Name = "name";
    public const string Value = "value";

    // Assets
    public const string AssetLibraryRoot = "asset-library";
    public const string DirFormat = "dir-format";
    public const string MipmapCount = "mipmap-count";
    public const string MipmapFiles = "mipmap-files";
    public const string MipmapMode = "mipmap-mode";
    public const string TextureFormat = "texture-format";

    // IMAGE SHARP
    // Resize
    public const string Compand = "compand";
    public const string PadColor = "pad-color";
    public const string Position = "position";
    public const string PremultiplyAlpha = "premultiply-alpha";
    public const string Resampler = "resampler";
    public const string ResizeMode = "resize-mode";
    public const string Width = "width";
    public const string Height = "height";
    // Other
    public const string ImageFormat = "image-format"; // six labors

    // Stage
    //public const string Color = "color";
    //public const string ColorR = "color-r";
    //public const string ColorG = "color-g";
    //public const string ColorB = "color-b";
    //public const string ColorA = "color-a";
    public const string FogColor = "fog-color";
    public const string FogInterpolationMode = "fog-interpolation-mode";
    public const string FogViewRangeNear = "fog-view-range-near";
    public const string FogViewRangeFar = "fog-view-range-far";
    public const string SetFlagsOff = "set-flags-off";

    // REL
    public const string BgmIndex = "bgm";
    public const string BgmFinalLapIndex = "bgmfl";
    public const string CourseIndex = "course";
    public const string Cup = "cup";
    public const string CupCourseIndex = "cup-course";
    public const string Difficulty = "difficulty";
    public const string PilotNumber = "pilot";
    public const string VenueIndex = "venue";

    // Emblem
    public const string EmblemHasAlphaBorder = "emblem-border";

    public static class Short
    {
        public const char GameCode = 'g';
        public const char OverwriteFiles = 'o';
        public const char SearchPattern = 'p';
        public const char SearchSubdirectories = 's';
        public const char GameFileFormat = 'f';
        public const char Region = 'r';
    }

    public static class Help
    {
        public const string Action =
            "The action to perform.\n" +
            "Call \"list\" for a simple list of actions.\n" +
            "Call \"usage\" for a detailed list of actions.\n" +
            "Call \"usage [action]\" for specific action details.";
        public const string InputPath =
            "The input path to a file or folder for the specified action.\n" +
            "Most actions support both.";
        public const string OutputPath =
            "Optional. The output path. Can be a full file path (for single file actions)\n" +
            "or destination directory (for multi file actions).";

        public const string GameCode =
            "Which game's files are being managed.\n" +
            "Options: \"gfzj01\", \"gfze01\", \"gfzp01\", \"ggge6e\".\n" +
            "Set to \"gfzj01\" by default.";
        public const string OverwriteFiles =
            "Allow output files to overwrite existing files.\n" +
            "Enabled only when called.";
        public const string SearchPattern =
            "The search pattern used to find files.\n" +
            "Ex: \"*.tpl.lz\" (find all compressed TPL files in any directory, if permitted.)\n" +
            "Ex: \"st??.gma\" (find GMA files with 2 digit stage index in same directory.)";
        public const string SearchSubdirectories =
            "Whether or not to search subdirectories for files when using the directory mode.\n" +
            "Enabled only when called.";
        public const string GameFileFormat =
            "The game file format used when serializing.\n" +
            "Options: \"ax\", \"gx\".\n" +
            "Set to \"gx\" by default.";
        public const string Region =
            "The region used when serializing.\n" +
            "Options: \"j\" (jp), \"e\" (na), \"p\" (eu).\n" +
            "Set to \"p\" by default.";
        //public const string Verbose =
        //    "Output all messages to console.\n" +
        //    "\tEnabled only when called.";
    }

    public static class SearchPatterns
    {
        public const string Anything = "*";
        public const string ARC = "*.arc";
        public const string Colicourse = "COLI_COURSE???";
        public const string FMI = "*.fmi";
        public const string FmiPlaintext = "*.fmi.txt";
        public const string GciGameSave = "f_zero.dat.gci";
        public const string GciEmblem = "*fze*.gci";
        public const string GciReplay = "*fzr*.gci";
        public const string GciGarage = "*fzc*.gci";
        public const string GciGhost = "*fzg*.gci";
        public const string GMA = "*.gma";
        public const string GMAREF = $"*.{GameCube.GFZ.Asset.GmaRef.Extension}";
        public const string ImagePNG = "*.png";
        public const string LivecamStage = "livecam_stage*.bin";
        public const string LineBIN = "*line__.bin";
        public const string LineREL = "*line__.rel";
        public const string TPL = "*.tpl";
        public const string TPLREF = $"*.{GameCube.GFZ.Asset.TplRef.Extension}";
    }
}