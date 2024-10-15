using System;

namespace Manifold.GFZCLI;

internal class ActionUsageHints
{
    public const string general_options =
         $"[-f|--{IGfzCliOptions.Args.SerializationFormat}]" +
        $" [-o|--{IGfzCliOptions.Args.OverwriteFiles}]" +
        $" [-p|--{IGfzCliOptions.Args.SearchPattern}]" +
        $" [-r|--{IGfzCliOptions.Args.SerializationRegion}]" +
        $" [-s|--{IGfzCliOptions.Args.SearchSubdirectories}]";

    public static void DisplayHint(Options options) => DisplayHint(options.Action);
    public static void DisplayHint(GfzCliAction actionValue)
    {
        // TODO PUT THIS CRAP IN A GENERIC FUNCTION
        string action = actionValue.ToString();
        var memberInfos = typeof(GfzCliAction).GetMember(action);
        if (memberInfos is null || memberInfos.Length == 0)
        {
            Terminal.WriteLine("[No hints specified for this action]", ConsoleColor.Yellow);
            return;
        }
        var actionAttributes = memberInfos[0].GetCustomAttributes(typeof(ActionAttribute), false);
        var actionAttribute = (ActionAttribute)actionAttributes[0];

        // input specifier
        string input = actionAttribute.IOMode switch
        {
            ActionIO.FileIn or
            ActionIO.FileInOut => "<input-file>",

            ActionIO.DirectoryIn or
            ActionIO.DirectoryInOut => "<input-directory>",

            ActionIO.PathIn or
            ActionIO.PathInOut => "<input-path>",

            _ => string.Empty,
        };
        // Output specifier
        string output = actionAttribute.IOMode switch
        {
            ActionIO.FileOut or
            ActionIO.FileInOut => "<output-file>",

            ActionIO.DirectoryOut or
            ActionIO.DirectoryInOut => "<output-directory>",

            ActionIO.PathOut or
            ActionIO.PathInOut => "<output-path>",

            _ => string.Empty,
        };
        // Custom parameters for certain actions
        string specialOptions = actionAttribute.SpecialOptions;

        // Construct hint and print
        action = action.Replace("_", "-");
        string hint = $"{action} {input} {output} {general_options} {specialOptions}";
        Terminal.WriteLine(hint, ConsoleColor.DarkYellow);
    }



}
