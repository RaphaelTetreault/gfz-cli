using System;
using System.Text;

namespace Manifold.GFZCLI;

public static class ActionsUsage
{
    /// <summary>
    ///     Print out usage message.
    /// </summary>
    /// <remarks>
    ///     usage (nothing) will print all actions.
    ///     usage (action-name) will print out all info for that command.
    /// </remarks>
    /// <param name="options"></param>
    public static void PrintActionUsage(Options options)
    {
        // Shenangians. Use input path as variable for this hack
        string actionStr = options.InputPath;

        // For 'usage' command, enum is passed in as input path
        Actions action = string.IsNullOrWhiteSpace(actionStr)
            ? Actions.none
            : GfzCliEnumParser.ParseUnderscoreToDash<Actions>(actionStr);

        // no action specified after 'usage' action
        if (action == Actions.none)
        {
            string msg = $"\"{actionStr}\" is an invalid action. Actions and general usage:";
            Terminal.WriteLine(msg);
            PrintActionUsageList();
        }
        else // print specific usage
        {
            PrintActionUsageComplete(action);
        }
    }

    public static void PrintActionUsageComplete(Actions action, ConsoleColor color = ConsoleColor.Cyan)
    {
        // Printable string of value
        string actionStr = action.ToString().Replace("_", "-");

        // If valid, get info about the action
        var actionAttribute = AttributeHelper.GetAttribute<ActionAttribute, Actions>(action);
        if (actionAttribute == null)
        {
            Terminal.WriteLine($"{actionStr} (usage not yet defined)", ConsoleColor.Red);
            return;
        }

        string input = actionAttribute.Input switch
        {
            ActionIO.Directory => " <input-directory>",
            ActionIO.File => " <input-file>",
            ActionIO.Path => " <input-path>",
            ActionIO.None => string.Empty,
            _ => throw new NotImplementedException(),
        };
        string optional = actionAttribute.IsOutputOptional ? "optional-" : string.Empty;
        string charL = actionAttribute.IsOutputOptional ? "[" : "<";
        string charR = actionAttribute.IsOutputOptional ? "]" : ">";
        string output = actionAttribute.Output switch
        {
            ActionIO.Directory => $" {charL}{optional}output-directory{charR}",
            ActionIO.File => $" {charL}{optional}output-file{charR}",
            ActionIO.Path => $" {charL}{optional}output-path{charR}",
            ActionIO.None => string.Empty,
            _ => throw new NotImplementedException(),
        };
        // Construct hint and print
        string generalOptions = GetActionOptionsMessage(actionAttribute.Options);
        string specialOptions = actionAttribute.SpecialOptions;
        string hint = $"{actionStr}{input}{output}{generalOptions} {specialOptions}";
        Terminal.WriteLine(hint, color);
    }

    public static void PrintActionUsageList()
    {
        foreach (Actions value in Enum.GetValues<Actions>())
        {
            // Skip meta values
            if (value == Actions.none || value == Actions.usage)
                continue;

            // Print out actions
            Terminal.Write($"\t");
            PrintActionUsageComplete(value);
        }
    }

    public static string GetActionOptionsMessage(ActionOption actionOptions)
    {
        if (actionOptions == ActionOption.None)
            return string.Empty;

        // Prepare string
        StringBuilder builder = new StringBuilder(66);
        builder.Append(" [");

        // Iterate over all possible values
        for (int i = 0; i < 32; i++)
        {
            ActionOption option = (ActionOption)((uint)actionOptions & (1 << i));
            if (option == ActionOption.None)
                continue;

            // Add pipe if not at start of string
            if (builder[^1] != '[')
                builder.Append('|');
            // Add parameter dash
            builder.Append('-');

            // Add action char
            switch (option)
            {
                case ActionOption.O_OverwriteFiles: builder.Append(IOptionsGfzCli.ArgsShort.OverwriteFiles); break;
                case ActionOption.P_SearchPattern: builder.Append(IOptionsGfzCli.ArgsShort.SearchPattern); break;
                case ActionOption.S_SearchSubdirectories: builder.Append(IOptionsGfzCli.ArgsShort.SearchSubdirectories); break;
                case ActionOption.F_SerializationFormat: builder.Append(IOptionsGfzCli.ArgsShort.SerializationFormat); break;
                case ActionOption.R_SerializationRegion: builder.Append(IOptionsGfzCli.ArgsShort.SerializationRegion); break;
                default: throw new NotImplementedException(option.ToString());
            }
        }
        // Close options and finish
        builder.Append(']');
        return builder.ToString();
    }

    // TODO: use these instead of throwing errors! (When possible? Does this make sense? Maybe do custom error?)

    public static void ActionWarning(Options options, string message)
    {
        Terminal.WriteLine(message, Program.WarningColor);
        PrintActionUsageComplete(options.Action, Program.WarningColor);
    }

    public static void ActionNotification(string message)
    {
        Terminal.WriteLine(message, Program.NotificationColor);
    }

}
