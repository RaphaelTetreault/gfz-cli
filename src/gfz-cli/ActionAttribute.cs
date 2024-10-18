using System;

namespace Manifold.GFZCLI;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class ActionAttribute : Attribute
{
    public ActionAttribute(
        ActionIO input,
        ActionIO output,
        ActionOption options = ActionOption.All,
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

    public ActionIO Input { get; }
    public ActionIO Output { get; }
    public bool IsOutputOptional { get; }
    public ActionOption Options { get; }
    public string SpecialOptions { get; }
    public string Footnote { get; }
}
