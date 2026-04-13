using GameCube.GFZ.GMA;
using Manifold.IO;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for modifying GMA files.
/// </summary>
public static class ActionsGMA
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionGmaPatchSubmeshRenderFlags"/>
    /// </remarks>
    public static void PatchSubmeshRenderFlags(Options options)
    {
        // Maybe what you need is a function just to get IO paths...?
        int count = ParallelizeFileInFileOutTasks(options, PatchSubmeshRenderFlags);

        static void PatchSubmeshRenderFlags(Options options, OSPath inputPath, OSPath _)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Write file
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, inputPath, options.ActionStr);
            if (doWriteFile)
            {
                // Copy input to output if needed
                CreateBackupFileIfAble(options, inputPath);

                const FileMode fileMode = FileMode.OpenOrCreate;
                const FileAccess fileAccess = FileAccess.ReadWrite;
                const FileShare fileShare = FileShare.ReadWrite;

                // Read GMA
                GmaFile gmaFile = new(inputPath);
                // Patch GMA
                using EndianBinaryWriter writer = new(File.Open(inputPath, fileMode, fileAccess, fileShare), GmaFile.endianness);

                //
                Gma gma = gmaFile;
                string name = options.Name;
                RenderFlags renderFlags = GfzCliParser.GetEnum<RenderFlags>(options.Value);

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

}
