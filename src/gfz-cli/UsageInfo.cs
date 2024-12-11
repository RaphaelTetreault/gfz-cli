using System;

namespace Manifold.GFZCLI;

/// <summary>
///     Defines the usage of some <see cref="Actions"/> command.
/// </summary>
public readonly record struct UsageInfo()
{
    public required Actions Action { get; init; }
    public readonly ActionIO InputIO { get; init; }
    public readonly ActionIO OutputIO { get; init; }
    public readonly ActionOption ActionOptions { get; init; }
    public readonly bool IsOutputOptional { get; init; }
    public required ArgumentInfo[] RequiredArguments { get; init; }
    public required ArgumentInfo[] OptionalArguments { get; init; }

    public const ConsoleColor ActionColor = ConsoleColor.White;
    public const ConsoleColor RequiredColor = ConsoleColor.Cyan;
    public const ConsoleColor OptionalColor = ConsoleColor.DarkCyan;


    public void PrintGeneralRequirements()
    {
        string actionStr = Action.ToString().Replace("_", "-");
        string input = GetInputString();
        string output = GetOutputString();
        string genericOptions = GetActionOptionsMessage();

        // Action
        Terminal.Write(actionStr, ActionColor);
        // Input path, if any
        if (!string.IsNullOrWhiteSpace(input))
            Terminal.Write(input, RequiredColor);
        // Output path, if any
        if (!string.IsNullOrWhiteSpace(output))
            Terminal.Write(output, IsOutputOptional ? OptionalColor : RequiredColor);
        // Generic FOPRS options
        if (!string.IsNullOrWhiteSpace(genericOptions))
            Terminal.Write(genericOptions, OptionalColor);
        // New line
        Terminal.WriteLine();
    }

    public void PrintAllArguments()
    {
        PrintGeneralRequirements();

        foreach (var requiredArgument in RequiredArguments)
        {
            PrintArgument(requiredArgument, true);
        }
        foreach (var optionalArgument in OptionalArguments)
        {
            PrintArgument(optionalArgument, false);
        }
    }

    private static void PrintArgument(ArgumentInfo argumentInfo, bool isRequired)
    {
        string argName = argumentInfo.ArgumentName;
        string argType = argumentInfo.ArgumentType.Name;
        string @default = argumentInfo.GetDefaultValueFormatted();
        string helpHint = argumentInfo.Help;
        ConsoleColor color = isRequired ? RequiredColor : OptionalColor;
        ConsoleColor argsColor = string.IsNullOrEmpty(@default) ? color : OptionalColor;

        // Tab inset
        Terminal.Write($"\t");
        if (!isRequired)
            Terminal.Write("[", color);
        Terminal.Write($"--{argName}", color);
        Terminal.Write($" ");
        Terminal.Write($"<{argType}{@default}>", argsColor);
        if (!isRequired)
            Terminal.Write("]", color);
        Terminal.Write($" ");
        // TEMP: move cursor / line up
        Console.SetCursorPosition(50, Console.CursorTop);
        Terminal.Write(helpHint);

        Terminal.WriteLine();
    }

    private string GetActionOptionsMessage()
    {
        if (ActionOptions == ActionOption.None)
            return string.Empty;

        // Prepare string
        var builder = new System.Text.StringBuilder(66);
        builder.Append(" [");

        // Iterate over all possible values
        for (int i = 0; i < 32; i++)
        {
            ActionOption option = (ActionOption)((uint)ActionOptions & (1 << i));
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
                case ActionOption.O: builder.Append(IOptionsGfzCli.ArgsShort.OverwriteFiles); break;
                case ActionOption.P: builder.Append(IOptionsGfzCli.ArgsShort.SearchPattern); break;
                case ActionOption.S: builder.Append(IOptionsGfzCli.ArgsShort.SearchSubdirectories); break;
                case ActionOption.F: builder.Append(IOptionsGfzCli.ArgsShort.SerializationFormat); break;
                case ActionOption.R: builder.Append(IOptionsGfzCli.ArgsShort.SerializationRegion); break;
                default: throw new NotImplementedException(option.ToString());
            }
        }
        // Close options and finish
        builder.Append(']');
        return builder.ToString();
    }

    private string GetInputString()
    {
        string input = InputIO switch
        {
            ActionIO.Directory => " <input-directory>",
            ActionIO.File => " <input-file>",
            ActionIO.Path => " <input-path>",
            ActionIO.None => string.Empty,
            _ => throw new NotImplementedException(),
        };
        return input;
    }

    private string GetOutputString()
    {
        string optional = IsOutputOptional ? "optional-" : string.Empty;
        string charL = IsOutputOptional ? "[" : "<";
        string charR = IsOutputOptional ? "]" : ">";
        string output = OutputIO switch
        {
            ActionIO.Directory => $" {charL}{optional}output-directory{charR}",
            ActionIO.File => $" {charL}{optional}output-file{charR}",
            ActionIO.Path => $" {charL}{optional}output-path{charR}",
            ActionIO.None => string.Empty,
            _ => throw new NotImplementedException(),
        };
        return output;
    }
}
