using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmojiClassifier
{
    public class EmojiClassifier : IDisposable
    {
        private readonly IEmojiDataProvider _emojiDataProvider;

        private IEnumerable<Emoji> _emojiData;

        public EmojiClassifier(IEmojiDataProvider emojiDataProvider)
        {
            _emojiDataProvider = emojiDataProvider;
        }

        ~EmojiClassifier()
        {
            Dispose(false);
        }

        private async Task<IEnumerable<Emoji>> GetDataAsync() =>
            _emojiData ??= await _emojiDataProvider.GetDataAsync();

        public async Task<IEnumerable<EmojiMatch>> GetEmojisAsync(string str)
        {
            var data = await GetDataAsync();
            List<EmojiMatch> emojiMatches = new();
            foreach (var emoji in data)
            {
                var variationAdded = false;
                EmojiMatch persistedMatch;
                EmojiMatch match;
                int matchCount;

                if (emoji.Variations != null)
                {
                    foreach (var variation in emoji.Variations)
                    {
                        matchCount = Regex.Matches(str, Regex.Escape(variation.Unicode)).Count;
                        if (matchCount <= 0) continue;
                        match = new(variation, matchCount);
                        persistedMatch =
                            emojiMatches.FirstOrDefault(targetMatch => targetMatch.Variation == match.Variation);
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
                match = new(emoji, matchCount);
                persistedMatch = emojiMatches.FirstOrDefault(targetMatch => targetMatch.Emoji == match.Emoji);
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
                Dictionary<EmojiMatch, bool> partialEmojis = new();
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

            _emojiDataProvider?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}