using CommandLine;
using Manifold.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manifold.GFZCLI;

public static class GfzCli
{
    public const ConsoleColor FileNameColor = ConsoleColor.Cyan;
    public const ConsoleColor FileWriteColor = ConsoleColor.Green;
    public const ConsoleColor FileOverwriteColor = ConsoleColor.DarkYellow;
    public const ConsoleColor FileOverwriteSkipColor = ConsoleColor.Red;
    public const ConsoleColor SubTaskColor = ConsoleColor.DarkGray;
    public const ConsoleColor WarningColor = ConsoleColor.Red;
    public const ConsoleColor NotificationColor = ConsoleColor.DarkYellow;
    public static readonly string[] HelpArg = ["--help"];

    private static readonly GfzCliAction ActionUsage = new()
    {
        Description = "Call to print out actions available and how to use them.",
        Action = PrintActionUsage,
        ActionID = CliActionID.usage,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    private static readonly GfzCliAction ActionList = new()
    {
        Description = "List all possible actions with description.",
        Action = PrintActionList,
        ActionID = CliActionID.list,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    private static readonly GfzCliAction ActionNone = new()
    {
        Description = "No action selected.",
        Action = PrintActionUsage,
        ActionID = CliActionID.none,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };

    public static readonly Dictionary<CliActionID, GfzCliAction> GfzCliActionsLibrary = [];
    public static readonly GfzCliAction[] GfzCliActions =
    [
        // PROGRAM-SPECIFIC: note actions in this class need to be initialzed before this code runs...
        ActionNone,
        ActionList,
        ActionUsage,
        // ARC
        ActionsARC.ActionArcPack,
        ActionsARC.ActionArcUnpack,
        // ASSET LIBRARY
        ActionsAsset.ActionAssetGenerateLibrary,
        ActionsAsset.ActionAssetCustomMipmapGxtex,
        ActionsAsset.ActionAssetGmarefPack,
        ActionsAsset.ActionAssetImageToGxtex,
        ActionsAsset.ActionAssetTplPack,
        ActionsAsset.ActionAssetTplUnpack,
        // CARDATA
        ActionsCarData.ActionCarDataFromTSV,
        ActionsCarData.ActionCarDataToTSV,
        // COLICOURSE
        ActionsColiCourse.ActionColicoursePatchFog,
        ActionsColiCourse.ActionColicoursePatchObjectRenderFlags,
        //      TODO: Extract ./files/ only
        //      TODO: Extract ./sys/ only
        // ENCODE TEXT
        ActionsEncodeText.ActionEncodeBytesToShiftJis,
        ActionsEncodeText.ActionEncodeWindows1252ToShiftJis,
        // EMBLEM
        ActionsEmblem.ActionEmblemGciFromImage,
        ActionsEmblem.ActionEmblemGciToImage,
        ActionsEmblem.ActionEmblemsBinFromImages,
        ActionsEmblem.ActionEmblemsBinToImages,
        // FMI
        ActionsFMI.ActionFmiFromPlainText,
        ActionsFMI.ActionFmiToPlainText,
        // GCI
        ActionsGhost.ActionGciExtractGhost,
        //      TODO: ActionsGCI - rename file...
        // GMA
        ActionsGMA.ActionGmaPatchSubmeshRenderFlags,
        // ISO
        ActionsISO.ActionIsoExtractAll,
        // IO: IN-OUT TESTS
        ActionsIO.ActionIOGma,
        ActionsIO.ActionIOScene,
        ActionsIO.ActionIOSceneAddComment,
        ActionsIO.ActionIOTpl,
        // line__.rel
        ActionsLineREL.ActionDecryptLineREL,
        ActionsLineREL.ActionEncryptLineREL,
        ActionsLineREL.ActionPatchClearAllCourseNames,
        ActionsLineREL.ActionPatchClearAllVenueNames,
        ActionsLineREL.ActionPatchClearUnusedCourseNames,
        ActionsLineREL.ActionPatchClearUnusedVenueNames,
        ActionsLineREL.ActionPatchBgm,
        ActionsLineREL.ActionPatchBgmFinalLap,
        ActionsLineREL.ActionPatchBgmBoth,
        ActionsLineREL.ActionPatchSetCarData,
        ActionsLineREL.ActionPatchSetCourseName,
        ActionsLineREL.ActionPatchSetCupCourse,
        ActionsLineREL.ActionPatchMachineRating,
        ActionsLineREL.ActionPatchMaxSpeed,
        ActionsLineREL.ActionPatchSetCourseVenue,
        ActionsLineREL.ActionPatchSetVenueName,
        // Log
        ActionsLog.ActionLogStage,
        ActionsLog.ActionLogGma,
        // LZ
        ActionsLZ.ActionLZCompress,
        ActionsLZ.ActionLZDecompress,
    ];

    private static void InitUsageDictionary()
    {
        // Prevent running twice
        if (GfzCliActionsLibrary.Count != 0)
            return;

        foreach (GfzCliAction value in GfzCliActions)
        {
            CliActionID key = value.ActionID;

            // Assert no duplicate value (two of the same records)
            bool doesContainValue = GfzCliActionsLibrary.ContainsValue(value);
            if (doesContainValue)
            {
                string message =
                    $"Duplicate {nameof(GfzCliAction)} value \"{value}\" in {nameof(GfzCliActions)}! " +
                    $"{nameof(GfzCliAction)}.{nameof(GfzCliAction.ActionID)} is \"{value.ActionID}\".";
                throw new ArgumentException(message);
            }

            // Assert no duplicate keys (two actions with same ID)
            bool doesContainKey = GfzCliActionsLibrary.ContainsKey(key);
            if (doesContainKey)
            {
                string message = $"Duplicate {nameof(CliActionID)} key \"{key}\" in {nameof(GfzCliActions)}!";
                throw new ArgumentException(message);
            }

            GfzCliActionsLibrary.Add(key, value);
        }
    }


    /// <summary>
    ///     Effective Main for this program.
    /// </summary>
    /// <param name="args">Program arguments.</param>
    public static void RunCliParseArgs(string[] args)
    {
        // Initialize text capabilities
        var encodingProvider = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(encodingProvider);
        Console.OutputEncoding = Encoding.Unicode;

        InitUsageDictionary();

        // If user did not pass any arguments, tell them how to use application.
        // This will happen when users double-click application.
        bool noArgumentsPassed = args.Length == 0;
        if (noArgumentsPassed)
        {
            string msg =
                "Invalid use of program.\n" +
                "• Call command LIST to list all possible actions.\n" +
                "• Call command USAGE to show usage for all commands.\n" +
                "• Call command USAGE [COMMAND] to show specific command usage.";
            Terminal.WriteLine(msg, ConsoleColor.DarkYellow, ConsoleColor.Black);
            //Terminal.WriteLine(msg);
            Terminal.WriteLine();
            // Force help page
            args = HelpArg;
        }

        // Run program with options
        var parseResult = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(ExecuteAction);

        // If user did not pass any arguments, pause application so they can read Console.
        // This will happen when users double-click application.
        if (noArgumentsPassed)
        {
            string msg = "Press ENTER to continue.";
            Terminal.Write(msg, ConsoleColor.DarkYellow, ConsoleColor.Black);
            Terminal.WriteLine();
            Console.Read();
        }
    }

    /// <summary>
    ///     Parse action parameter in <paramref name="options"/> and run it.
    /// </summary>
    /// <param name="options">The action and related arguments.</param>
    public static void ExecuteAction(Options options)
    {
        GfzCliAction gfzCliAction = GfzCliActionsLibrary[options.Action];
        Assert.IsTrue(gfzCliAction.ActionID == options.Action);
        gfzCliAction.Action.Invoke(options);
    }

    /// <summary>
    ///     Force print --help text.
    /// </summary>
    public static void PrintHelp()
    {
        // Force show --help menu
        Parser.Default.ParseArguments<Options>(HelpArg).WithParsed(ExecuteAction);
    }

    /// <summary>
    ///     Print action description and parameters in <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The action and related arguments.</param>
    public static void PrintActionUsage(Options options)
    {
        if (string.IsNullOrWhiteSpace(options.InputPath))
        {
            // No action specified, so print all
            foreach (var kvp in GfzCliActionsLibrary)
            {
                // Skip these helpers
                if (kvp.Key == CliActionID.none ||
                    kvp.Key == CliActionID.list ||
                    kvp.Key == CliActionID.usage)
                    continue;

                kvp.Value.PrintAllArguments();
            }
        }
        else
        {
            // Action specified, print specific
            string actionStr = options.InputPath;
            CliActionID actionID = GfzCliParser.EnumParseUnderscoreToDash<CliActionID>(actionStr);
            PrintAction(actionID);
        }
    }

    /// <summary>
    ///     Print list of all actions possible with this program.
    /// </summary>
    /// <param name="_">Discard; formatted to conform to <see cref="GfzCliAction.Action"/>.</param>
    public static void PrintActionList(Options _)
    {
        foreach (var kvp in GfzCliActionsLibrary)
        {
            // Skip these helpers
            if (kvp.Key == CliActionID.none ||
                kvp.Key == CliActionID.list ||
                kvp.Key == CliActionID.usage)
                continue;

            kvp.Value.PrintActionAndDescription();
        }
    }

    /// <summary>
    ///     Print <paramref name="action"/> description and parameters.
    /// </summary>
    /// <param name="action">The action information to print.</param>
    public static void PrintAction(CliActionID action)
    {
        var gfzCliAction = GfzCliActionsLibrary[action];
        gfzCliAction.PrintAllArguments();
    }

}
