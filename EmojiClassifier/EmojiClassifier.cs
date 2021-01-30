using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiClassifier
{
    public class EmojiClassifier<TEmoji, TVariation> : IDisposable where TEmoji : IEmoji<TEmoji, TVariation>
                                                                   where TVariation : IEmojiVariation<TEmoji, TVariation>
    {
        protected IEmojiDataProvider<TEmoji, TVariation> EmojiDataProvider { get; }

        public EmojiClassifier(IEmojiDataProvider<TEmoji, TVariation> emojiDataProvider)
        {
            EmojiDataProvider = emojiDataProvider;
        }
        
        protected async Task<IEnumerable<TEmoji>> GetDataAsync(CancellationToken cancellationToken = default) =>
            await EmojiDataProvider.GetDataAsync(cancellationToken);

        public async Task<IEnumerable<EmojiMatch<TEmoji, TVariation>>> GetEmojisAsync(string str, CancellationToken cancellationToken = default)
        {
            var data = await GetDataAsync(cancellationToken);
            var emojis = data.ToArray();
            
            var emojiMatches = new List<EmojiMatch<TEmoji, TVariation>>();
            var textElementEnumerator = StringInfo.GetTextElementEnumerator(str);
            
            while (textElementEnumerator.MoveNext())
            {
                var targetCharacter = textElementEnumerator.GetTextElement();
                if (!targetCharacter.IsUnicode())
                    continue;

                EmojiMatch<TEmoji, TVariation> match;
                TVariation targetVariation = default;
                var targetEmoji = emojis.FirstOrDefault(emoji =>
                    emoji.Unicode == targetCharacter || (targetVariation = emoji.Variations != null
                        ? emoji.Variations.FirstOrDefault(variation => variation.Unicode == targetCharacter)
                        : default) != null);

                if (targetVariation != null)
                {
                    if ((match = emojiMatches.FirstOrDefault(targetMatch => targetMatch.Variation != null && targetMatch.Variation.Equals(targetVariation))) != null)
                        match.Occurrences++;
                    else
                        emojiMatches.Add(new EmojiMatch<TEmoji, TVariation>(targetVariation));
                }
                else if (targetEmoji != null)
                {
                    if ((match = emojiMatches.FirstOrDefault(targetMatch => targetMatch.Emoji != null && targetMatch.Emoji.Equals(targetEmoji))) != null)
                        match.Occurrences++;
                    else
                        emojiMatches.Add(new EmojiMatch<TEmoji, TVariation>(targetEmoji));
                }
            }

            return emojiMatches;
        }

        public void Dispose()
        {
            EmojiDataProvider?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}