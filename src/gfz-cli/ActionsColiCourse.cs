using GameCube.GFZ.Stage;
using Manifold.IO;
using System.IO;
using System.Numerics;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI;

/// <summary>
///     Actions for modifying COLI_COURSE files (stage/scene).
/// </summary>
public static class ActionsColiCourse
{
    private static readonly GfzCliArgument Value = new()
    {
        ArgumentName = IOptionsLineRel.Args.Value,
        ArgumentType = typeof(ObjectRenderFlags0x00).Name,
        ArgumentDefault = null,
        Help = "The render flag value in decimal to apply.",
    };

    public static readonly GfzCliAction ActionColicoursePatchFog = new()
    {
        Description = "Patch the fog parameters of scenes.",
        Action = PatchFog,
        ActionID = CliActionID.colicourse_patch_fog,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.FPRS,
        RequiredArguments = [
            IOptionsStage.Arguments.ColorRed,
            IOptionsStage.Arguments.ColorGreen,
            IOptionsStage.Arguments.ColorBlue,
            ],
        OptionalArguments = [
            IOptionsLineRel.Arguments.Backup,
            IOptionsStage.Arguments.FogInterpolationMode,
            IOptionsStage.Arguments.FogViewRangeNear,
            IOptionsStage.Arguments.FogViewRangeFar,
            ],
    };

    public static readonly GfzCliAction ActionColicoursePatchObjectRenderFlags = new()
    {
        Description = "Patch a scene object's render flags by name.",
        Action = PatchSceneObjectDynamicRenderFlags,
        ActionID = CliActionID.colicourse_patch_object_render_flags,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.FPRS,
        RequiredArguments = [
            IOptionsStage.Arguments.Name,
            Value,
            ],
        OptionalArguments = [
            IOptionsLineRel.Arguments.Backup,
            IOptionsStage.Arguments.SetFlagsOff,
            ],
    };

    /// <summary>
    ///     Patch the fog parameters of scenes.
    /// </summary>
    /// <param name="options"></param>
    public static void PatchFog(Options options)
    {
        int count = ParallelizeFileInFileOutTasks(options, PatchFog);
    }

    /// <summary>
    ///     Patch the fog parameters of a scene.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputPath"></param>
    /// <param name="_">Output path (unused).</param>
    public static void PatchFog(Options options, OSPath inputPath, OSPath _)
    {
        inputPath.ThrowIfFileDoesNotExist();

        // Patch COLI_COURSE file
        bool doWriteFile = CheckWillFileWrite(options, inputPath, out ActionTaskResult result);
        PrintFileWriteResult(result, inputPath, options.ActionStr);
        if (doWriteFile)
        {
            // Copy input to output if needed
            CreateBackupFileIfAble(options, inputPath);
            const FileMode fileMode = FileMode.OpenOrCreate;
            const FileAccess fileAccess = FileAccess.ReadWrite;
            const FileShare fileShare = FileShare.ReadWrite;
            using var colicourseFile = File.Open(inputPath, fileMode, fileAccess, fileShare);

            // Read data
            Scene scene = new Scene();
            scene.FileName = inputPath.FileName;
            using EndianBinaryReader reader = new(colicourseFile, Scene.endianness);
            scene.Deserialize(reader);
            reader.Close();

            // Reset file stream position
            colicourseFile.Position = 0;
            // Modify existin file (in the future, re-serialize file)
            using EndianBinaryWriter writer = new(colicourseFile, scene.Endianness);
            PatchFog(options, scene, writer);
        }
    }

    /// <summary>
    ///     Patch the fog parameters of a <paramref name="scene"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="scene"></param>
    /// <param name="writer"></param>
    public static void PatchFog(Options options, Scene scene, EndianBinaryWriter writer)
    {
        // OPTIONAL: Get parameters and defaults
        FogType fogInterpolationMode = (uint)options.FogInterpolationMode == uint.MaxValue
            ? scene.fog.Interpolation
            : options.FogInterpolationMode;
        float fogViewRangeNear = options.FogViewRangeNear == float.MaxValue
            ? scene.fog.FogRange.near
            : options.FogViewRangeNear;
        float fogViewRangeFar = options.FogViewRangeFar == float.MinValue
            ? scene.fog.FogRange.far
            : options.FogViewRangeFar;
        // Get color value
        byte r = options.ColorRed;
        byte g = options.ColorGreen;
        byte b = options.ColorBlue;

        // Create new fog
        Fog fog = new()
        {
            Interpolation = fogInterpolationMode,
            FogRange = new ViewRange(fogViewRangeNear, fogViewRangeFar),
            ColorRGB = new Vector3(r, g, b) / 255f,
        };
        // Create curves from values
        FogCurves fogCurves = fog.ToFogCurves();

        // Patch existing values
        writer.JumpToAddress(scene.fog.GetPointer());
        writer.Write(fog);
        // Fog curves
        {
            bool hasFogCurves = scene.fogCurves is not null;

            // Get pointer to data or create new pointer
            Pointer fogCurvesAnimationsPtr = hasFogCurves
                ? scene.fogCurves!.animationCurves[0].GetPointer()
                : (Pointer)writer.BaseStream.Length; // append to end of existing file
            writer.JumpToAddress(fogCurvesAnimationsPtr);
            // Write out each animation curve
            foreach (var animationCurve in fogCurves.animationCurves)
                writer.Write(animationCurve);

            // Get pointer to data or create new pointer
            Pointer fogCurvesPtr = hasFogCurves
                ? scene.fogCurves!.GetPointer()
                : (Pointer)writer.BaseStream.Length;
            // Write out fog curves (pointers to above animation data)
            writer.JumpToAddress(fogCurvesPtr);
            writer.Write(fogCurves);

            // Patch FogCurves address in header
            writer.JumpToAddress(0x80);
            writer.Write(fogCurvesPtr);
        }
    }

    /// <summary>
    ///     Patch an object named <see cref="Options.Name"/> scene objects'
    ///     <see cref="SceneObjectDynamic.ObjectRenderFlags0x00"/>.
    /// </summary>
    /// <param name="options"></param>
    public static void PatchSceneObjectDynamicRenderFlags(Options options)
    {
        int count = ParallelizeFileInFileOutTasks(options, PatchSceneObjectDynamicRenderFlags);
    }

    /// <summary>
    ///     Patch the <see cref="SceneObjectDynamic.ObjectRenderFlags0x00"/> of a an object named
    ///     <see cref="Options.Name"/> in a scene.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    public static void PatchSceneObjectDynamicRenderFlags(Options options, OSPath inputPath, OSPath outputPath)
    {
        inputPath.ThrowIfFileDoesNotExist();

        // Patch COLI_COURSE file
        bool doWriteFile = CheckWillFileWrite(options, inputPath, out ActionTaskResult result);
        PrintFileWriteResult(result, inputPath, options.ActionStr);
        if (doWriteFile)
        {
            // Make backup if desired, then open file
            CreateBackupFileIfAble(options, inputPath);
            const FileMode fileMode = FileMode.OpenOrCreate;
            const FileAccess fileAccess = FileAccess.ReadWrite;
            const FileShare fileShare = FileShare.ReadWrite;
            using var colicourseFile = File.Open(inputPath, fileMode, fileAccess, fileShare);

            Scene scene = new Scene();
            scene.FileName = inputPath.FileName;
            using EndianBinaryReader reader = new(colicourseFile, Scene.endianness);
            scene.Deserialize(reader);
            reader.Close();

            // Reset file stream position
            colicourseFile.Position = 0;
            // Modify existing file (in the future, re-serialize file)
            using EndianBinaryWriter writer = new(colicourseFile, Scene.endianness);
            PatchSceneObjectDynamicRenderFlags(options, scene, writer);
        }
    }

    /// <summary>
    ///     Patch the <see cref="SceneObjectDynamic.ObjectRenderFlags0x00"/> of a an object named
    ///     <see cref="Options.Name"/> in a <paramref name="scene"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="scene"></param>
    /// <param name="writer"></param>
    public static void PatchSceneObjectDynamicRenderFlags(Options options, Scene scene, EndianBinaryWriter writer)
    {
        string name = options.Name;
        ObjectRenderFlags0x00 renderFlags = options.GetEnum<ObjectRenderFlags0x00>(options.Value);

        bool foundMatch = false;
        foreach (SceneObjectDynamic dynamicSceneObject in scene.dynamicSceneObjects)
        {
            if (dynamicSceneObject.Name != name)
                continue;

            foundMatch = true;

            if (options.SetFlagsOff)
                dynamicSceneObject.ObjectRenderFlags0x00 &= ~renderFlags;
            else // set flags on
                dynamicSceneObject.ObjectRenderFlags0x00 |= renderFlags;

            Pointer ptr = dynamicSceneObject.GetPointer();
            writer.JumpToAddress(ptr);
            writer.Write(dynamicSceneObject);
        }

        // TODO: make a better message, use color. Add occurrence count?
        if (!foundMatch)
        {
            Terminal.WriteLine($"Did not find match for \"{name}\"", Program.WarningColor);
        }
    }
}
