using GameCube.GFZ.Replay;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsMisc
    {
        public static void DumpHex32(Options options)
        {
            var inputFilePaths = GetInputFiles(options);
            var readers = new EndianBinaryReader[inputFilePaths.Length];
            for (int i = 0; i < readers.Length; i++)
            {
                readers[i] = new EndianBinaryReader(File.OpenRead(inputFilePaths[i]), Endianness.BigEndian);
            }

            string outputPath = GetOutputDirectory(options);
            FilePath fileOutputPath = new FilePath(outputPath);
            fileOutputPath.SetName("test");
            fileOutputPath.PushExtension("tsv");
            using var writer = new StreamWriter(File.Create(fileOutputPath));

            writer.WriteNextCol("Filename:");
            for (int i = 0; i < inputFilePaths.Length; i++)
            {
                string name = Path.GetFileNameWithoutExtension(inputFilePaths[i]);
                writer.WriteNextCol(name);
            }
            writer.WriteNextRow();

            int address = 0;
            int streamsCompleted = 0;
            while (streamsCompleted < readers.Length)
            {
                streamsCompleted = 0;
                for (int i = 0; i < readers.Length; i++)
                {
                    // Write address
                    if (i == 0)
                        writer.WriteNextCol($"0x{address:x4}");

                    // Only write if able
                    var reader = readers[i];
                    if (reader.IsAtEndOfStream())
                    {
                        streamsCompleted++;
                        writer.WriteNextCol();
                        continue;
                    }

                    // Write data
                    var value = reader.ReadUInt16();
                    writer.WriteNextCol($"0x{value:x4}");
                    // hack
                    address = reader.GetPositionAsPointer();

                    // End line
                    if (i == readers.Length - 1)
                        writer.WriteNextRow();
                }
            }

            foreach (var reader in readers)
                reader.Close();
        }
    }
}
