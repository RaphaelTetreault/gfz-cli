using CommandLine;
using GameCube.DiskImage;
using GameCube.GFZ.Stage;
using System.IO;

namespace Manifold.GFZCLI;

public interface IOptionsGfzCli
{
    internal static class ArgsShort
    {
        public const char OverwriteFiles = 'o';
        public const char SearchPattern = 'p';
        public const char SearchSubdirectories = 's';
        public const char SerializationFormat = 'f';
        public const char SerializationRegion = 'r';
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
        public const string SerializationRegion = "region";
    }

    internal static class Help
    {
        //public const string Verbose =
        //    "Output all messages to console.\n" +
        //    "\tEnabled only when called.";
        public const string Action =
            "The action to perform.\n" +
            "Call \"usage\" for a complete list of actions.\n" +
            "Call \"usage [action]\" for specific action usage.";
        public const string InputPath =
            "The input path to a file or folder for the specified action.\n" +
            "Most actions support both.";
        public const string OutputPath =
            "Optional. The output path. Can be a full file path (for single file actions)\n" +
            "or destination directory (for multi file actions).";

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
        public const string SerializationFormat =
            "The format used when serializing.\n" +
            "Options: \"ax\", \"gx\". Set to \"gx\" by default.";
        public const string SerializationRegion =
            "The region used when serializing.\n" +
            "Options: \"J\" (JP), \"E\" (NA), \"P\" (EU). Set to \"J\" by default.";
    }

    /// <summary>
    ///     Input string for enum.
    ///     GFZ CLI action to perform.
    /// </summary>
    [Value(0, MetaName = Args.Action, HelpText = Help.Action, Required = true)]
    public string ActionStr { get; set; }

    /// <summary>
    ///     GFZ CLI action to perform.
    /// </summary>
    public CliActionID Action { get; }

    /// <summary>
    ///     Input path for action.
    /// </summary>
    [Value(1, MetaName = Args.InputPath, HelpText = Help.InputPath, Required = false)]
    public string InputPath { get; set;  }

    /// <summary>
    ///     Output path for action.
    /// </summary>
    [Value(2, MetaName = Args.OutputPath, HelpText = Help.OutputPath, Required = false)]
    public string OutputPath { get; set; }

    /// <summary>
    ///     Whether overwriting files is allowed.
    /// </summary>
    [Option(ArgsShort.OverwriteFiles, Args.OverwriteFiles, HelpText = Help.OverwriteFiles)]
    public bool OverwriteFiles { get; set; }

    /// <summary>
    ///     File search pattern. Uses * and ? wildcards.
    /// </summary>
    [Option(ArgsShort.SearchPattern, Args.SearchPattern, HelpText = Help.SearchPattern)]
    public string SearchPattern { get; set; }

    /// <summary>
    ///     Input string for enum.
    ///     Whether search pattern applies to files in subfolders.
    /// </summary>
    [Option(ArgsShort.SearchSubdirectories, Args.SearchSubdirectories, HelpText = Help.SearchSubdirectories)]
    public bool SearchSubdirectories { get; set; }
    
    /// <summary>
    ///     Whether search pattern applies to files in subfolders.
    /// </summary>
    public SearchOption SearchOption { get; }

    /// <summary>
    ///     Input string for enum.
    ///     Which game to serialize.
    /// </summary>
    [Option(ArgsShort.SerializationFormat, Args.SerializationFormat, HelpText = Help.SerializationFormat)]
    public string SerializationFormat { get; set; }

    /// <summary>
    ///     Which game to serialize.
    /// </summary>
    public SerializeFormat SerializeFormat { get; }

    /// <summary>
    ///     Input string for enum.
    ///     Which region to serialize to.
    /// </summary>
    [Option(ArgsShort.SerializationRegion, Args.SerializationRegion, HelpText = Help.SerializationRegion)]
    public string SerializationRegionStr { get; set; }

    /// <summary>
    ///     Which region to serialize to.
    /// </summary>
    public Region SerializationRegion { get; }

}
