using GameCube.GFZ.Stage;
using Manifold.IO;
using System.IO;
using System.Numerics;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsColiCourse
    {
        public static void PatchFog(Options options)
        {
            //Terminal.WriteLine("FMI: converting FMI from plain text files.");
            int count = DoFileInFileOutTasks(options, PatchFog);
            //Terminal.WriteLine($"FMI: done converting {binCount} file{Plural(binCount)}.");
        }
        public static void PatchFog(Options options, OSPath inputPath, OSPath outputPath)
        {
            inputPath.ThrowIfFileDoesNotExist();

            var fileWrite = () =>
            {
                // Copy input to output if needed
                CopyInputToOutputIfNotSamePath(inputPath, outputPath);
                const FileMode fileMode = FileMode.OpenOrCreate;
                const FileAccess fileAccess = FileAccess.ReadWrite;
                const FileShare fileShare = FileShare.ReadWrite;

                // Read data
                Scene scene = new Scene();
                scene.FileName = inputPath.FileName;
                using EndianBinaryReader reader = new(File.Open(inputPath, fileMode, fileAccess, fileShare), Scene.endianness);
                scene.Deserialize(reader);
                reader.Close();

                // Write (patch) to file
                using EndianBinaryWriter writer = new(File.Open(outputPath, fileMode, fileAccess, fileShare), scene.Endianness);

                // Modify existin file (in the future, re-serialize file)
                PatchFog(options, scene, writer);
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputPath,
                OutputFilePath = outputPath,
                PrintDesignator = "COLICOURSE",
                PrintActionDescription = $"patch scene fog in",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
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
                    : writer.BaseStream.Length;
                writer.JumpToAddress(fogCurvesAnimationsPtr);
                // Write out each animation curve
                foreach (var animationCurve in fogCurves.animationCurves)
                    writer.Write(animationCurve);

                // Get pointer to data or create new pointer
                Pointer fogCurvesPtr = hasFogCurves
                    ? scene.fogCurves!.GetPointer()
                    : writer.BaseStream.Length;
                // Write out fog curves (pointers to above animation data)
                writer.JumpToAddress(fogCurvesPtr);
                writer.Write(fogCurves);

                // Patch FogCurves address in header
                writer.JumpToAddress(0x80);
                writer.Write(fogCurvesPtr);
            }
        }

        public static void PatchSceneObjectDynamicRenderFlags(Options options)
        {
            int count = DoFileInFileOutTasks(options, PatchSceneObjectDynamicRenderFlags);
        }
        public static void PatchSceneObjectDynamicRenderFlags(Options options, OSPath inputPath, OSPath outputPath)
        {
            inputPath.ThrowIfFileDoesNotExist();

            var fileWrite = () =>
            {
                // Read data
                Scene scene = new Scene();
                scene.FileName = inputPath.FileName;
                using EndianBinaryReader reader = new(File.OpenRead(inputPath), Scene.endianness);
                scene.Deserialize(reader);
                reader.Close();

                // Write (patch) to file
                using EndianBinaryWriter writer = new(File.OpenWrite(inputPath), scene.Endianness);

                // Modify existin file (in the future, re-serialize file)
                PatchSceneObjectDynamicRenderFlags(options, scene, writer);
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputPath,
                OutputFilePath = outputPath,
                PrintDesignator = "COLICOURSE",
                PrintActionDescription = $"patch dynamic scene object render flags",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
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

                if (!options.SetFlagsOff)
                    dynamicSceneObject.ObjectRenderFlags0x00 |= renderFlags;
                else
                    dynamicSceneObject.ObjectRenderFlags0x00 &= ~renderFlags;

                Pointer ptr = dynamicSceneObject.GetPointer();
                writer.JumpToAddress(ptr);
                writer.Write(dynamicSceneObject);
            }

            // TODO: make a better message, use color. Add occurrence count?
            if (!foundMatch)
            Terminal.WriteLine($"Did not find match for {name}");
        }

    }
}
