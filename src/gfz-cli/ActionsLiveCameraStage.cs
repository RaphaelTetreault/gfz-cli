using GameCube.GFZ.Camera;
using Manifold.IO;
using System.IO;
using static Manifold.GFZCLI.GfzCliUtilities;

namespace Manifold.GFZCLI
{
    public static class ActionsLiveCameraStage
    {

        public static void LiveCameraStageToTsv(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*livecam_stage_*.bin";

            Terminal.WriteLine("Live Camera Stage: converting file(s) to TSV.");
            int taskCount = DoFileInFileOutTasks(options, LiveCameraStageBinToTsvFile);
            Terminal.WriteLine($"Live Camera Stage: done converting {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }

        public static void LiveCameraStageBinToTsvFile(Options options, OSPath inputFile, OSPath outputFile)
        {
            outputFile.SetExtensions(".tsv");

            // Deserialize the file
            LiveCameraStage liveCameraStage = new LiveCameraStage();
            using (var reader = new EndianBinaryReader(File.OpenRead(inputFile), LiveCameraStage.endianness))
            {
                liveCameraStage.Deserialize(reader);
                liveCameraStage.FileName = Path.GetFileNameWithoutExtension(inputFile);
            }

            //
            var fileWrite = () =>
            {
                // Write it to the stream
                using (var textWriter = new StreamWriter(File.Create(outputFile)))
                {
                    liveCameraStage.Serialize(textWriter);
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintPrefix = "LiveCam Stage",
                PrintActionDescription = "creating livecam_stage TSV from file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }

        public static void LiveCameraStageFromTsv(Options options)
        {
            bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
            if (hasNoSearchPattern)
                options.SearchPattern = "*livecam_stage_*.tsv";

            Terminal.WriteLine("Live Camera Stage: converting TSV file(s) to binaries.");
            int taskCount = DoFileInFileOutTasks(options, LiveCameraStageTsvToBinFile);
            Terminal.WriteLine($"Live Camera Stage: done converting {taskCount} file{(taskCount != 1 ? 's' : "")}.");
        }

        public static void LiveCameraStageTsvToBinFile(Options options, OSPath inputFile, OSPath outputFile)
        {
            outputFile.SetExtensions(".bin");

            // Load file
            LiveCameraStage liveCameraStage = new LiveCameraStage();
            using (var textReader = new StreamReader(File.OpenRead(inputFile)))
            {
                liveCameraStage.Deserialize(textReader);
                liveCameraStage.FileName = Path.GetFileNameWithoutExtension(inputFile);
            }

            // 
            var fileWrite = () =>
            {
                using (var writer = new EndianBinaryWriter(File.Create(outputFile), LiveCameraStage.endianness))
                {
                    liveCameraStage.Serialize(writer);
                }
            };
            var info = new FileWriteInfo()
            {
                InputFilePath = inputFile,
                OutputFilePath = outputFile,
                PrintPrefix = "LiveCam Stage",
                PrintActionDescription = "creating livecam_stage TSV from file",
            };
            FileWriteOverwriteHandler(options, fileWrite, info);
        }
    }
}
