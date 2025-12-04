using System;

namespace Manifold.GFZCLI;

/// <summary>
///     Defines an argument: argument type, default (if any), and help hint.
/// </summary>
public readonly record struct GfzCliArgument()
{
    /// <summary>
    ///     Long name of argument for CLI.
    /// </summary>
    public required string ArgumentName { get; init; }

    /// <summary>
    ///     Argument type information. User-facing and not for casting.
    /// </summary>
    public required string ArgumentType { get; init; }

    /// <summary>
    ///     Default parameter to use if unset.
    ///     Null means no default parameter is defined.
    /// </summary>
    public required object? ArgumentDefault { get; init; }

    /// <summary>
    ///     Argument help text.
    /// </summary>
    public required string Help { get; init; }

    /// <summary>
    ///     Get <see cref="ArgumentDefault"/> formatted as string for display in
    ///     help/usage window.
    /// </summary>
    /// <returns>
    ///     
    /// </returns>
    public string GetDefaultValueFormatted()
    {
        string @default = ArgumentDefault != null && ArgumentDefault != (object)string.Empty
            ? $"={ArgumentDefault}"
            : string.Empty;

        return @default;
    }

    /// <summary>
    ///     Get <see cref="ArgumentDefault"/> as type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T Default<T>()
    {
        if (ArgumentDefault is null)
        {
            string msg = $"Cannot get default as default is null.";
            throw new Exception(msg);
        }
        else
        {
            Type argType = ArgumentDefault.GetType();
            Type desiredType = typeof(T);
            if (argType != desiredType)
            {
                string msg = $"Cannot convert default from type {desiredType.Name} to {argType.Name}.";
                throw new Exception(msg);
            }
        }

        return (T)ArgumentDefault!;
    }

    /// <summary>
    ///     Get <see cref="ArgumentDefault"/> as string.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string AsText()
    {
        if (ArgumentDefault is null || ArgumentDefault.ToString() == null)
            return "null";
        else
            return ArgumentDefault.ToString()!;
    }
}