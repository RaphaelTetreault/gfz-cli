using System;
using System.ComponentModel;

namespace Manifold.GFZCLI;

[Description]
[AttributeUsage(AttributeTargets.All)]
internal sealed class ActionAttribute : Attribute
{
    public ActionAttribute(ActionIO ioMode)
    {
        IOMode = ioMode;
        SpecialOptions = string.Empty;
        Footnote = string.Empty;
    }
    public ActionAttribute(ActionIO ioMode, string specialOptions)
    {
        IOMode = ioMode;
        SpecialOptions = specialOptions;
        Footnote = string.Empty;
    }
    public ActionAttribute(ActionIO ioMode, string specialOptions, string footnote)
    {
        IOMode = ioMode;
        SpecialOptions = specialOptions;
        Footnote = footnote;
    }

    public ActionIO IOMode { get; }
    public string SpecialOptions { get; }
    public string Footnote { get; }
}
