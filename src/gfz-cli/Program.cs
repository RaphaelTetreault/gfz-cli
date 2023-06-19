using CommandLine;
using GameCube.GX.Texture;
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;

namespace Manifold.GFZCLI
{
    internal class Program
    {
        public static readonly object lock_ConsoleWrite = new();
        public const ConsoleColor FileNameColor = ConsoleColor.Cyan;
        public const ConsoleColor OverwriteFileColor = ConsoleColor.DarkYellow;
        public const ConsoleColor WriteFileColor = ConsoleColor.Green;
        public static readonly string[] Help = new string[] { "--help" };

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
                string msg = "You must call this program using arguments via the Console/Terminal. ";
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

        public static void ExecuteAction(Options options)
        {
            switch (options.Action)
            {
                // ARC
                case GfzCliAction.arc_compress: ActionsARC.ArcCompress(options); break;
                case GfzCliAction.arc_decompress: ActionsARC.ArcDecompress(options); break;
                // CARDATA
                case GfzCliAction.cardata_bin_to_tsv: ActionsCarData.CarDataBinToTsv(options); break;
                case GfzCliAction.cardata_tsv_to_bin: ActionsCarData.CarDataTsvToBin(options); break;
                // ISO
                case GfzCliAction.extract_iso_files: ActionsISO.IsoExtractAll(options); break;
                // EMBLEM
                case GfzCliAction.emblem_to_image: ActionsEmblem.EmblemToImage(options); break;
                case GfzCliAction.image_to_emblem_bin: ActionsEmblem.ImageToEmblemBIN(options); break;
                case GfzCliAction.image_to_emblem_gci: ActionsEmblem.ImageToEmblemGCI(options); break;
                // LIVE CAMERA STAGE
                case GfzCliAction.live_camera_stage_bin_to_tsv: ActionsLiveCameraStage.LiveCameraStageBinToTsv(options); break;
                case GfzCliAction.live_camera_stage_tsv_to_bin: ActionsLiveCameraStage.LiveCameraStageTsvToBin(options); break;
                // LZ
                case GfzCliAction.lz_compress: ActionsLZ.LzCompress(options); break;
                case GfzCliAction.lz_decompress: ActionsLZ.LzDecompress(options); break;
                // TPL
                case GfzCliAction.tpl_unpack: ActionsTPL.TplUnpack(options); break;
                //case GfzCliAction.tpl_pack: TplPack(options); break;

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


        // TODO: place these in place meant for utility functions
        // Perhaps combine with "MultiFileUtility"
        public static Image<Rgba32> TextureToImage(Texture texture)
        {
            Image<Rgba32> image = new Image<Rgba32>(texture.Width, texture.Height);

            for (int y = 0; y < texture.Height; y++)
            {
                for (int x = 0; x < texture.Width; x++)
                {
                    TextureColor pixel = texture[x, y];
                    image[x, y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
                }
            }

            return image;
        }
        public static Texture ImageToTexture(Image<Rgba32> image, TextureFormat textureFormat = TextureFormat.RGBA8)
        {
            var texture = new Texture(image.Width, image.Height, textureFormat);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Rgba32 pixel = image[x, y];
                    texture[x, y] = new TextureColor(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            return texture;
        }
        public static void WriteImage(Options options, IImageEncoder encoder, Texture texture, FileWriteInfo info)
        {
            var action = () =>
            {
                Image<Rgba32> image = TextureToImage(texture);
                image.Save(info.OutputFilePath, encoder);
            };

            FileWriteOverwriteHandler(options, action, info);
        }
        public static void FileWriteOverwriteHandler(Options options, Action fileWrite, FileWriteInfo info)
        {
            bool outputFileExists = File.Exists(info.OutputFilePath);
            bool doWriteFile = !outputFileExists || options.OverwriteFiles;
            bool isOverwritingFile = outputFileExists && doWriteFile;
            var writeColor = isOverwritingFile ? OverwriteFileColor : WriteFileColor;
            var writeMsg = isOverwritingFile ? "Overwrote" : "Wrote";

            lock (lock_ConsoleWrite)
            {
                Terminal.Write($"{info.PrintDesignator}: ");
                if (doWriteFile)
                {
                    Terminal.Write(info.PrintActionDescription);
                    Terminal.Write(" ");
                    Terminal.Write(info.InputFilePath, FileNameColor);
                    Terminal.Write(". ");
                    Terminal.Write(writeMsg, writeColor);
                    Terminal.Write(" file ");
                    Terminal.Write(info.OutputFilePath, FileNameColor);
                }
                else
                {
                    Terminal.Write("skip ");
                    Terminal.Write(info.PrintActionDescription);
                    Terminal.Write(" ");
                    Terminal.Write(info.InputFilePath, FileNameColor);
                    Terminal.Write(" since ");
                    Terminal.Write(info.OutputFilePath, FileNameColor);
                    Terminal.Write(" already exists. ");
                    Terminal.Write(info.PrintMoreInfoOnSkip);
                }
                Terminal.WriteLine();
            }

            if (doWriteFile)
            {
                fileWrite.Invoke();
            }
        }
        public static string S(int length)
        {
            if (length == 1)
                return "";
            else
                return "s";
        }
        public static string S(Array array) => S(array.Length);
    }
}