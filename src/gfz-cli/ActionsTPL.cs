using GameCube.GFZ.TPL;
using GameCube.GX.Texture;
using Manifold.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;
using static Manifold.GFZCLI.GfzCliImageUtilities;

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

        public static void TplUnpackFile(Options options, FilePath inputFile, FilePath outputFile)
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
            outputFile.AppendDirectory(tpl.FileName);

            // Iterate over texture and mipmaps, save to disk
            int tplIndex = 0;
            foreach (var textureSeries in tpl.TextureSeries)
            {
                tplIndex++;

                if (textureSeries is null)
                    continue;

                int mipmapIndex = 0;
                int entryIndex = -1;
                foreach (var textureEntry in textureSeries.Entries)
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
                    string textureHash = textureSeries.MD5TextureHashes[entryIndex];
                    FilePath textureOutput = new FilePath(outputFile);
                    textureOutput.SetName($"{tplIndex}-{mipmapIndex}-{texture.Format}-{textureHash}");

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
    }
}
