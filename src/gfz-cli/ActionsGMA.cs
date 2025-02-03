using GameCube.GFZ.GMA;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

/// <summary>
///     Actions for modifying GMA files.
/// </summary>
public static class ActionsGMA
{
    private static readonly GfzCliArgument Name = new()
    {
        ArgumentName = IOptionsStage.Args.Name,
        ArgumentType = typeof(string).Name,
        ArgumentDefault = null,
        Help = "The model to modify.",
    };

    private static readonly GfzCliArgument Value = new()
    {
        ArgumentName = IOptionsLineRel.Args.Value,
        ArgumentType = typeof(RenderFlags).Name,
        ArgumentDefault = null,
        Help = "The model render flags to set.",
    };

    public static readonly GfzCliAction ActionGmaPatchSubmeshRenderFlags = new()
    {
        Description = "Patch render flags on model submesh.",
        Action = PatchSubmeshRenderFlags,
        ActionID = CliActionID.gma_patch_submesh_render_flags,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.PS,
        RequiredArguments = [ Name, Value ],
        OptionalArguments = [
            IOptionsStage.Arguments.SetFlagsOff,
            ],
    };


    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public static void PatchSubmeshRenderFlags(Options options)
    {
        // Maybe what you need is a function just to get IO paths...?
        int count = ParallelizeFileInFileOutTasks(options, PatchSubmeshRenderFlags);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    public static void PatchSubmeshRenderFlags(Options options, OSPath inputPath, OSPath _)
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
            PatchSubmeshRenderFlags(options, gmaFile, writer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="gma"></param>
    /// <param name="writer"></param>
    public static void PatchSubmeshRenderFlags(Options options, Gma gma, EndianBinaryWriter writer)
    {
        string name = options.Name;
        RenderFlags renderFlags = Options.GetEnum<RenderFlags>(options.Value);

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
