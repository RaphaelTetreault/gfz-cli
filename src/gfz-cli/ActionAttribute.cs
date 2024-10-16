using System;
using System.ComponentModel;

namespace Manifold.GFZCLI;

[Description]
[AttributeUsage(AttributeTargets.Field)]
internal sealed class ActionAttribute : Attribute
{
    public ActionAttribute(ActionIO ioMode)
        : this(ioMode, ActionOption.All, string.Empty, string.Empty) { }

    public ActionAttribute(ActionIO ioMode, ActionOption options)
        : this(ioMode, options, string.Empty, string.Empty) { }

    public ActionAttribute(ActionIO ioMode, ActionOption options, string specialOptions)
        : this(ioMode, options, specialOptions, string.Empty) { }

    public ActionAttribute(ActionIO ioMode, ActionOption options, string specialOptions, string footnote)
    {
        IOMode = ioMode;
        Options = options;
        SpecialOptions = specialOptions;
        Footnote = footnote;
    }

    public ActionIO IOMode { get; }
    public ActionOption Options { get; }
    public string SpecialOptions { get; }
    public string Footnote { get; }
}
