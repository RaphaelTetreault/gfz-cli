namespace Manifold.GFZCLI
{
    /// <summary>
    /// Static wrapper to make handle Console verbosity
    /// </summary>
    public static class VerboseConsole
    {
        public static bool IsVerbose { get; set; }

        public static void Write(object? value) { if (IsVerbose) Console.Write(value); }
        public static void Write(string? value) { if (IsVerbose) Console.Write(value); }
        public static void Write(string format, object? value) { if (IsVerbose) Console.Write(format, value); }
        public static void Write(string format, object?[] value) { if (IsVerbose) Console.Write(format, value); }

        public static void WriteLine() { if (IsVerbose) Console.WriteLine(); }
        public static void WriteLine(object? value) { if (IsVerbose) Console.WriteLine(value); }
        public static void WriteLine(string? value) { if (IsVerbose) Console.WriteLine(value); }
        public static void WriteLine(string format, object? value) { if (IsVerbose) Console.WriteLine(format, value); }
        public static void WriteLine(string format, object?[] value) { if (IsVerbose) Console.WriteLine(format, value); }
    }
}
