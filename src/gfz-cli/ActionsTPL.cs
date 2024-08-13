using GameCube.GFZ.TPL;
using GameCube.GX.Texture;
using Manifold.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;
using static Manifold.GFZCLI.GfzCliImageUtilities;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Text;
using SixLabors.ImageSharp.Formats;
using System.Linq;

namespace Manifold.GFZCLI
{
    public static class ActionsTPL
    {

        public static void TplUnpack(Options options)
        {
            // Force search TPL files IF there is no defined search pattern
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*.tpl";

            Terminal.WriteLine("TPL: unpacking file(s).");
            int taskCount = DoFileInFileOutTasks(options, TplUnpackFile);
            Terminal.WriteLine($"TPL: done unpacking {taskCount} TPL file{(taskCount != 1 ? 's' : "")}.");
        }
        public static void TplUnpackFile(Options options, OSPath inputFile, OSPath outputFile)
        {
            // Deserialize the TPL
            Tpl tpl = new Tpl();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), Tpl.endianness))
            {
                tpl.Deserialize(reader);
                tpl.FileName = Path.GetFileNameWithoutExtension(inputFile);
            }

            // Create folder named the same thing as the TPL input file
            string directory = Path.GetDirectoryName(outputFile);
            string outputDirectory = Path.Combine(directory, tpl.FileName);
            Directory.CreateDirectory(outputDirectory);

            // Prepare image encoder
            var encoder = new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression
            };
            outputFile.SetExtensions(".png");
            outputFile.PushDirectory(tpl.FileName);

            // Iterate over texture and mipmaps, save to disk
            int tplIndex = 0;
            foreach (var textureBundle in tpl.TextureBundles)
            {
                tplIndex++;

                if (textureBundle is null)
                    continue;

                int mipmapIndex = -1;
                int entryIndex = -1;
                foreach (var textureEntry in textureBundle.Elements)
                {
                    entryIndex++;

                    bool isMipmap = mipmapIndex > 0;
                    bool skipMipmaps = isMipmap && !options.TplUnpackMipmaps;
                    if (skipMipmaps)
                        continue;

                    mipmapIndex++;

                    // Optionally bow out of saving texture if it is corrupted.
                    bool skipCorruptedTexture = textureEntry.IsCorrupted && !options.TplUnpackSaveCorruptedTextures;
                    if (skipCorruptedTexture)
                        continue;

                    // TODO
                    // Use new FileDescription
                    var texture = textureEntry.Texture;
                    string textureHash = textureBundle.Elements[entryIndex].CRC32;
                    OSPath textureOutput = new OSPath(outputFile);
                    textureOutput.SetFileName($"{tplIndex}-{mipmapIndex}-{texture.Format}-{textureHash}");

                    //
                    var fileWrite = () =>
                    {
                        // Copy contents of GameCube texture into ImageSharp representation
                        var texture = textureEntry.Texture;
                        Image<Rgba32> image = TextureToImage(texture);
                        // Save to disk
                        image.Save(textureOutput, encoder);
                    };
                    var info = new FileWriteInfo()
                    {
                        InputFilePath = inputFile,
                        OutputFilePath = textureOutput,
                        PrintDesignator = "TPL",
                        PrintActionDescription = $"unpacking texture {tplIndex} mipmap {mipmapIndex} of file",
                    };
                    FileWriteOverwriteHandler(options, fileWrite, info);
                }
            }
        }

        public static void TplPack(Options options)
        {
            string path = options.InputPath;
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
                    Rgba32 pixel = image[x, y];
                    texture[x, y] = new TextureColor(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            string directory = Path.GetDirectoryName(path);
            string fileName = $"temp.tpl";
            string filePath = Path.Combine(directory, fileName);
            //using (var writer = new EndianBinaryWriter(new MemoryStream(), Tpl.endianness))
            using (var writer = new EndianBinaryWriter(File.Create(filePath), Tpl.endianness))
            {
                var encoding = new EncodingCMPR(BCnEncoder.Encoder.CompressionQuality.BestQuality);
                //var encoding = Encoding.EncodingRGBA8;
                //var encoding = Encoding.EncodingRGB565;
                //var encoding = Encoding.EncodingRGB5A3;
                //var encoding = Encoding.EncodingIA8;
                //var encoding = Encoding.EncodingIA4;
                var blocks = Texture.CreateDirectColorBlocksFromTexture(texture, encoding, out int bch, out int bcv);
                encoding.WriteTexture(writer, blocks);
                writer.Flush();

                writer.BaseStream.Position = 0;
                using (var reader = new EndianBinaryReader(writer.BaseStream, Tpl.endianness))
                {
                    var blocksCopy = encoding.ReadBlocks<DirectBlock>(reader, encoding, blocks.Length);
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

                    //var tempStream = new MemoryStream();
                    //var format = PngFormat.Instance;
                    //imageCopy.Save(tempStream, format);
                    //var imageHash = GetMD5Hastpl-packhName(tempStream);

                    // Find where to save file
                    directory = Path.GetDirectoryName(path);
                    fileName = $"temp.png";
                    filePath = Path.Combine(directory, fileName);
                    // Save to disk
                    imageCopy.SaveAsPng(filePath);
                    Terminal.WriteLine($"Wrote file: {filePath}");
                }
            }
        }


        public static void TplGenerateMipmaps(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*.png";

            Terminal.WriteLine("TPL: generating mipmaps.");
            int taskCount = DoFileInFileOutTasks(options, TplGenerateMipmaps);
            Terminal.WriteLine($"TPL: done generating mipmaps for {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }
        public static void TplGenerateMipmaps(Options options, OSPath inputFilePath, OSPath outputFilePath)
        {
            // Check to see if file can be loaded, error if not.
            using var sourceImage = File.OpenRead(inputFilePath);
            try
            {
                IImageFormat imageFormat = Image.DetectFormat(sourceImage);
            }
            catch (Exception e)
            {
                StringBuilder supportedTypes = new StringBuilder();
                foreach (var type in Enum.GetNames<ImageFormat>())
                    supportedTypes.Append($" {type}");

                string msg =
                    $"File {inputFilePath} is invalid {inputFilePath.Extension}. " +
                    $"Use supported types{supportedTypes}." +
                    $"\n{e.Message}";

                throw new ArgumentException(msg);
            }

            // Load image
            using var image = Image.Load(sourceImage);
            // Prepare file and encoding information
            IResampler resampler = options.Resampler;
            ImageEncoder imageEncoder = options.ImageEncoder;
            List<OSPath> mipmapNames = [outputFilePath];
            TplTextureName baseTextureName = new(outputFilePath.FileName);

            // Calculate number of texture levels (1 main tex + mipmap count)
            int numberOfLevels = 1;
            int resizeWidth = image.Width / 2;
            int resizeHeight = image.Height / 2;
            while (resizeWidth > 0 && resizeHeight > 0)
            {
                numberOfLevels++;
                resizeWidth >>= 1; // div by 2
                resizeHeight >>= 1; // div by 2
            }

            // Create mipmaps
            for (int mipmapLevel = 1; mipmapLevel < numberOfLevels; mipmapLevel++)
            {
                // Check to see if mipmap already exists, skip if so
                // NOTE: ignores 'name' and file extension
                TplTextureName searchPattern = new()
                {
                    TplIndex = baseTextureName.TplIndex,
                    TextureLevel = mipmapLevel,
                    TextureFormat = baseTextureName.TextureFormat,
                    Name = $"*",
                };
                string[] matches = Directory.GetFiles(outputFilePath.Directories, searchPattern);
                if (matches != null && matches.Length > 0)
                {
                    mipmapNames.Add(new());
                    continue;
                }

                // Compute w/h for this iteration
                resizeWidth = image.Width >> mipmapLevel;
                resizeHeight = image.Height >> mipmapLevel;

                // Create name for this mipmap based on main texture
                TplTextureName mipmapName = new()
                {
                    TplIndex = baseTextureName.TplIndex,
                    TextureLevel = mipmapLevel,
                    TextureFormat = baseTextureName.TextureFormat,
                    Name = "generated",
                };
                OSPath mipmapPath = outputFilePath.Copy();
                mipmapPath.SetFileName(mipmapName);
                mipmapNames.Add(mipmapPath);

                // Actual code which writes mipmaps
                void WriteMipmapFile()
                {
                    // Copy data local to function/thread
                    int _mipmapLevel = mipmapLevel;
                    int _resizeWidth = resizeWidth;
                    int _resizeHeight = resizeHeight;

                    // Create resized clone
                    var imageCopy = image.Clone(c => c.Resize(_resizeWidth, _resizeHeight, resampler));

                    // Save out mipmap
                    string mipmapName = mipmapNames[mipmapLevel];
                    using var mipmapFile = File.Create(mipmapName);
                    imageCopy.Save(mipmapFile, imageEncoder);
                }
                // Info when writing mipmaps out
                var info = new FileWriteInfo()
                {
                    InputFilePath = inputFilePath,
                    OutputFilePath = mipmapNames[mipmapLevel],
                    PrintDesignator = "TPL",
                    PrintActionDescription = $"generating mipmap level {mipmapLevel} of",
                };
                // Code that runs function, writes info, decides if functions runs (eg: allow overwrite)
                FileWriteOverwriteHandler(options, WriteMipmapFile, info);
            }
        }

    }
}
