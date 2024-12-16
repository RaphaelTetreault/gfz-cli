using CommandLine;
using Manifold.IO;
using System;
using System.Collections.Generic;
using System.IO;
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
        //case CliActionID.extract_iso: ActionsISO.IsoExtractAll(options); break;
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
        //case CliActionID.fmi_from_plaintext: ActionsFMI.FmiFromPlaintext(options); break;
        //case CliActionID.fmi_to_plaintext: ActionsFMI.FmiToPlainText(options); break;
             
        //case CliActionID.generate_asset_library: ActionsAssetLibrary.CreateGmaTplLibrary(options); break;
        // GCI
        //case CliActionID.gci_extract_ghost: ActionsGhost.ExtractGhostFromGci(options); break;
        // GMA
        //case CliActionID.gma_patch_submesh_render_flags: ActionsGMA.PatchSubmeshRenderFlags(options); break;
        // IO - IN-OUT TESTS
        //case CliActionID.io_gma: ActionsIO.InOutGMA(options); break;
        //case CliActionID.io_scene: ActionsIO.InOutScene(options); break;
        //case CliActionID.io_scene_patch: ActionsIO.PatchSceneComment(options); break;
        //case CliActionID.io_tpl: ActionsIO.InOutTPL(options); break;
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
        //case CliActionID.linerel_set_bgm: ActionsLineREL.PatchSetBgm(options); break;
        //case CliActionID.linerel_set_bgmfl: ActionsLineREL.PatchSetBgmFinalLap(options); break;
        //case CliActionID.linerel_set_bgm_bgmfl: ActionsLineREL.PatchSetBgmAndBgmFinalLap(options); break;
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
        // NEW WAY!!!
        GfzCliAction gfzCliAction = GfzCliActionsLibrary[options.Action];
        Assert.IsTrue(gfzCliAction.ActionID == options.Action);
        gfzCliAction.Action.Invoke(options);
        return;

        switch (options.Action)
        {
            // ARC
            case CliActionID.arc_pack: ActionsARC.ArcPack(options); break;
            case CliActionID.arc_unpack: ActionsARC.ArcUnpack(options); break;
            // CARDATA
            case CliActionID.cardata_from_tsv: ActionsCarData.CarDataFromTsv(options); break;
            case CliActionID.cardata_to_tsv: ActionsCarData.CarDataToTsv(options); break;
            // COLICOURSE
            case CliActionID.colicourse_patch_fog: ActionsColiCourse.PatchFog(options); break;
            case CliActionID.colicourse_patch_object_render_flags: ActionsColiCourse.PatchSceneObjectDynamicRenderFlags(options); break;
            // ISO
            case CliActionID.extract_iso: ActionsISO.IsoExtractAll(options); break;
            // TODO: Extract ./files/ only
            // TODO: Extract ./sys/ only
            // ENCODE TEXT
            case CliActionID.encode_windows_to_shift_jis: ActionsEncodeText.PrintAsciiToShiftJis(options); break;
            case CliActionID.encode_bytes_to_shift_jis: ActionsEncodeText.PrintBytesToShiftJis(options); break;
            // EMBLEM
            case CliActionID.emblems_bin_from_images: ActionsEmblem.EmblemsBinFromImages(options); break;
            case CliActionID.emblems_bin_to_images: ActionsEmblem.EmblemsBinToImages(options); break;
            case CliActionID.emblem_gci_from_image: ActionsEmblem.EmblemGciFromImage(options); break;
            case CliActionID.emblem_gci_to_image: ActionsEmblem.EmblemGciToImage(options); break;
            // FMI
            case CliActionID.fmi_from_plaintext: ActionsFMI.FmiFromPlaintext(options); break;
            case CliActionID.fmi_to_plaintext: ActionsFMI.FmiToPlainText(options); break;
            // 
            case CliActionID.generate_asset_library: ActionsAssetLibrary.CreateGmaTplLibrary(options); break;
            // GCI
            case CliActionID.gci_extract_ghost: ActionsGhost.ExtractGhostFromGci(options); break;
            // GMA
            case CliActionID.gma_patch_submesh_render_flags: ActionsGMA.PatchSubmeshRenderFlags(options); break;
            // IO - IN-OUT TESTS
            case CliActionID.io_gma: ActionsIO.InOutGMA(options); break;
            case CliActionID.io_scene: ActionsIO.InOutScene(options); break;
            case CliActionID.io_scene_patch: ActionsIO.PatchSceneComment(options); break;
            case CliActionID.io_tpl: ActionsIO.InOutTPL(options); break;
            // LIVE CAMERA STAGE
            case CliActionID.live_camera_stage_from_tsv: ActionsLiveCameraStage.LiveCameraStageFromTsv(options); break;
            case CliActionID.live_camera_stage_to_tsv: ActionsLiveCameraStage.LiveCameraStageToTsv(options); break;
            // LZ
            case CliActionID.lz_compress: ActionsLZ.LzCompress(options); break;
            case CliActionID.lz_decompress: ActionsLZ.LzDecompress(options); break;
            // line__.rel
            case CliActionID.linerel_clear_all_course_names: ActionsLineREL.PatchClearAllCourseNames(options); break;
            case CliActionID.linerel_clear_all_venue_names: ActionsLineREL.PatchClearAllVenueNames(options); break;
            case CliActionID.linerel_clear_unused_course_names: ActionsLineREL.PatchClearUnusedCourseNames(options); break;
            case CliActionID.linerel_clear_unused_venue_names: ActionsLineREL.PatchClearUnusedVenueNames(options); break;
            case CliActionID.linerel_decrypt: ActionsLineREL.DecryptLineRel(options); break;
            case CliActionID.linerel_encrypt: ActionsLineREL.EncryptLineRel(options); break;
            case CliActionID.linerel_set_bgm: ActionsLineREL.PatchSetBgm(options); break;
            case CliActionID.linerel_set_bgmfl: ActionsLineREL.PatchSetBgmFinalLap(options); break;
            case CliActionID.linerel_set_bgm_bgmfl: ActionsLineREL.PatchSetBgmAndBgmFinalLap(options); break;
            case CliActionID.linerel_set_cardata: ActionsLineREL.PatchSetCarData(options); break;
            case CliActionID.linerel_set_course_difficulty: ActionsLineREL.PatchSetCourseDifficulty(options); break;
            case CliActionID.linerel_set_course_name: ActionsLineREL.PatchSetCourseName(options); break;
            case CliActionID.linerel_set_cup_course: ActionsLineREL.PatchSetCupCourse(options); break;
            case CliActionID.linerel_set_machine_rating: ActionsLineREL.PatchMachineRating(options); break;
            case CliActionID.linerel_set_max_speed: ActionsLineREL.PatchMaxSpeed(options); break;
            case CliActionID.linerel_set_venue: ActionsLineREL.PatchSetVenueIndex(options); break;
            case CliActionID.linerel_set_venue_name: ActionsLineREL.PatchSetVenueName(options); break;
            // TPL
            case CliActionID.tpl_generate_mipmaps: ActionsTPL.TplGenerateMipmaps(options); break;
            case CliActionID.tpl_pack: ActionsTPL.TplPack(options); break;
            case CliActionID.tpl_unpack: ActionsTPL.TplUnpack(options); break;

            // PROGRAM-SPECIFIC
            case CliActionID.usage: PrintActionUsage(options); break;
            case CliActionID.none: PrintHelp(); break;

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


    //    // TODO: use these instead of throwing errors! (When possible? Does this make sense? Maybe do custom error?)

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