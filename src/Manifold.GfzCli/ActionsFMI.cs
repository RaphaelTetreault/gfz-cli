using GameCube.GFZ.FMI;
using Manifold.IO;
using System.IO;
using static Manifold.GfzCli.GfzCliUtilities;

namespace Manifold.GfzCli;

/// <summary>
///     Actions for working with FMI files (vehicle booster particle emitters).
/// </summary>
public static class ActionsFMI
{
    const string prefix = "FMI";

    public static readonly GfzCliAction ActionFmiFromPlainText = new()
    {
        Description = "Create a FMI-plaintext file from FMI binary file.",
        Action = FmiFromPlainText,
        ActionID = CliActionID.fmi_from_plaintext,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly GfzCliAction ActionFmiToPlainText = new()
    {
        Description = "Create a FMI binary file from FMI-plaintext.",
        Action = FmiToPlainText,
        ActionID = CliActionID.fmi_to_plaintext,
        InputIO = CliActionIO.Path,
        OutputIO = CliActionIO.Path,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.OPS,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    public static void FmiToPlainText(Options options)
    {
        options.OverrideSearchPatternIfUnset("*.fmi");
        Terminal.WriteLine($"{prefix}: converting FMI to plain text files.");
        int binCount = ParallelizeFileInFileOutTasks(options, FmiToPlainText);
        Terminal.WriteLine($"{prefix}: done converting {binCount} file{Plural(binCount)}.");
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    public static void FmiFromPlainText(Options options)
    {
        options.OverrideSearchPatternIfUnset("*.fmi.txt");
        Terminal.WriteLine($"{prefix}: converting FMI from plain text files.");
        int binCount = ParallelizeFileInFileOutTasks(options, FmiFromPlainText);
        Terminal.WriteLine($"{prefix}: done converting {binCount} file{Plural(binCount)}.");
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    private static void FmiToPlainText(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Set output extensions
        outputFile.SetExtensions(".fmi.txt");

        // Write file
        bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
        PrintFileWriteResult(result, outputFile, options.ActionStr);
        if (doWriteFile)
        {
            // Read data
            FmiFile fmiFile = new(inputFile);
            // Write to file
            using PlainTextWriter writer = new(outputFile);
            fmiFile.Value.Serialize(writer);
            writer.Flush();
        }
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="options"></param>
    /// <param name="inputFile"></param>
    /// <param name="outputFile"></param>
    private static void FmiFromPlainText(Options options, OSPath inputFile, OSPath outputFile)
    {
        // Set output extension
        outputFile.SetExtensions(".fmi");

        // Write file
        bool doWriteFile = CheckWillFileWrite(options, outputFile, out ActionTaskResult result);
        PrintFileWriteResult(result, outputFile, options.ActionStr);
        if (doWriteFile)
        {
            // Read data
            FmiFile fmiFile = new();
            using PlainTextReader reader = new(inputFile);
            fmiFile.Value.Deserialize(reader);
            // Write to file
            fmiFile.WriteFile(outputFile);
        }
    }

}
