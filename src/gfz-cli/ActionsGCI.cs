﻿using GameCube.GCI;
using GameCube.GFZ.Replay;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public class ActionsGCI
    {
        public static void RenameGCI(Options options)
        {
            Terminal.WriteLine("GCI: converting emblems from BIN files.");
            int fileCount = DoFileInFileOutTasks(options, RenameGciFile);
            Terminal.WriteLine($"GCI: done renaming {fileCount} file{Plural(fileCount)}.");
        }

        public static void RenameGciFile(Options options, FilePath inputFilePath, FilePath outputFilePath)
        {
            Gci gci = new Gci();
            inputFilePath.ThrowIfDoesNotExist();
            using var reader = new EndianBinaryReader(File.OpenRead(inputFilePath), Gci.endianness);
            gci.Deserialize(reader);
            reader.SeekBegin();

            string name = GetName(gci.UniqueID, reader);
            outputFilePath.SetName(name);

            var fileWrite = () =>
            {
                File.Copy(inputFilePath, outputFilePath, options.OverwriteFiles);
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFilePath,
                OutputFilePath = outputFilePath,
                PrintDesignator = "GCI",
                PrintActionDescription = "renaming file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        public static string GetName(ushort uniqueID, EndianBinaryReader reader)
        {
            switch (uniqueID)
            {
                case ReplayGCI.UID:
                    var replay = new ReplayGCI();
                    replay.Deserialize(reader);
                    return "replay";

                default:
                    return "UNHANDLED";
            }
        }

    }
}
