using GameCube.GFZ.GameData;
using System;

namespace Manifold.GfzCli;

internal class CliArgumentAsserts
{
    internal static void AssertNameIsNotNullOrWhitespace(Options options)
    {
        if (string.IsNullOrWhiteSpace(options.Name))
        {
            string msg = $"Argument --{CliArgumentText.Name} must be set.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertNameIsNotNull(Options options)
    {
        if (options.Name is null)
        {
            string msg = $"Argument --{CliArgumentText.Name} must be set.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertValueExists(Options options)
    {
        if (string.IsNullOrEmpty(options.Value))
        {
            string msg = $"Argument --{CliArgumentText.Value} must be set.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertBgmIndex(Options options)
    {
        // Consts
        const int maxBgmIndex = (byte)BgmIndex.metadata_invalid_id_start;
        const int bgmExceptionIndex = (byte)BgmIndex.metadata_random;
        // Validate
        bool isValidIndex = options.BgmIndex.Byte <= maxBgmIndex;
        bool isValidException = options.BgmIndex.Byte == bgmExceptionIndex;
        bool isInvalid = !(isValidIndex || isValidException);
        if (isInvalid)
        {
            string msg =
                $"Argument --{nameof(CliArgumentText.BgmIndex)} " +
                $"must be a value in the range {0}-{maxBgmIndex}. " +
                $"Value was {(int)options.BgmIndex} \"{options.BgmIndex}\".";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertBgmFinalLapIndex(Options options)
    {
        bool isInvalid = (BgmIndex)options.BgmFinalLapIndex switch
        {
            // *_b are the final lap songs
            BgmIndex.casino_b or
            BgmIndex.elev_b or
            BgmIndex.fire_b or
            BgmIndex.forest_b or
            BgmIndex.lightning_b or
            BgmIndex.meteor_b or
            BgmIndex.mutecity_b or
            BgmIndex.ocean_b or
            BgmIndex.ptown_b or
            BgmIndex.rainbow_b or
            BgmIndex.sand_b or
            BgmIndex.tower_b => false,
            // 0xFF is valid, means no final lap song
            BgmIndex.metadata_random => false,
            // Anything else is not a final lap BGM
            _ => true,
        };

        if (isInvalid)
        {
            BgmIndex[] validValues = [
                BgmIndex.casino_b,   BgmIndex.elev_b,      BgmIndex.fire_b,
                BgmIndex.forest_b,   BgmIndex.lightning_b, BgmIndex.meteor_b,
                BgmIndex.mutecity_b, BgmIndex.ocean_b,     BgmIndex.ptown_b,
                BgmIndex.rainbow_b,  BgmIndex.sand_b,      BgmIndex.tower_b,
                BgmIndex.metadata_random,
            ];
            Terminal.WriteLine($"Valid --{nameof(CliArgumentText.CupCourseIndex)} values:");
            foreach (BgmIndex value in validValues)
                Terminal.WriteLine($"{value} {(byte)value}");
            string msg =
                $"Argument --{nameof(CliArgumentText.CupCourseIndex)} " +
                $"must be a valid value. Value was \"{options.CupCourseIndex}\".";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertCourseIndex(Options options)
    {
        if (options.CourseIndex > GameDataConsts.MaxCourseIndex)
        {
            string msg = $"Argument --{CliArgumentText.CourseIndex} must be a value in the range 0-{GameDataConsts.MaxCourseIndex}. " +
                $"Value is {options.CourseIndex}.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertCourseIndexAllow0xFFFF(Options options)
    {
        bool isValidIndex = options.CourseIndex <= GameDataConsts.MaxCourseIndex;
        bool isValidException = options.CourseIndex == Course.UnassignedCourseIndex;
        bool isInvalid = !(isValidIndex || isValidException);
        if (isInvalid)
        {
            string msg =
                $"Argument --{CliArgumentText.CourseIndex} " +
                $"must be a value in the range 0-{GameDataConsts.MaxCourseIndex} or exactly {Course.UnassignedCourseIndex}. " +
                $"Value is {options.CourseIndex}.";
            throw new Exception(msg);
        }
    }

    internal static void AssertCup(Options options)
    {
        if (!Enum.IsDefined(options.Cup))
        {
            string msg =
                $"Argument --{CliArgumentText.Cup} must be in the range {(int)CupIndex.RubyCup} {(int)CupIndex.E3_Versus}. " +
                $"Value is {(int)options.Cup}.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertCupCourseIndex(Options options)
    {
        const int minCupCourseIndex = 1;
        if (options.CupCourseIndex < minCupCourseIndex || options.CupCourseIndex > GameDataConsts.MaxCupCourseIndex)
        {
            string msg =
                $"Argument --{nameof(CliArgumentText.CupCourseIndex)} " +
                $"must be a value in the range {minCupCourseIndex}-{GameDataConsts.MaxCupCourseIndex}.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertDifficultyStars(Options options)
    {
        if (options.Difficulty > GameDataConsts.MaxDifficultyStars)
        {
            string msg = $"Argument --{CliArgumentText.Difficulty} must a value in the range 0-{GameDataConsts.MaxDifficultyStars}. " +
                $"Value is {options.Difficulty}.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertPilotNumber(Options options)
    {
        const int minPilotNumber = 0; // Deathborn
        const int maxPilotNumber = 40; // Rainbow Pheonix
        if (options.PilotNumber < minPilotNumber || options.CupCourseIndex > maxPilotNumber)
        {
            string msg =
                $"Argument --{nameof(CliArgumentText.PilotNumber)} " +
                $"must be a value in the range {minPilotNumber}-{maxPilotNumber}.";
            throw new ArgumentException(msg);
        }
    }

    internal static void AssertVenueIndex(Options options)
    {
        if (options.VenueIndex.Byte > GameDataConsts.MaxVenueIndex)
        {
            string msg =
                $"Argument --{CliArgumentText.VenueIndex} must be a value in the range 0-{GameDataConsts.MaxVenueIndex}. " +
                $"Value is {options.VenueIndex}.";
            throw new ArgumentException(msg);
        }
    }

}
