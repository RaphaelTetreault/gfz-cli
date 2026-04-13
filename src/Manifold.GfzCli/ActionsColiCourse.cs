using GameCube.GFZ.Stage;
using Manifold.IO;
using System.IO;
using System.Numerics;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for modifying COLI_COURSE files (stage/scene).
/// </summary>
public static class ActionsColiCourse
{
    /// <summary>
    ///     Patch the fog parameters of scenes.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionColicoursePatchFog"/>
    /// </remarks>
    public static void PatchFog(Options options)
    {
        int count = ParallelizeFileInFileOutTasks(options, PatchFogIO);
        static void PatchFogIO(Options options, OSPath inputPath, OSPath _)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Patch COLI_COURSE file
            options.OverwriteFiles = true;
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, inputPath, options.ActionStr);
            if (doWriteFile)
            {
                // Copy input to output if needed
                CreateBackupFileIfAble(options, inputPath);

                // Open stream to modify file
                const FileMode fileMode = FileMode.OpenOrCreate;
                const FileAccess fileAccess = FileAccess.ReadWrite;
                const FileShare fileShare = FileShare.ReadWrite;
                using var colicourseFile = File.Open(inputPath, fileMode, fileAccess, fileShare);
                using EndianBinaryWriter writer = new(colicourseFile, SceneFile.endianness);

                // Read data in new stream
                Scene scene = new SceneFile(inputPath);

                // Modify existing file (in the future, re-serialize file)
                // OPTIONAL: Get parameters and defaults
                FogType fogInterpolationMode = (uint)options.FogInterpolationMode == GfzCliArgumentDB.FogInterpolationMode.Default<uint>()
                    ? scene.fog.Interpolation
                    : options.FogInterpolationMode;
                float fogViewRangeNear = options.FogViewRangeNear == GfzCliArgumentDB.FogViewRangeNear.Default<float>()
                    ? scene.fog.FogRange.near
                    : options.FogViewRangeNear;
                float fogViewRangeFar = options.FogViewRangeFar == GfzCliArgumentDB.FogViewRangeFar.Default<float>()
                    ? scene.fog.FogRange.far
                    : options.FogViewRangeFar;
                // Get color value from either components or single color
                byte r = options.UnionColorR;
                byte g = options.UnionColorG;
                byte b = options.UnionColorB;

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
        }
    }

    /// <summary>
    ///     Patch the <see cref="SceneObjectDynamic.ObjectRenderFlags0x00"/> of a an object named
    ///     <see cref="Options.Name"/> in a scene.
    /// </summary>
    /// <param name="options"></param>
    /// <remarks>
    ///     Action: <see cref="GfzCliActionDB.ActionColicoursePatchObjectRenderFlags"/>
    /// </remarks>
    public static void PatchSceneObjectDynamicRenderFlags(Options options)
    {
        int count = ParallelizeFileInFileOutTasks(options, PatchSceneObjectDynamicRenderFlagsIO);
        static void PatchSceneObjectDynamicRenderFlagsIO(Options options, OSPath inputPath, OSPath outputPath)
        {
            inputPath.ThrowIfFileDoesNotExist();

            // Patch COLI_COURSE file
            bool doWriteFile = CheckWillFileWrite(options, inputPath, out ActionTaskResult result);
            PrintFileWriteResult(result, inputPath, options.ActionStr);
            if (doWriteFile)
            {
                // Make backup if desired, then open file
                CreateBackupFileIfAble(options, inputPath);

                // Open stream to modify file
                const FileMode fileMode = FileMode.OpenOrCreate;
                const FileAccess fileAccess = FileAccess.ReadWrite;
                const FileShare fileShare = FileShare.ReadWrite;
                using var colicourseFile = File.Open(inputPath, fileMode, fileAccess, fileShare);
                using EndianBinaryWriter writer = new(colicourseFile, SceneFile.endianness);

                // Read data in new stream
                Scene scene = new SceneFile(inputPath);

                // Modify existing file (in the future, re-serialize file)
                string name = options.Name;
                ObjectRenderFlags0x00 renderFlags = GfzCliParser.GetEnum<ObjectRenderFlags0x00>(options.Value);

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
                    Terminal.WriteLine($"Did not find match for \"{name}\"", GfzCli.WarningColor);
                }
            }
        }
    }

}
