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
        public static readonly string[] Help = new string[] { "--help" };

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
                args = Help;
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
                case GfzCliAction.arc_compress: ActionsARC.ArcCompress(options); break;
                case GfzCliAction.arc_decompress: ActionsARC.ArcDecompress(options); break;
                // GCI Utility
                //case GfzCliAction.auto_rename_gci: ActionsGCI.RenameGCI(options); break;
                // CARDATA
                case GfzCliAction.cardata_bin_to_tsv: ActionsCarData.CarDataBinToTsv(options); break;
                case GfzCliAction.cardata_tsv_to_bin: ActionsCarData.CarDataTsvToBin(options); break;
                // ISO
                case GfzCliAction.extract_iso_files: ActionsISO.IsoExtractAll(options); break;
                // EMBLEM
                case GfzCliAction.emblem_bin_to_image: ActionsEmblem.EmblemBinToImage(options); break;
                case GfzCliAction.emblem_gci_to_image: ActionsEmblem.EmblemGciToImage(options); break;
                case GfzCliAction.image_to_emblem_bin: ActionsEmblem.ImageToEmblemBIN(options); break;
                case GfzCliAction.image_to_emblem_gci: ActionsEmblem.ImageToEmblemGCI(options); break;
                // FMI
                case GfzCliAction.fmi_to_plaintext: ActionsFMI.FmiToPlainText(options); break;
                case GfzCliAction.fmi_from_plaintext: ActionsFMI.FmiFromPlaintext(options); break;
                // GCI
                case GfzCliAction.gci_extract_ghost: ActionsGhost.ExtractGhostFromGci(options); break;
                // LIVE CAMERA STAGE
                case GfzCliAction.live_camera_stage_bin_to_tsv: ActionsLiveCameraStage.LiveCameraStageBinToTsv(options); break;
                case GfzCliAction.live_camera_stage_tsv_to_bin: ActionsLiveCameraStage.LiveCameraStageTsvToBin(options); break;
                // LZ
                case GfzCliAction.lz_compress: ActionsLZ.LzCompress(options); break;
                case GfzCliAction.lz_decompress: ActionsLZ.LzDecompress(options); break;
                // line__.rel
                case GfzCliAction.linerel_bgm: ActionsLineREL.PatchBgm(options); break;
                case GfzCliAction.linerel_bgmfl: ActionsLineREL.PatchBgmFinalLap(options); break;
                case GfzCliAction.linerel_bgm_both: ActionsLineREL.PatchBgmBoth(options); break;
                case GfzCliAction.linerel_clear_all_stage_names: ActionsLineREL.PatchClearAllCourseNames(options); break;
                case GfzCliAction.linerel_clear_unused_stage_names: ActionsLineREL.PatchClearUnusedCourseNames(options); break;
                case GfzCliAction.linerel_decrypt: ActionsLineREL.DecryptLineRel(options); break;
                case GfzCliAction.linerel_encrypt: ActionsLineREL.EncryptLineRel(options); break;
                case GfzCliAction.linerel_set_cup: ActionsLineREL.PatchCup(options); break;
                case GfzCliAction.linerel_set_difficulty: ActionsLineREL.PatchCourseDifficulty(options); break;
                case GfzCliAction.linerel_set_name_stage: ActionsLineREL.PatchCourseName(options); break;
                case GfzCliAction.linerel_set_venue: ActionsLineREL.PatchVenueIndex(options); break;
                //case GfzCliAction.linerel_set_venue_name: ActionsLineREL.PatchVenueIndex(options); break;
                // TPL
                case GfzCliAction.tpl_unpack: ActionsTPL.TplUnpack(options); break;
                //case GfzCliAction.tpl_pack: TplPack(options); break;

                //
                //case GfzCliAction.dump_hex: ActionsMisc.DumpHex32(options); break;

                // UNSET
                case GfzCliAction.none:
                    {
                        string msg = $"Could not parse action '{options.ActionStr}'.";
                        Terminal.WriteLine(msg);
                        Terminal.WriteLine();
                        // Force show --help menu
                        Parser.Default.ParseArguments<Options>(Help).WithParsed(ExecuteAction);
                    }
                    break;

                default:
                    {
                        string msg = $"Unimplemented command {options.Action}.";
                        throw new NotImplementedException(msg);
                    }
            }
        }
    }
}