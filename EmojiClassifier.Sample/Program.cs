using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiClassifier.Sample
{
    public record EmojiVariation : IEmojiVariation<Emoji,EmojiVariation>
    {
        public string Id { get; set; }

        [JsonPropertyName("unified")]
        public string Unified { get; init; }

        public EmojiVariation()
        {
        }

        public EmojiVariation(Emoji parent, EmojiVariation emojiVariation, string variation)
        {
            Id = variation;
            Unified = emojiVariation.Unified;
            Parent = parent;
            Variation = variation?.ToUpper() switch
            {
                "1F3FC" => EmojiClassifier.EmojiVariation.LightSkinTone,
                "1F3FB" => EmojiClassifier.EmojiVariation.MediumLightSkinTone,
                "1F3FD" => EmojiClassifier.EmojiVariation.MediumSkinTone,
                "1F3FE" => EmojiClassifier.EmojiVariation.MediumDarkSkinTone,
                "1F3FF" => EmojiClassifier.EmojiVariation.DarkSkinTone,
                _ => EmojiClassifier.EmojiVariation.Default
            };
        }

        public Emoji Parent { get; }
        public EmojiClassifier.EmojiVariation Variation { get; }
        
        public string Unicode => Unified.Split('-').Aggregate("",
            (current, hex) => current + char.ConvertFromUtf32(Convert.ToInt32(hex, 16)));
    }

    public record Emoji : IEmoji<Emoji,EmojiVariation>
    {
        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("unified")]
        public string Unified { get; set; }

        public string Unicode => Unified.Split('-').Aggregate("",
            (current, hex) => current + char.ConvertFromUtf32(Convert.ToInt32(hex, 16)));

        [JsonPropertyName("skin_variations")]
        public Dictionary<String, EmojiVariation> SkinVariations
        {
            get => _variations.ToDictionary((variation => variation.Id));
            set => _variations = value?.Select(keyValuePair =>
                new EmojiVariation(this, keyValuePair.Value, keyValuePair.Key)
            );
        }

        private IEnumerable<EmojiVariation> _variations;

        public IEnumerable<EmojiVariation> Variations
        {
            get => _variations;
            init => _variations = value;
        }
    }

    public class GithubEmojiDataProvider : IEmojiDataProvider<Emoji,EmojiVariation>
    {
        private const string Url = "https://raw.githubusercontent.com/iamcal/emoji-data/master/emoji_pretty.json";
        private readonly HttpClient _httpClient = new HttpClient();
        private IEnumerable<Emoji> _emojis;

        ~GithubEmojiDataProvider()
        {
            Dispose(false);
        }

        public async Task<IEnumerable<Emoji>> GetDataAsync(CancellationToken cancellationToken = default) => _emojis ??=
            JsonSerializer.Deserialize<List<Emoji>>(await _httpClient.GetStringAsync(Url, cancellationToken));

        public IEnumerable<Emoji> GetData() => GetDataAsync().GetAwaiter().GetResult();

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            _httpClient?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var emojiProvider = new GithubEmojiDataProvider();
            using var emojiClassifier = new EmojiClassifier<Emoji,EmojiVariation>(emojiProvider);
            var str = await File.ReadAllTextAsync("file.txt");

            var emojis = await emojiClassifier.GetEmojisAsync(str);

            foreach (var emojiMatch in emojis)
            {
                Console.WriteLine(
                    $"Found {emojiMatch.Emoji.Name} {(emojiMatch.VariationName.ToUpper())} - {emojiMatch.Occurrences} Times");
            }
        }
    }
}