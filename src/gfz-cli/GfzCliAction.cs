using System;
using System.Globalization;

namespace Manifold.GFZCLI;

/// <summary>
///     Defines the usage of some <see cref="CliActionID"/> command.
/// </summary>
public readonly record struct GfzCliAction()
{
    /// <summary>
    ///     Represents a GFZ CLI Action (function call with <paramref name="options"/>).
    /// </summary>
    /// <param name="options">The options to provide the function.</param>
    public delegate void GfzCliActionDelegate(Options options);

    // TODO: maybe don't require everything...
    public required GfzCliActionDelegate Action { get; init; }
    public required string Description { get; init; }
    public required CliActionID ActionID { get; init; }
    public required CliActionIO InputIO { get; init; }
    public required CliActionIO OutputIO { get; init; }
    public required CliActionOption ActionOptions { get; init; }
    public required bool IsOutputOptional { get; init; } = true;
    public required GfzCliArgument[] RequiredArguments { get; init; }
    public required GfzCliArgument[] OptionalArguments { get; init; }

    // Description of action
    public const ConsoleColor ActionDescriptionColor = ConsoleColor.Green;
    public const ConsoleColor ActionNoDescriptionColor = ConsoleColor.Red;
    // Action itself, eg. arc-pack
    public const ConsoleColor ActionColor = ConsoleColor.White;
    // Arguments for action
    public const ConsoleColor RequiredArgColor = ConsoleColor.Cyan;
    public const ConsoleColor OptionalArgColor = ConsoleColor.DarkCyan;
    // Description of arguments
    public const ConsoleColor RequiredArgDescriptionColor = ConsoleColor.White;
    public const ConsoleColor OptionalArgDescriptionColor = ConsoleColor.DarkGray;

    private ConsoleColor GetArgumentColor(bool isRequired)
    {
        ConsoleColor color = isRequired
            ? RequiredArgColor
            : OptionalArgColor;
        return color;
    }
    private ConsoleColor GetArgDescriptionColor(bool isRequired)
    {
        ConsoleColor color = isRequired
            ? RequiredArgDescriptionColor
            : OptionalArgDescriptionColor;
        return color;
    }
    private ConsoleColor GetDescriptionColor()
    {
        ConsoleColor color = string.IsNullOrEmpty(Description)
            ? ActionNoDescriptionColor
            : ActionDescriptionColor;
        return color;
    }
    private string GetDescriptionText()
    {
        string desciption = string.IsNullOrEmpty(Description)
            ? "NO DESCRIPTION"
            : Description;
        return desciption;
    }

    public void PrintGeneralRequirements()
    {
        string actionStr = ActionID.ToString().Replace("_", "-");
        string input = GetInputString();
        string output = GetOutputString();
        string genericOptions = GetActionOptionsMessage();

        // Action
        Terminal.Write(actionStr, ActionColor);
        // Input path, if any
        if (!string.IsNullOrWhiteSpace(input))
            Terminal.Write(input, RequiredArgColor);
        // Output path, if any
        if (!string.IsNullOrWhiteSpace(output))
            Terminal.Write(output, IsOutputOptional ? OptionalArgColor : RequiredArgColor);
        // Generic FOPRS options
        if (!string.IsNullOrWhiteSpace(genericOptions))
            Terminal.Write(genericOptions, OptionalArgColor);
        // New line
        Terminal.WriteLine();
    }

    public void PrintAllArguments()
    {
        string description = GetDescriptionText();
        ConsoleColor descColor = GetDescriptionColor();
        Terminal.WriteLine(description, descColor);

        PrintGeneralRequirements();
        foreach (var requiredArgument in RequiredArguments)
            PrintArgument(requiredArgument, true);
        foreach (var optionalArgument in OptionalArguments)
            PrintArgument(optionalArgument, false);
        Terminal.WriteLine();
    }

    public void PrintActionAndDescription()
    {
        string actionStr = ActionID.ToString().Replace("_", "-");
        string description = GetDescriptionText();
        ConsoleColor descColor = GetDescriptionColor();

        Terminal.Write(actionStr);
        Terminal.Write(" ");
        Terminal.Write(description, descColor);
        Terminal.WriteLine();
    }

    private void PrintArgument(GfzCliArgument argumentInfo, bool isRequired)
    {
        string argName = argumentInfo.ArgumentName;
        string argType = argumentInfo.ArgumentType;
        string argDefault = argumentInfo.GetDefaultValueFormatted();
        string helpHint = argumentInfo.Help;

        ConsoleColor descColor = GetArgDescriptionColor(isRequired);
        ConsoleColor argColor = GetArgumentColor(isRequired);
        // required action, but optional arg
        //ConsoleColor argParamColor = string.IsNullOrEmpty(argDefault) ? argColor : OptionalArgColor;

        // Tab inset
        Terminal.Write($"\t");
        Terminal.Write($"--{argName}", argColor);
        Terminal.Write($" ");
        Terminal.Write($"<{argType}{argDefault}>");//, argParamColor);
        Terminal.Write($" ");
        // TEMP: move cursor / line up
        Console.SetCursorPosition(50, Console.CursorTop);

        if (!isRequired)
            Terminal.Write("Optional: ", descColor);
        Terminal.Write(helpHint);
        Terminal.WriteLine();
    }

    private string GetActionOptionsMessage()
    {
        if (ActionOptions == CliActionOption.None)
            return string.Empty;

        // Prepare string
        var builder = new System.Text.StringBuilder(66);
        builder.Append(" [");

        // Iterate over all possible values
        for (int i = 0; i < 32; i++)
        {
            CliActionOption option = (CliActionOption)((uint)ActionOptions & (1 << i));
            if (option == CliActionOption.None)
                continue;

            // Add pipe if not at start of string
            if (builder[^1] != '[')
                builder.Append('|');
            // Add parameter dash
            builder.Append('-');

            // Add action char
            switch (option)
            {
                case CliActionOption.O: builder.Append(IOptionsGfzCli.ArgsShort.OverwriteFiles); break;
                case CliActionOption.P: builder.Append(IOptionsGfzCli.ArgsShort.SearchPattern); break;
                case CliActionOption.S: builder.Append(IOptionsGfzCli.ArgsShort.SearchSubdirectories); break;
                case CliActionOption.F: builder.Append(IOptionsGfzCli.ArgsShort.SerializationFormat); break;
                case CliActionOption.R: builder.Append(IOptionsGfzCli.ArgsShort.SerializationRegion); break;
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
            CliActionIO.Directory => " <input-directory>",
            CliActionIO.File => " <input-file>",
            CliActionIO.Path => " <input-path>",
            CliActionIO.None => string.Empty,
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
            CliActionIO.Directory => $" {charL}{optional}output-directory{charR}",
            CliActionIO.File => $" {charL}{optional}output-file{charR}",
            CliActionIO.Path => $" {charL}{optional}output-path{charR}",
            CliActionIO.None => string.Empty,
            _ => throw new NotImplementedException(),
        };
        return output;
    }

}
