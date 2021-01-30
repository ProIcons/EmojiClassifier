using System.Text;

namespace EmojiClassifier
{
    internal static class StringExtensions
    {
        internal static bool IsUnicode(this string input) => Encoding.ASCII.GetByteCount(input) != Encoding.UTF8.GetByteCount(input);
    }
}