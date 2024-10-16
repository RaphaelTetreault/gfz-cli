using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manifold.GFZCLI
{
    public static class Program
    {
        public static object LockConsoleWrite { get; } = new();
        public const ConsoleColor FileNameColor = ConsoleColor.Cyan;
        public const ConsoleColor OverwriteFileColor = ConsoleColor.DarkYellow;
        public const ConsoleColor WriteFileColor = ConsoleColor.Green;
        public const ConsoleColor SubTaskColor = ConsoleColor.DarkGray;
        public static readonly string[] HelpArg = ["--help"];

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
                // 
                case GfzCliAction.generate_asset_library: ActionsAssetLibrary.CreateGmaTplLibrary(options); break;
                // GCI
                case GfzCliAction.gci_extract_ghost: ActionsGhost.ExtractGhostFromGci(options); break;
                // GMA
                case GfzCliAction.gma_patch_submesh_render_flags: ActionsGMA.PatchSubmeshRenderFlags(options); break;
                // IO - IN-OUT TESTS
                case GfzCliAction.io_gma: ActionsIO.InOutGMA(options); break;
                case GfzCliAction.io_scene: ActionsIO.InOutScene(options); break;
                case GfzCliAction.io_scene_patch: ActionsIO.PatchSceneComment(options); break;
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
                case GfzCliAction.tpl_generate_mipmaps: ActionsTPL.TplGenerateMipmaps(options); break;
                case GfzCliAction.tpl_pack: ActionsTPL.TplPack(options); break;
                case GfzCliAction.tpl_unpack: ActionsTPL.TplUnpack(options); break;

                // PROGRAM-SPECIFIC
                case GfzCliAction.usage: PrintActionUsage(options); break;
                case GfzCliAction.none: PrintHelp(); break;

                // ANYTHING ELSE
                default:
                    {
                        string msg = $"Unimplemented command {options.Action}.";
                        throw new NotImplementedException(msg);
                    }
            }
        }

        public static void PrintHelp()
        {
            // Force show --help menu
            Parser.Default.ParseArguments<Options>(HelpArg).WithParsed(ExecuteAction);
        }

        public static void PrintActionUsage(Options options)
        {
            // For 'usage' command, enum is passed in as input path
            GfzCliAction action = GfzCliEnumParser.ParseUnderscoreToDash<GfzCliAction>(options.InputPath);

            // If invalid, show possible actions
            if (action == GfzCliAction.none)
            {
                Terminal.WriteLine("Invalid action specified. Here are all possible actions.");
                foreach (GfzCliAction value in Enum.GetValues<GfzCliAction>())
                {
                    // Skip meta values
                    if (value == GfzCliAction.none || value == GfzCliAction.usage)
                        continue;

                    // Print out actions
                    //string valueStr = value.ToString().Replace("_", "-");
                    //Terminal.WriteLine($"\t{valueStr}");
                    Terminal.Write($"\t");
                    PrintActionUsageComplete(value);
                }
            }
            else // print specific usage
            {
                PrintActionUsageComplete(action);
            }
        }

        public static void PrintActionUsageComplete(GfzCliAction action, ConsoleColor color = ConsoleColor.Cyan)
        {
            // Printable string of value
            string actionStr = action.ToString().Replace("_", "-");

            // If valid, get info about the action
            var actionAttribute = AttributeHelper.GetAttribute<ActionAttribute, GfzCliAction>(action);
            if (actionAttribute == null)
            {
                Terminal.WriteLine($"{actionStr} [usage not yet defined]", ConsoleColor.Red);
                return;
            }

            // Input specifier
            string input = actionAttribute.IOMode switch
            {
                ActionIO.FileIn or
                ActionIO.FileInOut => " <input-file>",

                ActionIO.DirectoryIn or
                ActionIO.DirectoryInOut => " <input-directory>",

                ActionIO.PathIn or
                ActionIO.PathInOut => " <input-path>",

                _ => string.Empty,
            };

            // Output specifier
            string output = actionAttribute.IOMode switch
            {
                ActionIO.FileOut or
                ActionIO.FileInOut => " <output-file>",

                ActionIO.DirectoryOut or
                ActionIO.DirectoryInOut => " <output-directory>",

                ActionIO.PathOut or
                ActionIO.PathInOut => " <output-path>",

                _ => string.Empty,
            };

            // Construct hint and print
            string generalOptions = GetActionOptionsMessage(actionAttribute.Options);
            string specialOptions = actionAttribute.SpecialOptions;
            string hint = $"{actionStr}{input}{output}{generalOptions} {specialOptions}";
            Terminal.WriteLine(hint, color);
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
                    case ActionOption.OverwriteFiles: builder.Append(IGfzCliOptions.ArgsShort.OverwriteFiles); break;
                    case ActionOption.SearchPattern: builder.Append(IGfzCliOptions.ArgsShort.SearchPattern); break;
                    case ActionOption.SearchSubdirectories: builder.Append(IGfzCliOptions.ArgsShort.SearchSubdirectories); break;
                    case ActionOption.SerializationFormat: builder.Append(IGfzCliOptions.ArgsShort.SerializationFormat); break;
                    case ActionOption.SerializationRegion: builder.Append(IGfzCliOptions.ArgsShort.SerializationRegion); break;
                    default: throw new NotImplementedException(option.ToString());
                }
            }
            // Close options and finish
            builder.Append(']');
            return builder.ToString();
        }
    }
}