using System.Linq;

namespace EmojiClassifier
{
    internal static class StringExtensions
    {
        internal static bool IsInEmojiUnicodeSpace(this string input) => input.Length > 0 && (input[0] >= 0xD800 && input[0] <= 0xDFFF || input[0] < 0 || input[0] > 0x10FFFF);
    }
}