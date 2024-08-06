using CommandLine;
using System;

namespace Manifold.GFZCLI
{
    public static class Program
    {
        public static object lock_ConsoleWrite { get; } = new();
        public const ConsoleColor FileNameColor = ConsoleColor.Cyan;
        public const ConsoleColor OverwriteFileColor = ConsoleColor.DarkYellow;
        public const ConsoleColor WriteFileColor = ConsoleColor.Green;
        public const ConsoleColor SubTaskColor = ConsoleColor.DarkGray;
        public static readonly string[] HelpArg = new string[] { "--help" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Initialize text capabilities
            var encodingProvider = System.Text.CodePagesEncodingProvider.Instance;
            System.Text.Encoding.RegisterProvider(encodingProvider);

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
                case GfzCliAction.arc_pack: ActionsARC.ArcPack(options); break;
                case GfzCliAction.arc_unpack: ActionsARC.ArcUnpack(options); break;
                // CARDATA
                case GfzCliAction.cardata_from_tsv: ActionsCarData.CarDataFromTsv(options); break;
                case GfzCliAction.cardata_to_tsv: ActionsCarData.CarDataToTsv(options); break;
                // COLICOURSE
                case GfzCliAction.colicourse_patch_fog: ActionsColiCourse.PatchFog(options); break;
                case GfzCliAction.colicourse_patch_object_render_flags: ActionsColiCourse.PatchSceneObjectDynamicRenderFlags(options); break;
                // ISO
                case GfzCliAction.extract_iso: ActionsISO.IsoExtractAll(options); break;
                    // TODO: Extract ./files/ only
                    // TODO: Extract ./sys/ only
                // EMBLEM
                case GfzCliAction.emblems_bin_from_images: ActionsEmblem.EmblemsBinFromImages(options); break;
                case GfzCliAction.emblems_bin_to_images: ActionsEmblem.EmblemsBinToImages(options); break;
                case GfzCliAction.emblem_gci_from_image: ActionsEmblem.EmblemGciFromImage(options); break;
                case GfzCliAction.emblem_gci_to_image: ActionsEmblem.EmblemGciToImage(options); break;
                // FMI
                case GfzCliAction.fmi_from_plaintext: ActionsFMI.FmiFromPlaintext(options); break;
                case GfzCliAction.fmi_to_plaintext: ActionsFMI.FmiToPlainText(options); break;
                // GCI
                case GfzCliAction.gci_extract_ghost: ActionsGhost.ExtractGhostFromGci(options); break;
                // GMA
                case GfzCliAction.gma_patch_submesh_render_flags: ActionsGMA.PatchSubmeshRenderFlags(options); break;
                // IO - IN-OUT TESTS
                case GfzCliAction.io_gma: ActionsIO.InOutGMA(options); break;
                case GfzCliAction.io_scene: ActionsIO.InOutScene(options); break;
                case GfzCliAction.io_tpl: ActionsIO.InOutTPL(options); break;
                // LIVE CAMERA STAGE
                case GfzCliAction.live_camera_stage_from_tsv: ActionsLiveCameraStage.LiveCameraStageFromTsv(options); break;
                case GfzCliAction.live_camera_stage_to_tsv: ActionsLiveCameraStage.LiveCameraStageToTsv(options); break;
                // LZ
                case GfzCliAction.lz_compress: ActionsLZ.LzCompress(options); break;
                case GfzCliAction.lz_decompress: ActionsLZ.LzDecompress(options); break;
                // line__.rel
                case GfzCliAction.linerel_clear_all_course_names: ActionsLineREL.PatchClearAllCourseNames(options); break;
                case GfzCliAction.linerel_clear_all_venue_names: ActionsLineREL.PatchClearAllVenueNames(options); break;
                case GfzCliAction.linerel_clear_unused_course_names: ActionsLineREL.PatchClearUnusedCourseNames(options); break;
                case GfzCliAction.linerel_clear_unused_venue_names: ActionsLineREL.PatchClearUnusedVenueNames(options); break;
                case GfzCliAction.linerel_decrypt: ActionsLineREL.DecryptLineRel(options); break;
                case GfzCliAction.linerel_encrypt: ActionsLineREL.EncryptLineRel(options); break;
                case GfzCliAction.linerel_set_bgm: ActionsLineREL.PatchSetBgm(options); break;
                case GfzCliAction.linerel_set_bgmfl: ActionsLineREL.PatchSetBgmFinalLap(options); break;
                case GfzCliAction.linerel_set_bgm_bgmfl: ActionsLineREL.PatchSetBgmAndBgmFinalLap(options); break;
                case GfzCliAction.linerel_set_cardata: ActionsLineREL.PatchSetCarData(options); break;
                case GfzCliAction.linerel_set_course_difficulty: ActionsLineREL.PatchSetCourseDifficulty(options); break;
                case GfzCliAction.linerel_set_course_name: ActionsLineREL.PatchSetCourseName(options); break;
                case GfzCliAction.linerel_set_cup_course: ActionsLineREL.PatchSetCupCourse(options); break;
                case GfzCliAction.linerel_set_machine_rating: ActionsLineREL.PatchMachineRating(options); break;
                case GfzCliAction.linerel_set_venue: ActionsLineREL.PatchSetVenueIndex(options); break;
                case GfzCliAction.linerel_set_venue_name: ActionsLineREL.PatchSetVenueName(options); break;
                // TPL
                case GfzCliAction.tpl_unpack: ActionsTPL.TplUnpack(options); break;
                //case GfzCliAction.tpl_pack: TplPack(options); break;

                // UNSET
                case GfzCliAction.none: ForceShowHelp(); break;

                // ANYTHING ELSE
                default:
                    {
                        string msg = $"Unimplemented command {options.Action}.";
                        throw new NotImplementedException(msg);
                    }
            }
        }

        public static void ForceShowHelp()
        {
            // Force show --help menu
            Parser.Default.ParseArguments<Options>(HelpArg).WithParsed(ExecuteAction);
        }
    }
}