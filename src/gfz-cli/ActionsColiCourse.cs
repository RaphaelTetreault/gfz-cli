﻿using GameCube.GFZ.Stage;
using Manifold.IO;
using System.IO;
using Unity.Mathematics;
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
        public static void PatchFog(Options options, FilePath inputPath, FilePath outputPath)
        {
            inputPath.ThrowIfDoesNotExist();

            var fileWrite = () =>
            {
                // Read data
                Scene scene = new Scene();
                scene.FileName = inputPath.Name;
                using EndianBinaryReader reader = new(File.OpenRead(inputPath), Scene.endianness);
                scene.Deserialize(reader);
                reader.Close();

                // Write (patch) to file
                using EndianBinaryWriter writer = new(File.OpenWrite(inputPath), scene.Endianness);

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
            FogType fogInterpolationMode = (uint)options.FogInterpolationMode == 0xFFFFFFFF
                ? scene.fog.Interpolation
                : options.FogInterpolationMode;
            float fogViewRangeNear = options.FogViewRangeNear < 0
                ? scene.fog.FogRange.near
                : options.FogViewRangeNear;
            float fogViewRangeFar = options.FogViewRangeFar < 0
                ? scene.fog.FogRange.far
                : options.FogViewRangeFar;
            // Get color value
            byte r = options.ColorRed;
            byte g = options.ColorGreen;
            byte b = options.ColorBlue;

            // Create new fog
            Fog fog = new();
            fog.Interpolation = fogInterpolationMode;
            fog.FogRange = new ViewRange(fogViewRangeNear, fogViewRangeFar);
            fog.ColorRGB = new float3(r, g, b) / 255f;
            // Create curves from values
            FogCurves fogCurves = fog.ToFogCurves();

            // Patch existing file
            writer.JumpToAddress(scene.fog.GetPointer());
            writer.Write(fog);
            // Fog curves
            {
                bool hasFogCurves = scene.fogCurves is not null;

                // Write anim data
                Pointer fogCurvesAnimationsPtr = hasFogCurves
                    ? scene.fogCurves!.animationCurves[0].GetPointer()
                    : writer.BaseStream.Length;
                writer.JumpToAddress(fogCurvesAnimationsPtr);
                // Write out each animation curve
                foreach (var animationCurve in fogCurves.animationCurves)
                    writer.Write(animationCurve);

                // 
                Pointer fogCurvesPtr = hasFogCurves
                    ? scene.fogCurves!.GetPointer()
                    : writer.BaseStream.Length;
                writer.JumpToAddress(fogCurvesPtr);
                writer.Write(fogCurves);

                // Patch address in header
                writer.JumpToAddress(0x80);
                writer.Write(fogCurvesPtr);
            }

        }

        public static void PatchSceneObjectDynamicRenderFlags(Options options)
        {
            int count = DoFileInFileOutTasks(options, PatchSceneObjectDynamicRenderFlags);
        }
        public static void PatchSceneObjectDynamicRenderFlags(Options options, FilePath inputPath, FilePath outputPath)
        {
            inputPath.ThrowIfDoesNotExist();

            var fileWrite = () =>
            {
                // Read data
                Scene scene = new Scene();
                scene.FileName = inputPath.Name;
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

            foreach (SceneObjectDynamic dynamicSceneObject in scene.dynamicSceneObjects)
            {
                if (dynamicSceneObject.Name != name)
                    continue;

                if (options.SetFlagsOn)
                    dynamicSceneObject.ObjectRenderFlags0x00 |= renderFlags;
                else
                    dynamicSceneObject.ObjectRenderFlags0x00 &= ~renderFlags;

                Pointer ptr = dynamicSceneObject.GetPointer();
                writer.JumpToAddress(ptr);
                writer.Write(dynamicSceneObject);
            }
        }

    }
}
