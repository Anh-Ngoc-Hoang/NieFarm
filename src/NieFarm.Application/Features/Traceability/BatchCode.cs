using System.Security.Cryptography;
using System.Text;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Traceability;

/// <summary>
/// The public code of a roast batch (R-26 D3): 12 characters from a 31-character alphabet without
/// 0 1 I L O (about 59.5 bits), stored and put in URLs as 12 letters/digits, shown in groups as
/// <c>ABCD-2345-EFGH</c>. Generated here, never in the domain; the domain only checks the shape.
/// </summary>
public static class BatchCode
{
    public const string Alphabet = RoastBatch.CodeAlphabet;
    public const int Length = RoastBatch.CodeLength;

    /// <summary>Anything longer than this cannot be a code with a few separators, so it is rejected unread.</summary>
    private const int MaxRawLength = 40;

    /// <summary>A uniformly random code (<see cref="RandomNumberGenerator.GetInt32(int)"/> has no modulo bias).</summary>
    public static string Generate()
    {
        var chars = new char[Length];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

        return new string(chars);
    }

    /// <summary>
    /// Accepts what a person might type or what a URL carries: spaces and dashes are dropped and ASCII
    /// letters are upper-cased. Anything else (including non-ASCII letters that Unicode case mapping could
    /// fold into ASCII) is rejected. Returns false for every wrong shape, and does no I/O.
    /// </summary>
    public static bool TryNormalise(string? raw, out string code)
    {
        code = string.Empty;
        if (string.IsNullOrEmpty(raw) || raw.Length > MaxRawLength)
            return false;

        var buffer = new StringBuilder(Length);
        foreach (var c in raw)
        {
            if (c == '-' || c == ' ')
                continue;

            var upper = c is >= 'a' and <= 'z' ? (char)(c - 32) : c;
            if (Alphabet.IndexOf(upper) < 0 || buffer.Length == Length)
                return false;

            buffer.Append(upper);
        }

        if (buffer.Length != Length)
            return false;

        code = buffer.ToString();
        return true;
    }

    /// <summary><c>ABCD2345EFGH</c> becomes <c>ABCD-2345-EFGH</c>. A value of the wrong length is returned unchanged.</summary>
    public static string Format(string code) =>
        code.Length == Length
            ? $"{code[..4]}-{code[4..8]}-{code[8..]}"
            : code;
}
