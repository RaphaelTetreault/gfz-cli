using CommandLine;
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


    public static readonly Dictionary<Actions, UsageInfo> UsageInfo = [];
    public static readonly UsageInfo[] Usage =
    [
        // EMBLEM
        ActionsEmblem.UsageEmblemBinFromImage,
        ActionsEmblem.UsageEmblemBinToImage,
        ActionsEmblem.UsageEmblemGciFromImage,
        ActionsEmblem.UsageEmblemGciToImage,
    ];

    private static void InitUsageDictionary()
    {
        foreach (UsageInfo value in Usage)
        {
            Actions key = value.Action;
            UsageInfo.Add(key, value);
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
            // ENCODE TEXT
            case Actions.encode_windows_to_shift_jis: ActionsEncodeText.PrintAsciiToShiftJis(options); break;
            case Actions.encode_bytes_to_shift_jis: ActionsEncodeText.PrintBytesToShiftJis(options); break;
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

    public static void PrintActionUsage(Options options)
    {
        // TODO: distinguish 'usage' from 'usage action'

        foreach (var kvp in UsageInfo)
        {
            kvp.Value.PrintAllArguments();
        }
    }
}