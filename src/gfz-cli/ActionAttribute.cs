using System;

namespace Manifold.GFZCLI;

/// <summary>
///     Attribute which adds metadata that descibes how <see cref="CliActionID"/> values work.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class ActionAttribute : Attribute
{
    public ActionAttribute(
        CliActionIO input,
        CliActionIO output,
        CliActionOption options = CliActionOption.All,
        bool outputOptional = true,
        string specialOptions = "",
        string notes = "")
    {
        Input = input;
        Output = output;
        IsOutputOptional = outputOptional;
        Options = options;
        SpecialOptions = specialOptions;
        Footnote = notes;
    }

    public CliActionIO Input { get; }
    public CliActionIO Output { get; }
    public bool IsOutputOptional { get; }
    public CliActionOption Options { get; }
    public string SpecialOptions { get; }
    public string Footnote { get; }
}
