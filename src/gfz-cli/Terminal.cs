using System;

namespace Manifold.GFZCLI;

/// <summary>
///     A wrapper for System.Console with the option to write with ConsoleColor
/// </summary>
public static class Terminal
{
    /// <summary>
    ///     Shared lock for Terminal usage.
    /// </summary>
    public static readonly object Lock = new();

    private static void Write(Action consoleWrite, ConsoleColor foregroundColor)
    {
        var fgColor = Console.ForegroundColor;
        Console.ForegroundColor = foregroundColor;
        consoleWrite.Invoke();
        Console.ForegroundColor = fgColor;
    }
    private static void Write(Action consoleWrite, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
    {
        var fgColor = Console.ForegroundColor;
        var bgColor = Console.BackgroundColor;
        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
        consoleWrite.Invoke();
        Console.ForegroundColor = fgColor;
        Console.BackgroundColor = bgColor;
    }

    // Write
    public static void Write(string value, object[] args) => Console.Write(value, args);
    public static void Write(string value, object arg) => Console.Write(value, arg);
    public static void Write(ulong value) => Console.Write(value);
    public static void Write(uint value) => Console.Write(value);
    public static void Write(string value) => Console.Write(value);
    public static void Write(char[] value, int index, int count) => Console.Write(value, index, count);
    public static void Write(object value) => Console.Write(value);
    public static void Write(float value) => Console.Write(value);
    public static void Write(char value) => Console.Write(value);
    public static void Write(char[] value) => Console.Write(value);
    public static void Write(bool value) => Console.Write(value);
    public static void Write(double value) => Console.Write(value);
    public static void Write(int value) => Console.Write(value);
    public static void Write(long value) => Console.Write(value);
    public static void Write(decimal value) => Console.Write(value);

    // WriteLine
    public static void WriteLine() => Console.WriteLine();
    public static void WriteLine(string value, object[] args) => Console.WriteLine(value, args);
    public static void WriteLine(string value, object arg) => Console.WriteLine(value, arg);
    public static void WriteLine(ulong value) => Console.WriteLine(value);
    public static void WriteLine(uint value) => Console.WriteLine(value);
    public static void WriteLine(string value) => Console.WriteLine(value);
    public static void WriteLine(char[] value, int index, int count) => Console.WriteLine(value, index, count);
    public static void WriteLine(object value) => Console.WriteLine(value);
    public static void WriteLine(float value) => Console.WriteLine(value);
    public static void WriteLine(char value) => Console.WriteLine(value);
    public static void WriteLine(char[] value) => Console.WriteLine(value);
    public static void WriteLine(bool value) => Console.WriteLine(value);
    public static void WriteLine(double value) => Console.WriteLine(value);
    public static void WriteLine(int value) => Console.WriteLine(value);
    public static void WriteLine(long value) => Console.WriteLine(value);
    public static void WriteLine(decimal value) => Console.WriteLine(value);

    // Write with console color fg
    public static void Write(object value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(string value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(char[] value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(char value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(bool value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(int value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(long value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(uint value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(ulong value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(float value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(double value, ConsoleColor foregroundColor) => Write(() => Console.Write(value), foregroundColor);
    public static void Write(object value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(string value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(char[] value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(char value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(bool value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(int value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(long value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(uint value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(ulong value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(float value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);
    public static void Write(double value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.Write(value), foregroundColor, backgroundColor);

    // Write with console color fg/bg
    public static void WriteLine(object value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(string value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(char[] value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(char value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(bool value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(int value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(long value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(uint value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(ulong value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(float value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(double value, ConsoleColor foregroundColor) => Write(() => Console.WriteLine(value), foregroundColor);
    public static void WriteLine(object value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(string value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(char[] value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(char value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(bool value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(int value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(long value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(uint value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(ulong value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(float value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);
    public static void WriteLine(double value, ConsoleColor foregroundColor, ConsoleColor backgroundColor) => Write(() => Console.WriteLine(value), foregroundColor, backgroundColor);

}
