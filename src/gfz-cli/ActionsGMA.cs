using GameCube.GFZ.GMA;
using GameCube.GFZ.Stage;
using Manifold.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsGMA
    {
        public static void PatchSubmeshRenderFlags(Options options)
        {
            // Maybe what you need is a function just to get IO paths...?
            int count = DoFileInFileOutTasks(options, PatchSubmeshRenderFlags);
        }

        public static void PatchSubmeshRenderFlags(Options options, OSPath inputPath, OSPath outputPath)
        {
            inputPath.ThrowIfFileDoesNotExist();

            var fileWrite = () =>
            {
                // Copy input to output if needed
                CopyInputToOutputIfNotSamePath(inputPath, outputPath);

                const FileMode fileMode = FileMode.OpenOrCreate;
                const FileAccess fileAccess = FileAccess.ReadWrite;
                const FileShare fileShare = FileShare.ReadWrite;

                // Read GMA
                Gma gma = new Gma();
                using EndianBinaryReader reader = new(File.Open(inputPath, fileMode, fileAccess, fileShare), Gma.endianness);
                gma.Deserialize(reader);

                // Patch GMA
                using EndianBinaryWriter writer = new(File.Open(outputPath, fileMode, fileAccess, fileShare), Gma.endianness);
                PatchSubmeshRenderFlags(options, gma, writer);
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputPath,
                OutputFilePath = outputPath,
                PrintPrefix = "COLICOURSE",
                PrintActionDescription = $"patch scene fog in",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        public static void PatchSubmeshRenderFlags(Options options, Gma gma, EndianBinaryWriter writer)
        {
            string name = options.Name;
            RenderFlags renderFlags = options.GetEnum<RenderFlags>(options.Value);

            int countMatches = 0;
            foreach (Model model in gma.Models)
            {
                if (model.Name != name)
                    continue;

                countMatches++;
                Terminal.WriteLine(model.Name);

                foreach (var submesh in model.Gcmf.Submeshes)
                {
                    if (options.SetFlagsOff)
                        submesh.RenderFlags &= ~renderFlags;
                    else // set flags on
                        submesh.RenderFlags |= renderFlags;

                    Pointer ptr = submesh.GetPointer() + 0; // RenderFlags offset is 0 bytes
                    writer.JumpToAddress(ptr);
                    writer.Write(submesh.RenderFlags);
                }
            }

            // TODO: make a better message, use color.
            if (countMatches <= 0)
                Terminal.WriteLine($"Did not find match for {name}");
            else
                Terminal.WriteLine($"Matches for {name}: {countMatches}");

        }
    }
}
