using CommandLine;
using Manifold.IO;
using System;
using System.Collections.Generic;
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

    private static readonly GfzCliAction ActionUsage = new()
    {
        Description = "Call to print out actions available and how to use them.",
        Action = PrintActionUsage,
        ActionID = CliActionID.usage,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    private static readonly GfzCliAction ActionList = new()
    {
        Description = "List all possible actions with description.",
        Action = PrintActionList,
        ActionID = CliActionID.list,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    private static readonly GfzCliAction ActionNone = new()
    {
        Description = "No action selected.",
        Action = PrintActionUsage,
        ActionID = CliActionID.none,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    //public static readonly GfzCliAction Action = new()
    //{
    //    Description = "",
    //    Action = ,
    //    ActionID = CliActionID,
    //    InputIO = CliActionIO,
    //    OutputIO = CliActionIO,
    //    IsOutputOptional = true,
    //    ActionOptions = CliActionOption,
    //    RequiredArguments = [],
    //    OptionalArguments = [],
    //};

    public static readonly Dictionary<CliActionID, GfzCliAction> GfzCliActionsLibrary = [];
    public static readonly GfzCliAction[] GfzCliActions =
    [
        // PROGRAM-SPECIFIC: note same class actions need to be initialzed before this code runs...
        ActionNone,
        ActionList,
        ActionUsage,
        // ARC
        ActionsARC.ActionArcPack,
        ActionsARC.ActionArcUnpack,
        // CARDATA
        ActionsCarData.ActionCarDataToTSV,
        ActionsCarData.ActionCardDataFromTSV,
        // COLICOURSE
        ActionsColiCourse.ActionPatchFog,
        ActionsColiCourse.ActionPatchObjectRenderFlags,
        // ISO
        ActionsISO.ActionExtractISO,
        // TODO: Extract ./files/ only
        // TODO: Extract ./sys/ only
        // ENCODE TEXT
        ActionsEncodeText.ActionEncodeBytesToShiftJis,
        ActionsEncodeText.ActionEncodeWindows1252ToShiftJis,
        // EMBLEM
        ActionsEmblem.UsageEmblemsBinFromImages,
        ActionsEmblem.UsageEmblemsBinToImages,
        ActionsEmblem.UsageEmblemGciFromImage,
        ActionsEmblem.UsageEmblemGciToImage,
        // FMI
        ActionsFMI.ActionFmiFromPlainText,
        ActionsFMI.ActionFmiToPlainText,
        // GEN ASSETS

        // GCI
        ActionsGhost.ActionExtractGhostFromGci,
        // GMA
        ActionsGMA.ActionPatchSubmeshRenderFlags,
        // IO: IN-OUT TESTS
        ActionsIO.ActionInOutGMA,
        ActionsIO.ActionInOutTPL,
        ActionsIO.ActionInOutScene,
        ActionsIO.ActionInOutScenePatch,
        // LIVE CAMERA STAGE
        //case CliActionID.live_camera_stage_from_tsv: ActionsLiveCameraStage.LiveCameraStageFromTsv(options); break;
        //case CliActionID.live_camera_stage_to_tsv: ActionsLiveCameraStage.LiveCameraStageToTsv(options); break;
        // LZ
        //case CliActionID.lz_compress: ActionsLZ.LzCompress(options); break;
        //case CliActionID.lz_decompress: ActionsLZ.LzDecompress(options); break;
        // line__.rel
        //case CliActionID.linerel_clear_all_course_names: ActionsLineREL.PatchClearAllCourseNames(options); break;
        //case CliActionID.linerel_clear_all_venue_names: ActionsLineREL.PatchClearAllVenueNames(options); break;
        //case CliActionID.linerel_clear_unused_course_names: ActionsLineREL.PatchClearUnusedCourseNames(options); break;
        //case CliActionID.linerel_clear_unused_venue_names: ActionsLineREL.PatchClearUnusedVenueNames(options); break;
        //case CliActionID.linerel_decrypt: ActionsLineREL.DecryptLineRel(options); break;
        //case CliActionID.linerel_encrypt: ActionsLineREL.EncryptLineRel(options); break;
        ActionsLineREL.ActionPatchBgm,
        ActionsLineREL.ActionPatchBgmFinalLap,
        ActionsLineREL.ActionPatchBgmBoth,
        //case CliActionID.linerel_set_cardata: ActionsLineREL.PatchSetCarData(options); break;
        //case CliActionID.linerel_set_course_difficulty: ActionsLineREL.PatchSetCourseDifficulty(options); break;
        //case CliActionID.linerel_set_course_name: ActionsLineREL.PatchSetCourseName(options); break;
        //case CliActionID.linerel_set_cup_course: ActionsLineREL.PatchSetCupCourse(options); break;
        //case CliActionID.linerel_set_machine_rating: ActionsLineREL.PatchMachineRating(options); break;
        //case CliActionID.linerel_set_max_speed: ActionsLineREL.PatchMaxSpeed(options); break;
        //case CliActionID.linerel_set_venue: ActionsLineREL.PatchSetVenueIndex(options); break;
        //case CliActionID.linerel_set_venue_name: ActionsLineREL.PatchSetVenueName(options); break;
        // TPL
        //case CliActionID.tpl_generate_mipmaps: ActionsTPL.TplGenerateMipmaps(options); break;
        //case CliActionID.tpl_pack: ActionsTPL.TplPack(options); break;
        //case CliActionID.tpl_unpack: ActionsTPL.TplUnpack(options); break;
    ];

    private static void InitUsageDictionary()
    {
        foreach (GfzCliAction value in GfzCliActions)
        {
            CliActionID key = value.ActionID;

            // Assert no duplicate value (two of the same records)
            bool doesContainValue = GfzCliActionsLibrary.ContainsValue(value);
            if (doesContainValue)
            {
                string message =
                    $"Duplicate {nameof(GfzCliAction)} value \"{value}\" in {nameof(GfzCliActions)}! " +
                    $"{nameof(GfzCliAction)}.{nameof(GfzCliAction.ActionID)} is \"{value.ActionID}\".";
                throw new ArgumentException(message);
            }

            // Assert no duplicate keys (two actions with same ID)
            bool doesContainKey = GfzCliActionsLibrary.ContainsKey(key);
            if (doesContainKey)
            {
                string message = $"Duplicate {nameof(CliActionID)} key \"{key}\" in {nameof(GfzCliActions)}!";
                throw new ArgumentException(message);
            }

            GfzCliActionsLibrary.Add(key, value);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // Initialize text capabilities
        var encodingProvider = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(encodingProvider);
        Console.OutputEncoding = Encoding.Unicode;

        InitUsageDictionary();

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
        GfzCliAction gfzCliAction = GfzCliActionsLibrary[options.Action];
        Assert.IsTrue(gfzCliAction.ActionID == options.Action);
        gfzCliAction.Action.Invoke(options);
    }

    public static void PrintHelp()
    {
        // Force show --help menu
        Parser.Default.ParseArguments<Options>(HelpArg).WithParsed(ExecuteAction);
    }


    // TODO: use these instead of throwing errors! (When possible? Does this make sense? Maybe do custom error?)

    [Obsolete]
    public static void ActionWarning(Options options, string message)
    {
        throw new NotImplementedException();
    }

    [Obsolete]
    public static void ActionNotification(string message)
    {
        throw new NotImplementedException();
    }

    public static void PrintActionUsage(Options _)
    {
        // TODO: distinguish 'usage' from 'usage action'

        foreach (var kvp in GfzCliActionsLibrary)
        {
            // Skip these helpers
            if (kvp.Key == CliActionID.none ||
                kvp.Key == CliActionID.list || 
                kvp.Key == CliActionID.usage)
                continue;

            kvp.Value.PrintAllArguments();
        }
    }

    public static void PrintActionList(Options _)
    {
        foreach (var kvp in GfzCliActionsLibrary)
        {
            // Skip these helpers
            if (kvp.Key == CliActionID.none ||
                kvp.Key == CliActionID.list ||
                kvp.Key == CliActionID.usage)
                continue;

            kvp.Value.PrintActionAndDescription();
        }
    }
}