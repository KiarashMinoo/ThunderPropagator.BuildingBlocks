using System.Security.Cryptography;
using System.Text;

namespace RapidStreamer.BuildingBlocks.Application.Ciphering;

public
#if !DEBUG
        sealed
#endif
    class PasswordGenerator
{
    public
#if !DEBUG
        sealed
#endif
        class PasswordSettings
    {
        public bool IncludeUpperCase { get; set; } = true;
        public string? CustomUpperCase { get; set; }
        public bool IncludeLowerCase { get; set; } = true;
        public string? CustomLowerCase { get; set; }
        public bool IncludeNumbers { get; set; } = true;
        public string? CustomDigits { get; set; }
        public bool IncludeSymbols { get; set; } = false;
        public string? CustomSymbols { get; set; }
        public bool BeginWithLetter { get; set; } = true;
        public bool PreventDuplicateCharacters { get; set; } = false;
        public bool PreventSequentialCharacters { get; set; } = false;
    }

    private const string UpperCase = "ABCDEFGHJKMNPQRSTUVWXYZ"; // Excludes I and O
    private const string LowerCase = "abcdefghjkmnpqrstuvwxyz"; // Excludes i and o
    private const string Digits = "23456789"; // Excludes 0 and 1
    private const string Symbols = "!\";#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

    public static string Generate(int length, Action<PasswordSettings>? configure = null)
    {
        if (length < 4) throw new ArgumentException("Password length must be at least 4.");

        var settings = new PasswordSettings();
        configure?.Invoke(settings);

        var charSets = new List<string>();
        if (settings.IncludeUpperCase) charSets.Add(settings.CustomUpperCase ?? UpperCase);
        if (settings.IncludeLowerCase) charSets.Add(settings.CustomLowerCase ?? LowerCase);
        if (settings.IncludeNumbers) charSets.Add(settings.CustomDigits ?? Digits);
        if (settings.IncludeSymbols) charSets.Add(settings.CustomSymbols ?? Symbols);

        if (charSets.Count == 0) throw new ArgumentException("At least one character type must be selected.");

        var password = new StringBuilder();
        var usedCharacters = new HashSet<char>();

        if (settings.BeginWithLetter)
            AppendRandomChar(UpperCase + LowerCase);

        while (password.Length < length)
        {
            var selectedSet = charSets[GetRandomIndex(charSets.Count)];
            AppendRandomChar(selectedSet);

            if (password.Length > 1 && IsSequential(password[^2], password[^1]))
            {
                password.Remove(password.Length - 1, 1);
                usedCharacters.Remove(password[^1]);
            }
        }

        return new string(password.ToString().OrderBy(_ => GetRandomIndex(password.Length)).ToArray());

        void AppendRandomChar(string chars)
        {
            char c;
            do
            {
                c = chars[GetRandomIndex(chars.Length)];

                if (!settings.PreventDuplicateCharacters)
                    break;
            } while (usedCharacters.Contains(c));

            password.Append(c);
            usedCharacters.Add(c);
        }

        bool IsSequential(char a, char b)
            => settings.PreventSequentialCharacters && Math.Abs(a - b) == 1;

        static int GetRandomIndex(int max)
        {
            var data = new byte[4];
            RandomNumberGenerator.Fill(data);
            return BitConverter.ToInt32(data, 0) & int.MaxValue % max;
        }
    }
}