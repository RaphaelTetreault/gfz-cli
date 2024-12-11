using System;

namespace Manifold.GFZCLI;

/// <summary>
///     Defines an argument and defines argument type, default (if any), and help hint.
/// </summary>
public readonly record struct ArgumentInfo()
{
    public required string ArgumentName { get; init; }
    public required Type ArgumentType { get; init; }
    public required object? ArgumentDefault { get; init; }
    public required string Help { get; init; }

    //public string GetArgument()
    //{
    //    string @default = ArgumentDefault != null ? $"={ArgumentDefault}" : string.Empty;
    //    string value = $"--{ArgumentName} <{ArgumentType}{@default}>";
    //    return value;
    //}

    public string GetDefaultValueFormatted()
    {
        string @default = ArgumentDefault != null
            ? $"={ArgumentDefault}"
            : string.Empty;

        return @default;
    }
}