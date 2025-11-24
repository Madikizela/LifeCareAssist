using System.Security.Cryptography;
using System.Text;

namespace RuralHealthcare.Services;

public static class PasswordGenerator
{
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*";

    public static string GenerateSecurePassword(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8 characters");

        var allChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;
        var password = new StringBuilder();

        // Ensure at least one character from each category
        password.Append(GetRandomChar(LowercaseChars));
        password.Append(GetRandomChar(UppercaseChars));
        password.Append(GetRandomChar(DigitChars));
        password.Append(GetRandomChar(SpecialChars));

        // Fill the rest randomly
        for (int i = 4; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        // Shuffle the password
        return Shuffle(password.ToString());
    }

    private static char GetRandomChar(string chars)
    {
        var randomIndex = RandomNumberGenerator.GetInt32(0, chars.Length);
        return chars[randomIndex];
    }

    private static string Shuffle(string input)
    {
        var array = input.ToCharArray();
        int n = array.Length;
        
        while (n > 1)
        {
            n--;
            int k = RandomNumberGenerator.GetInt32(0, n + 1);
            (array[k], array[n]) = (array[n], array[k]);
        }
        
        return new string(array);
    }
}
