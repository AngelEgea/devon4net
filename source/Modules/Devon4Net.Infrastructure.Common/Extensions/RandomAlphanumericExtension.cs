using System.Security.Cryptography;
using System.Text;

namespace Devon4Net.Infrastructure.Common.Extensions
{
    public static class RandomAlphanumericExtension
    {
        public static string GenerateRandomAlphanumeric(int length)
        {
            var passwordStringBuilder = new StringBuilder();

            // Generate characters sets
            var specialCharacters = new char[]
            {
                '^', '$', '*', '.', '[', ']', '{', '}', '(', ')', '?', '-', '"', '!', '@', '#', '%', '&', '/', '\\', ',', '>', '<', '\'', ':', ';', '|', '_', '~', '`', '+', '='
            };
            var numbers = Enumerable.Range('0', '9' - '0' + 1).Select(x => (char)x).ToArray();
            var lowerCase = Enumerable.Range('a', 'z' - 'a' + 1).Select(x => (char)x).ToArray();
            var upperCase = Enumerable.Range('A', 'Z' - 'A' + 1).Select(x => (char)x).ToArray();

            // Merge characters sets
            var characters = new char[][] { specialCharacters, numbers, lowerCase, upperCase };

            // Array to store where each special character set will go, to guarantee at least 1 character from each set
            var specialIndexes = Enumerable.Repeat(-1, characters.Length).ToArray();

            // Choose the special indexes
            for (int i = 0; i < specialIndexes.Length; i++)
            {
                int chosenIndex;

                do
                {
                    chosenIndex = RandomNumberGenerator.GetInt32(length);
                } while (specialIndexes.Contains(chosenIndex));

                specialIndexes[i] = chosenIndex;
            }

            // Generate password taking care of the special indexes
            for (int i = 0; i < length; i++)
            {
                char[] chosenSet;
                if (specialIndexes.Contains(i))
                {
                    chosenSet = characters[Array.IndexOf(specialIndexes, i)];
                }
                else
                {
                    chosenSet = characters[RandomNumberGenerator.GetInt32(characters.Length)];
                }
                var chosenCharacter = chosenSet[RandomNumberGenerator.GetInt32(chosenSet.Length)];
                passwordStringBuilder.Append(chosenCharacter);
            }

            return passwordStringBuilder.ToString();
        }
    }
}
