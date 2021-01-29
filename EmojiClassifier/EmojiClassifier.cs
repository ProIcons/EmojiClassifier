using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiClassifier
{
    public class EmojiClassifier<TEmoji,TVariation> : IDisposable where TEmoji : IEmoji<TEmoji,TVariation> where TVariation : IEmojiVariation<TEmoji,TVariation>
    {
        protected IEmojiDataProvider<TEmoji,TVariation> EmojiDataProvider { get; }

        public EmojiClassifier(IEmojiDataProvider<TEmoji,TVariation> emojiDataProvider)
        {
            EmojiDataProvider = emojiDataProvider;
        }

        ~EmojiClassifier()
        {
            Dispose(false);
        }

        protected async Task<IEnumerable<TEmoji>> GetDataAsync(CancellationToken cancellationToken = default) =>
            await EmojiDataProvider.GetDataAsync(cancellationToken);

        public virtual async Task<IEnumerable<EmojiMatch<TEmoji,TVariation>>> GetEmojisAsync(string str,
            CancellationToken cancellationToken = default)
        {
            var data = await GetDataAsync(cancellationToken);
            var emojiMatches = new List<EmojiMatch<TEmoji, TVariation>>();
            foreach (var emoji in data)
            {
                var variationAdded = false;
                EmojiMatch<TEmoji,TVariation> persistedMatch;
                EmojiMatch<TEmoji,TVariation> match;
                int matchCount;

                if (emoji.Variations != null)
                {
                    foreach (var variation in emoji.Variations)
                    {
                        matchCount = Regex.Matches(str, Regex.Escape(variation.Unicode)).Count;
                        if (matchCount <= 0) continue;
                        match = new EmojiMatch<TEmoji, TVariation>(variation, matchCount);
                        persistedMatch =
                            emojiMatches.FirstOrDefault(targetMatch => targetMatch.Variation != null && targetMatch.Variation.Equals(match.Variation));
                        if (persistedMatch == null)
                        {
                            emojiMatches.Add(match);
                        }
                        else
                        {
                            persistedMatch.Occurrences += matchCount;
                        }

                        variationAdded = true;
                    }
                }

                matchCount = Regex.Matches(str, Regex.Escape(emoji.Unicode)).Count;
                if (variationAdded || matchCount <= 0) continue;
                match = new EmojiMatch<TEmoji, TVariation>(emoji, matchCount);
                persistedMatch = emojiMatches.FirstOrDefault(targetMatch => targetMatch.Emoji != null && targetMatch.Emoji.Equals(match.Emoji));
                if (persistedMatch == null)
                {
                    emojiMatches.Add(match);
                }
                else
                {
                    persistedMatch.Occurrences += matchCount;
                }
            }

            foreach (var match in emojiMatches)
            {
                var partialEmojis = new Dictionary<EmojiMatch<TEmoji, TVariation>, bool>();
                var matchUnicode = match.Variation != null ? match.Variation.Unicode : match.Emoji.Unicode;
                foreach (var partialMatch in emojiMatches.Where(partialEmoji => partialEmoji != match))
                {
                    var partialMatchUnicode = partialMatch.Variation != null
                        ? partialMatch.Variation.Unicode
                        : partialMatch.Emoji.Unicode;
                    if (matchUnicode.Contains(partialMatchUnicode))
                    {
                        partialEmojis.Add(partialMatch, true);
                    }
                }

                if (matchUnicode.Length >= partialEmojis.Count)
                {
                    foreach (var targetEmoji in partialEmojis.Keys)
                    {
                        targetEmoji.Occurrences -= 1;
                    }
                }
            }

            return emojiMatches.Where(emojiOccurrence => emojiOccurrence.Occurrences > 0);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            EmojiDataProvider?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}