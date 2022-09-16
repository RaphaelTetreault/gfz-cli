using CommandLine;
using GameCube.GFZ.CarData;
using Manifold;
using Manifold.IO;
using System;
using System.IO;


namespace Manifold.GFZCLI
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
        }

        public static void RunOptions(Options options)
        {
            if (options.Verbose)
            {
                options.PrintState();
                Console.WriteLine();
            }
            VerboseConsole.IsVerbose = options.Verbose;

            // Everythign else from here
            CarDataToTSV(options);
            CarDataToBIN(options);
        }

        public static void CarDataToTSV(Options options)
        {
            if (string.IsNullOrEmpty(options.CarDataBinPath))
                return;

            using (var reader = new EndianBinaryReader(File.OpenRead(options.CarDataBinPath), CarData.endianness))
            {
                var carData = new CarData();
                reader.Read(ref carData);

                string outputPath = options.CarDataBinPath + ".tsv";
                using (var writer = new StreamWriter(File.Create(outputPath)))
                {
                    carData.Serialize(writer);
                }
            }
        }

        public static void CarDataToBIN(Options options)
        {
            if (string.IsNullOrEmpty(options.CarDataTsvPath))
                return;

            using (var reader = new StreamReader(File.OpenRead(options.CarDataTsvPath)))
            {
                var carData = new CarData();
                carData.Deserialize(reader);

                string outputPath = options.CarDataTsvPath + ".bin";
                using (var writer = new EndianBinaryWriter(File.Create(outputPath), CarData.endianness))
                {
                    carData.Serialize(writer);
                }
            }
        }

    }
}