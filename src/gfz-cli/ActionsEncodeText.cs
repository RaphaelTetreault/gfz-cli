using GameCube.GFZ;
using System;
using System.Text;

namespace Manifold.GFZCLI;

public class ActionsEncodeText
{
    public static readonly GfzCliAction ActionEncodeBytesToShiftJis = new()
    {
        Description = "Takes in hex-string of bytes and prints the Shift-JIS encoded version of the value.",
        Action = PrintAsciiToShiftJis,
        ActionID = CliActionID.encode_bytes_to_shift_jis,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };
    public static readonly GfzCliAction ActionEncodeWindows1252ToShiftJis = new()
    {
        Description = "Takes in Windows code page 1252 string and prints the Shift-JIS encoded version of the value.",
        Action = PrintAsciiToShiftJis,
        ActionID = CliActionID.encode_windows_to_shift_jis,
        InputIO = CliActionIO.None,
        OutputIO = CliActionIO.None,
        IsOutputOptional = true,
        ActionOptions = CliActionOption.None,
        RequiredArguments = [],
        OptionalArguments = [],
    };


    // Generic and usable in any context
    public static void AssertOnlyHexCharacters(string value)
    {
        // Values are sequencial when doing '0' through 'F'
        value = value.ToUpper();

        // Ensure values are in range 0 through F
        bool isInvalid = false;
        foreach (char c in value)
        {
            if (c < '0' || c > 'F')
            {
                isInvalid = true;
                break;
            }
        }

        // Assert
        if (isInvalid)
        {
            string msg = $"Argument --{IOptionsLineRel.Args.Value} must be only hexadecimal characters.";
            throw new ArgumentException(msg);
        }
    }

    public static byte[] GetHexStringAsByteArray(string value)
    {
        bool isNotEven = value.Length % 2 == 1;
        if (isNotEven)
        {
            throw new Exception();
        }

        int length = value.Length / 2;
        byte[] byteArray = new byte[length];
        for (int i = 0; i < length; i++)
        {
            string byteString = value.Substring(i*2, 2);
            byte @byte = byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber);
            byteArray[i] = @byte;
        }

        return byteArray;
    }

    public static string ConvertBytesToEncoding(string value, Encoding encoding)
    {
        // Sanitize string
        value = value.Replace(" ", "");
        AssertOnlyHexCharacters(value);

        // Convert bytes to encoded string
        byte[] bytes = GetHexStringAsByteArray(value);
        string result = encoding.GetString(bytes);
        return result;
    }

    public static string ConvertEncodingToEncoding(string value, Encoding encodingInput, Encoding encodingOutput)
    {
        // Convert string from one format to another
        byte[] bytes = encodingInput.GetBytes(value);
        string result = encodingOutput.GetString(bytes);
        return result;
    }


    // Specifically linked to read Options.Value
    public static string ConvertValueBytesToEncoding(Options options, Encoding encoding)
    {
        ActionsLineREL.AssertValue(options);
        string result = ConvertBytesToEncoding(options.Value, encoding);
        return result;
    }

    public static string ConvertValueEncodingToEncoding(Options options, Encoding encodingInput, Encoding encodingOutput)
    {
        ActionsLineREL.AssertValue(options);
        string result = ConvertEncodingToEncoding(options.Value, encodingInput, encodingOutput);
        return result;
    }


    /// <summary>
    ///     Takes in hex-string of bytes and converts to Shift-JIS encoded string then finally to Unicode.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    public static string ConvertBytesToShiftJis(Options options)
        => ConvertValueBytesToEncoding(options, TextEncoding.ShiftJIS);

    /// <summary>
    ///     Takes in an ASCII string of character and converts to Shift-JIS encoded  string then finally to Unicode.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    public static string ConvertWindows1252ToShiftJis(Options options)
        => ConvertValueEncodingToEncoding(options, TextEncoding.Windows1252, TextEncoding.ShiftJIS);


    /// <summary>
    ///     Takes in hex-string of bytes and prints the Shift-JIS encoded version of the value.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    public static void PrintBytesToShiftJis(Options options)
        => Terminal.WriteLine(ConvertBytesToShiftJis(options));

    /// <summary>
    ///     Takes in Windows code page 1252 string and prints the Shift-JIS encoded version of the value.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    public static void PrintAsciiToShiftJis(Options options)
        => Terminal.WriteLine(ConvertWindows1252ToShiftJis(options));
}
