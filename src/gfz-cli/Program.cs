using CommandLine;
using System;
using System.Text;

namespace Manifold.GFZCLI;

public static class Program
{
    public static object LockConsoleWrite { get; } = new();
    public const ConsoleColor FileNameColor = ConsoleColor.Cyan;
    public const ConsoleColor FileWriteColor = ConsoleColor.Green;
    public const ConsoleColor FileOverwriteColor = ConsoleColor.DarkYellow;
    public const ConsoleColor FileOverwriteSkipColor = ConsoleColor.Red;
    public const ConsoleColor SubTaskColor = ConsoleColor.DarkGray;
    public const ConsoleColor WarningColor = ConsoleColor.Red;
    public const ConsoleColor NotificationColor = ConsoleColor.DarkYellow;
    public static readonly string[] HelpArg = ["--help"];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // Initialize text capabilities
        var encodingProvider = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(encodingProvider);

        // If user did not pass any arguments, tell them how to use application.
        // This will happen when users double-click application.
        bool noArgumentsPassed = args.Length == 0;
        if (noArgumentsPassed)
        {
            string msg = "You must call this program using arguments via the Console/Terminal.";
            Terminal.WriteLine(msg, ConsoleColor.Black, ConsoleColor.Red);
            Terminal.WriteLine();
            // Force help page
            args = HelpArg;
        }

        // Run program with options
        var parseResult = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(ExecuteAction);

        // If user did not pass any arguments, pause application so they can read Console.
        // This will happen when users double-click application.
        if (noArgumentsPassed)
        {
            string msg = "Press ENTER to continue.";
            Terminal.Write(msg, ConsoleColor.Black, ConsoleColor.Red);
            Terminal.WriteLine();
            Console.Read();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void ExecuteAction(Options options)
    {
        switch (options.Action)
        {
            // ARC
            case Actions.arc_pack: ActionsARC.ArcPack(options); break;
            case Actions.arc_unpack: ActionsARC.ArcUnpack(options); break;
            // CARDATA
            case Actions.cardata_from_tsv: ActionsCarData.CarDataFromTsv(options); break;
            case Actions.cardata_to_tsv: ActionsCarData.CarDataToTsv(options); break;
            // COLICOURSE
            case Actions.colicourse_patch_fog: ActionsColiCourse.PatchFog(options); break;
            case Actions.colicourse_patch_object_render_flags: ActionsColiCourse.PatchSceneObjectDynamicRenderFlags(options); break;
            // ISO
            case Actions.extract_iso: ActionsISO.IsoExtractAll(options); break;
            // TODO: Extract ./files/ only
            // TODO: Extract ./sys/ only
            // EMBLEM
            case Actions.emblems_bin_from_images: ActionsEmblem.EmblemsBinFromImages(options); break;
            case Actions.emblems_bin_to_images: ActionsEmblem.EmblemsBinToImages(options); break;
            case Actions.emblem_gci_from_image: ActionsEmblem.EmblemGciFromImage(options); break;
            case Actions.emblem_gci_to_image: ActionsEmblem.EmblemGciToImage(options); break;
            // FMI
            case Actions.fmi_from_plaintext: ActionsFMI.FmiFromPlaintext(options); break;
            case Actions.fmi_to_plaintext: ActionsFMI.FmiToPlainText(options); break;
            // 
            case Actions.generate_asset_library: ActionsAssetLibrary.CreateGmaTplLibrary(options); break;
            // GCI
            case Actions.gci_extract_ghost: ActionsGhost.ExtractGhostFromGci(options); break;
            // GMA
            case Actions.gma_patch_submesh_render_flags: ActionsGMA.PatchSubmeshRenderFlags(options); break;
            // IO - IN-OUT TESTS
            case Actions.io_gma: ActionsIO.InOutGMA(options); break;
            case Actions.io_scene: ActionsIO.InOutScene(options); break;
            case Actions.io_scene_patch: ActionsIO.PatchSceneComment(options); break;
            case Actions.io_tpl: ActionsIO.InOutTPL(options); break;
            // LIVE CAMERA STAGE
            case Actions.live_camera_stage_from_tsv: ActionsLiveCameraStage.LiveCameraStageFromTsv(options); break;
            case Actions.live_camera_stage_to_tsv: ActionsLiveCameraStage.LiveCameraStageToTsv(options); break;
            // LZ
            case Actions.lz_compress: ActionsLZ.LzCompress(options); break;
            case Actions.lz_decompress: ActionsLZ.LzDecompress(options); break;
            // line__.rel
            case Actions.linerel_clear_all_course_names: ActionsLineREL.PatchClearAllCourseNames(options); break;
            case Actions.linerel_clear_all_venue_names: ActionsLineREL.PatchClearAllVenueNames(options); break;
            case Actions.linerel_clear_unused_course_names: ActionsLineREL.PatchClearUnusedCourseNames(options); break;
            case Actions.linerel_clear_unused_venue_names: ActionsLineREL.PatchClearUnusedVenueNames(options); break;
            case Actions.linerel_decrypt: ActionsLineREL.DecryptLineRel(options); break;
            case Actions.linerel_encrypt: ActionsLineREL.EncryptLineRel(options); break;
            case Actions.linerel_set_bgm: ActionsLineREL.PatchSetBgm(options); break;
            case Actions.linerel_set_bgmfl: ActionsLineREL.PatchSetBgmFinalLap(options); break;
            case Actions.linerel_set_bgm_bgmfl: ActionsLineREL.PatchSetBgmAndBgmFinalLap(options); break;
            case Actions.linerel_set_cardata: ActionsLineREL.PatchSetCarData(options); break;
            case Actions.linerel_set_course_difficulty: ActionsLineREL.PatchSetCourseDifficulty(options); break;
            case Actions.linerel_set_course_name: ActionsLineREL.PatchSetCourseName(options); break;
            case Actions.linerel_set_cup_course: ActionsLineREL.PatchSetCupCourse(options); break;
            case Actions.linerel_set_machine_rating: ActionsLineREL.PatchMachineRating(options); break;
            case Actions.linerel_set_max_speed: ActionsLineREL.PatchMaxSpeed(options); break;
            case Actions.linerel_set_venue: ActionsLineREL.PatchSetVenueIndex(options); break;
            case Actions.linerel_set_venue_name: ActionsLineREL.PatchSetVenueName(options); break;
            // TPL
            case Actions.tpl_generate_mipmaps: ActionsTPL.TplGenerateMipmaps(options); break;
            case Actions.tpl_pack: ActionsTPL.TplPack(options); break;
            case Actions.tpl_unpack: ActionsTPL.TplUnpack(options); break;

            // PROGRAM-SPECIFIC
            case Actions.usage: PrintActionUsage(options); break;
            case Actions.none: PrintHelp(); break;

            // ANYTHING ELSE
            default:
                string msg = $"Unimplemented command {options.Action}.";
                throw new NotImplementedException(msg);
        }
    }

    public static void PrintHelp()
    {
        // Force show --help menu
        Parser.Default.ParseArguments<Options>(HelpArg).WithParsed(ExecuteAction);
    }

    public static void PrintActionUsage(Options options)
    {
        // Shenangians. Use input path as variable for this hack
        string actionStr = options.InputPath;

        // For 'usage' command, enum is passed in as input path
        Actions action = string.IsNullOrWhiteSpace(actionStr)
            ? Actions.none
            : GfzCliEnumParser.ParseUnderscoreToDash<Actions>(actionStr);

        if (action == Actions.none)
        {
            string msg = $"\"{actionStr}\" is an invalid action. Actions and general usage:";
            Terminal.WriteLine(msg);
            PrintActionUsageList();
        }
        else // print specific usage
        {
            PrintActionUsageComplete(action);
        }
    }

    public static void PrintActionUsageComplete(Actions action, ConsoleColor color = ConsoleColor.Cyan)
    {
        // Printable string of value
        string actionStr = action.ToString().Replace("_", "-");

        // If valid, get info about the action
        var actionAttribute = AttributeHelper.GetAttribute<ActionAttribute, Actions>(action);
        if (actionAttribute == null)
        {
            Terminal.WriteLine($"{actionStr} [usage not yet defined]", ConsoleColor.Red);
            return;
        }

        string input = actionAttribute.Input switch
        {
            ActionIO.Directory => " <input-directory>",
            ActionIO.File => " <input-file>",
            ActionIO.Path => " <input-path>",
            _ => string.Empty,
        };
        string optional = actionAttribute.IsOutputOptional ? "optional-" : string.Empty;
        string output = actionAttribute.Output switch
        {
            ActionIO.Directory => $" <{optional}output-directory>",
            ActionIO.File => $" <{optional}output-file>",
            ActionIO.Path => $" <{optional}output-path>",
            _ => string.Empty,
        };

        // Construct hint and print
        string generalOptions = GetActionOptionsMessage(actionAttribute.Options);
        string specialOptions = actionAttribute.SpecialOptions;
        string hint = $"{actionStr}{input}{output}{generalOptions} {specialOptions}";
        Terminal.WriteLine(hint, color);
    }

    public static void PrintActionUsageList()
    {
        foreach (Actions value in Enum.GetValues<Actions>())
        {
            // Skip meta values
            if (value == Actions.none || value == Actions.usage)
                continue;

            // Print out actions
            Terminal.Write($"\t");
            PrintActionUsageComplete(value);
        }
    }

    public static string GetActionOptionsMessage(ActionOption actionOptions)
    {
        // Prepare string
        StringBuilder builder = new StringBuilder(66);
        builder.Append(" [");

        // Iterate over all possible values
        for (int i = 0; i < 32; i++)
        {
            ActionOption option = (ActionOption)((uint)actionOptions & (1 << i));
            if (option == ActionOption.None)
                continue;

            // Add pipe if not at start of string
            if (builder[^1] != '[')
                builder.Append('|');
            // Add parameter dash
            builder.Append('-');

            // Add action char
            switch (option)
            {
                case ActionOption.OverwriteFiles: builder.Append(IOptionsGfzCli.ArgsShort.OverwriteFiles); break;
                case ActionOption.SearchPattern: builder.Append(IOptionsGfzCli.ArgsShort.SearchPattern); break;
                case ActionOption.SearchSubdirectories: builder.Append(IOptionsGfzCli.ArgsShort.SearchSubdirectories); break;
                case ActionOption.SerializationFormat: builder.Append(IOptionsGfzCli.ArgsShort.SerializationFormat); break;
                case ActionOption.SerializationRegion: builder.Append(IOptionsGfzCli.ArgsShort.SerializationRegion); break;
                default: throw new NotImplementedException(option.ToString());
            }
        }
        // Close options and finish
        builder.Append(']');
        return builder.ToString();
    }

    public static void ActionWarning(Options options, string message)
    {
        Terminal.WriteLine(message, WarningColor);
        PrintActionUsageComplete(options.Action, WarningColor);
    }

    public static void ActionNotification(string message)
    {
        Terminal.WriteLine(message, NotificationColor);
    }
}