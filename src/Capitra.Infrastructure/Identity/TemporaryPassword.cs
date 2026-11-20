using System.Security.Cryptography;

namespace Capitra.Infrastructure.Identity;

/// <summary>
/// Temporary passwords made by the system when an administrator creates an account or resets its password: random, meeting the
/// password policy (upper and lower case, digit, symbol), without look-alike characters (0/O, 1/l/I) so they can be read out.
/// The user must replace it at first sign-in.
/// </summary>
public static class TemporaryPassword
{
    public const int Length = 14;

    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%*?-+=";
    private const string All = Upper + Lower + Digits + Symbols;

    public static string Generate()
    {
        var chars = new char[Length];
        chars[0] = Pick(Upper);
        chars[1] = Pick(Lower);
        chars[2] = Pick(Digits);
        chars[3] = Pick(Symbols);
        for (var i = 4; i < Length; i++)
            chars[i] = Pick(All);
        // shuffle so the required kinds are not always first
        for (var i = Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char Pick(string from) => from[RandomNumberGenerator.GetInt32(from.Length)];
}
