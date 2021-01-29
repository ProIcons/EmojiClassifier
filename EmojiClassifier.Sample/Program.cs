using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmojiClassifier.Sample
{
    public record DEmojiVariation : Emoji.Variation
    {
        public string Id { get; set; }

        [JsonPropertyName("unified")]
        public string Unified { get; init; }

        public DEmojiVariation()
        {
        }

        public DEmojiVariation(DEmoji parent, DEmojiVariation emojiVariation, string variation)
        {
            Id = variation;
            Unified = emojiVariation.Unified;
            Parent = parent;
            EmojiVariation = variation?.ToUpper() switch
            {
                "1F3FC" => EmojiVariation.LightSkinTone,
                "1F3FB" => EmojiVariation.MediumLightSkinTone,
                "1F3FD" => EmojiVariation.MediumSkinTone,
                "1F3FE" => EmojiVariation.MediumDarkSkinTone,
                "1F3FF" => EmojiVariation.DarkSkinTone,
                _ => EmojiVariation.Default
            };
        }

        public override string Unicode => Unified.Split('-').Aggregate("",
            (current, hex) => current + char.ConvertFromUtf32(Convert.ToInt32(hex, 16)));
    }

    public record DEmoji : Emoji
    {
        [JsonPropertyName("name")]
        public override string Name { get; init; }

        [JsonPropertyName("unified")]
        public string Unified { get; set; }

        public override string Unicode => Unified.Split('-').Aggregate("",
            (current, hex) => current + char.ConvertFromUtf32(Convert.ToInt32(hex, 16)));

        [JsonPropertyName("skin_variations")]
        public Dictionary<String, DEmojiVariation> SkinVariations
        {
            get => _variations.ToDictionary((variation => variation.Id));
            set => _variations = value?.Select(keyValuePair =>
                new DEmojiVariation(this, keyValuePair.Value, keyValuePair.Key)
            );
        }

        private IEnumerable<DEmojiVariation> _variations;

        public override IEnumerable<Emoji.Variation> Variations
        {
            get => _variations;
            init => _variations = value.Cast<DEmojiVariation>();
        }
    }

    public class GithubEmojiDataProvider : IEmojiDataProvider, IDisposable
    {
        private const string Url = "https://raw.githubusercontent.com/iamcal/emoji-data/master/emoji_pretty.json";
        public IEnumerable<Emoji> GetData() => GetDataAsync().GetAwaiter().GetResult();
        private readonly HttpClient _httpClient = new HttpClient();
        private IEnumerable<Emoji> _emojis;
        ~GithubEmojiDataProvider()
        {
            Dispose(false);
        }

        public async Task<IEnumerable<Emoji>> GetDataAsync() => _emojis ??= JsonSerializer.Deserialize<List<DEmoji>>(await _httpClient.GetStringAsync(Url));

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
            using var emojiClassifier = new EmojiClassifier(emojiProvider);
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