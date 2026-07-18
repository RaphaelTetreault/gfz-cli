using CommandLine;
using Manifold.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manifold.GfzCli;

public static class GfzCli
{
    // TODO: rewrite Terminal to use 
    public const ConsoleColor FileNameColor = ConsoleColor.Cyan;
    public const ConsoleColor FileWriteColor = ConsoleColor.Green;
    public const ConsoleColor FileOverwriteColor = ConsoleColor.DarkYellow;
    public const ConsoleColor FileOverwriteSkipColor = ConsoleColor.Red;
    public const ConsoleColor SubTaskColor = ConsoleColor.DarkGray;
    public const ConsoleColor WarningColor = ConsoleColor.Red;
    public const ConsoleColor NotificationColor = ConsoleColor.DarkYellow;
    public static readonly string[] HelpArg = ["--help"];

    public static readonly Dictionary<CliActionID, CliAction> GfzCliActionsLibrary = [];

    private static void InitUsageDictionary()
    {
        // Prevent running twice
        if (GfzCliActionsLibrary.Count != 0)
            return;

        foreach (CliAction value in CliActionDB.GfzCliActions)
        {
            CliActionID key = value.ActionID;

            // Assert no duplicate value (two of the same records)
            bool doesContainValue = GfzCliActionsLibrary.ContainsValue(value);
            if (doesContainValue)
            {
                string message =
                    $"Duplicate {nameof(CliAction)} value \"{value}\" in {nameof(CliActionDB.GfzCliActions)}! " +
                    $"{nameof(CliAction)}.{nameof(CliAction.ActionID)} is \"{value.ActionID}\".";
                throw new ArgumentException(message);
            }

            // Assert no duplicate keys (two actions with same ID)
            bool doesContainKey = GfzCliActionsLibrary.ContainsKey(key);
            if (doesContainKey)
            {
                string message = $"Duplicate {nameof(CliActionID)} key \"{key}\" in {nameof(CliActionDB.GfzCliActions)}!";
                throw new ArgumentException(message);
            }

            GfzCliActionsLibrary.Add(key, value);
        }

        foreach (CliActionID cliActionID in Enum.GetValues<CliActionID>())
        {
            bool doesCLiMapEnum = GfzCliActionsLibrary.ContainsKey(cliActionID);
            if (!doesCLiMapEnum)
            {
                string msg = $"WARNING: {nameof(CliActionDB.GfzCliActions)} missing map to enum {cliActionID}.";
                Terminal.WriteLine(msg, WarningColor);
            }
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
        ParserResult<OptionsCliArgs> parseCliOptions = Parser.Default.ParseArguments<OptionsCliArgs>(args);
        Options options = parseCliOptions.Value.CreateOptions();
        ExecuteAction(options);

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
        // Get action to run
        CliAction gfzCliAction = GfzCliActionsLibrary[options.Action];
        Assert.IsTrue(gfzCliAction.ActionID == options.Action);

        // If search pattern unset but default exists, patch
        bool hasNoSearchPattern = string.IsNullOrEmpty(options.SearchPattern);
        bool hasDefaultSearchPattern = !string.IsNullOrWhiteSpace(gfzCliAction.DefaultSearchPattern);
        if (hasNoSearchPattern && hasDefaultSearchPattern)
            options = options with { SearchPattern = gfzCliAction.DefaultSearchPattern };

        // Run assertions
        foreach (CliArgument requiredArgument in gfzCliAction.RequiredArguments)
            if (requiredArgument.Assert is not null)
                requiredArgument.Assert(options);
        foreach (CliArgument optionalArgument in gfzCliAction.OptionalArguments)
            if (optionalArgument.Assert is not null)
                optionalArgument.Assert(options);

        // Invoke action
        gfzCliAction.Action.Invoke(options);
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
    /// <param name="_">Discard; formatted to conform to <see cref="CliAction.Action"/>.</param>
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
