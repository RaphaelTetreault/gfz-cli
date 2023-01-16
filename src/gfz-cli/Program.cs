using CommandLine;
using GameCube.GFZ.CarData;
using GameCube.GFZ.LZ;
using GameCube.GFZ.TPL;
using Manifold;
using Manifold.IO;
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using GameCube.GX.Texture;


// TODO:
// Make sure all parts of code respect new 'overwrite' flag
// 


namespace Manifold.GFZCLI
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
        }

        public static void RunOptions(Options options)
        {
            if (options.Verbose)
            {
                options.PrintState();
                Console.WriteLine();
            }
            VerboseConsole.IsVerbose = options.Verbose;

            // Everything else from here
            CarDataToTSV(options);
            CarDataToBIN(options);
            //
            LzDecompress(options);
            LzCompress(options);
            //
            TplUnpack(options);
            TplPack(options);
        }

        public static void CarDataToTSV(Options options)
        {
            string filePath = options.CarDataBinPath;
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File at path {filePath} does not exist.");

            // Decompress LZ if not decompressed yet
            string extension = Path.GetExtension(filePath);
            if (!string.IsNullOrEmpty(extension))
            {
                // Save decompressed file
                LzUtility.DecompressAvLzToDisk(filePath, true);
                VerboseConsole.WriteLine($"Decompressed {filePath} for conversion.");
                // Update path to decompressed file
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDir = Path.GetDirectoryName(filePath);
                filePath = Path.Combine(fileDir, fileName);
            }

            using (var reader = new EndianBinaryReader(File.OpenRead(filePath), CarData.endianness))
            {
                var carData = new CarData();
                reader.Read(ref carData);

                string outputPath = filePath + ".tsv";
                using (var writer = new StreamWriter(File.Create(outputPath)))
                {
                    carData.Serialize(writer);
                    Console.WriteLine($"Created file: {outputPath}");
                }
            }
        }
        public static void CarDataToBIN(Options options)
        {
            string filePath = options.CarDataTsvPath;
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File at path {filePath} does not exist.");

            // Open CarData
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                var carData = new CarData();
                carData.Deserialize(reader);

                // Update path to decompressed file
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDir = Path.GetDirectoryName(filePath);
                filePath = Path.Combine(fileDir, fileName);

                // Save out 
                string outputPath = filePath + ".lz";
                using (var writer = new EndianBinaryWriter(File.Create(outputPath), CarData.endianness))
                {
                    carData.Serialize(writer);
                    Console.WriteLine($"Created file: {outputPath}");
                }
                LzUtility.CompressAvLzToDisk(outputPath, GameCube.AmusementVision.GxGame.FZeroGX, true);
            }
        }

        public static void LzDecompress(Options options)
        {
            string path = options.LzDecompressTarget;
            if (string.IsNullOrEmpty(path))
                return;

            bool fileExists = File.Exists(path);
            bool dirExists = Directory.Exists(path);
            if (!fileExists && !dirExists)
                throw new ArgumentException($"Provided file or folder path target {path} does not exist.");

            if (fileExists)
            {
                LzUtility.DecompressAvLzToDisk(path, true);
                Console.WriteLine($"Decompressed: {path}");
            }
            else if (dirExists)
            {
                var searchOption = options.SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var enumerable = LzUtility.DecompressAvLzDirectoryToDisk(path, true, searchOption);
                foreach (var file in enumerable)
                    Console.WriteLine($"Decompressed: {file.filePath}");
            }
            else
            {
                Assert.IsTrue(false, "This code should never run.");
            }
        }
        public static void LzCompress(Options options)
        {
            string path = options.LzCompressTarget;
            if (string.IsNullOrEmpty(path))
                return;

            bool fileExists = File.Exists(path);
            bool dirExists = Directory.Exists(path);
            if (!fileExists && !dirExists)
                throw new ArgumentException($"Provided file or folder path target {path} does not exist.");
            bool hasSearchPattern = !string.IsNullOrEmpty(options.SearchPattern);
            if (dirExists && !hasSearchPattern)
                throw new ArgumentException($"Cannot compress target folder {path} without specifying a search pattern.");

            if (fileExists)
            {
                LzUtility.CompressAvLzToDisk(path, options.AvGame, true);
                Console.WriteLine($"Compressed: {path}");
            }
            else if (dirExists)
            {
                var searchOption = options.SearchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var enumerable = LzUtility.CompressAvLzDirectoryToDisk(path, options.AvGame, true, searchOption, options.SearchPattern);
                foreach (var file in enumerable)
                    Console.WriteLine($"Compressed: {file.filePath}");
            }
            else
            {
                Assert.IsTrue(false, "This code should never run.");
            }
        }

        public static void TplUnpack(Options options)
        {
            string path = options.TplUnpack;
            if (string.IsNullOrEmpty(path))
                return;

            // Ensure input file exists
            bool fileExists = File.Exists(path);
            if (!fileExists)
                throw new ArgumentException($"Target file '{path}' does not exist.");

            // Deserialize the TPL
            Tpl tpl = new Tpl();
            using (var reader = new EndianBinaryReader(File.OpenRead(path), Tpl.endianness))
            {
                tpl.Deserialize(reader);
                tpl.FileName = Path.GetFileNameWithoutExtension(path);
            }

            // Create folder named the same thing as the TPL input file
            string directory = Path.GetDirectoryName(path);
            directory = Path.Combine(directory, tpl.FileName);
            Directory.CreateDirectory(directory);

            // Iterate over texture and mipmaps, save to disk
            int tplIndex = 0;
            foreach (var textureSeries in tpl.TextureSeries)
            {
                tplIndex++;
                int mipmapIndex = 0;
                foreach (var textureEntry in textureSeries.Entries)
                {
                    bool isMipmap = mipmapIndex > 0;
                    bool skipMipmaps = isMipmap && !options.TplUnpackMipmaps;
                    if (skipMipmaps)
                        continue;

                    mipmapIndex++;

                    // Optionally bow out of saving texture if it is corrupted.
                    bool skipCorruptedTexture = textureEntry.IsCorrupted && !options.TplUnpackSaveCorruptedTextures;
                    if (skipCorruptedTexture)
                        continue;

                    // Copy contents of GameCube texture into ImageSharp representation
                    var texture = textureEntry.Texture;
                    Image<Rgba32> image = new Image<Rgba32>(texture.Width, texture.Height);

                    for (int y = 0; y < texture.Height; y++)
                    {
                        for (int x = 0; x < texture.Width; x++)
                        {
                            TextureColor pixel = texture[x, y];
                            image[x, y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
                        }
                    }

                    // Find where to save file
                    string fileName = $"{tplIndex}-{mipmapIndex}.png";
                    string filePath = Path.Combine(directory, fileName);
                    
                    bool skipFileWrite = File.Exists(filePath) && !options.OverwriteFiles;
                    if (skipFileWrite)
                        continue;

                    // Save to disk
                    image.SaveAsPng(filePath);
                    VerboseConsole.WriteLine($"Wrote file: {filePath}");
                }
            }
        }
        public static void TplPack(Options options)
        {
            string path = options.TplPack;
            if (string.IsNullOrEmpty(path))
                return;

            // Ensure input file exists
            bool fileExists = File.Exists(path);
            if (!fileExists)
                throw new ArgumentException($"Target file '{path}' does not exist.");

            // TEMP: just do 1 file

            Image<Rgba32> image = Image.Load<Rgba32>(path);
            Texture texture = new Texture(image.Width, image.Height, TextureFormat.CMPR);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Rgba32 pixel = image[x,y];
                    texture[x, y] = new TextureColor(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            using (var writer = new EndianBinaryWriter(new MemoryStream(), Tpl.endianness))
            {
                var encoding = Encoding.EncodingCMPR;
                //var encoding = Encoding.EncodingRGBA8;
                //var encoding = Encoding.EncodingRGB565;
                //var encoding = Encoding.EncodingRGB5A3;
                //var encoding = Encoding.EncodingIA8;
                //var encoding = Encoding.EncodingIA4;
                var blocks = texture.CreateTextureDirectColorBlocks(encoding, out int bch, out int bcv);
                encoding.WriteTexture(writer, blocks);

                writer.BaseStream.Position = 0;
                using (var reader = new EndianBinaryReader(writer.BaseStream, Tpl.endianness))
                {
                    var blocksCopy = encoding.ReadBlocks<DirectBlock>(reader, blocks.Length, encoding);
                    Texture textureCopy = Texture.FromDirectBlocks(blocksCopy, bch, bcv);

                    // HACK - copy/paste garbage test
                    // Copy contents of GameCube texture into ImageSharp representation
                    Image<Rgba32> imageCopy = new Image<Rgba32>(textureCopy.Width, textureCopy.Height);
                    for (int y = 0; y < textureCopy.Height; y++)
                    {
                        for (int x = 0; x < textureCopy.Width; x++)
                        {
                            TextureColor pixel = textureCopy[x, y];
                            imageCopy[x, y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
                        }
                    }
                    // Find where to save file
                    string directory = Path.GetDirectoryName(path);
                    string fileName = $"temp.png";
                    string filePath = Path.Combine(directory, fileName);
                    // Save to disk
                    imageCopy.SaveAsPng(filePath);
                    VerboseConsole.WriteLine($"Wrote file: {filePath}");
                }
            }

        }

        private static void AssertFileOrDirectoryExists(string path)
        {
            bool fileExists = File.Exists(path);
            bool dirExists = Directory.Exists(path);
            if (!fileExists && !dirExists)
                throw new ArgumentException($"Target file or folder '{path}' does not exist.");
        }
        private static void AssertDirectoryExists(string path)
        {
            bool dirExists = Directory.Exists(path);
            if (!dirExists)
                throw new ArgumentException($"Target directory '{path}' does not exist.");
        }
        private static void AssertFileExists(string path)
        {
            bool fileExists = File.Exists(path);
            if (!fileExists)
                throw new ArgumentException($"Target file '{path}' does not exist.");
        }


    }
}