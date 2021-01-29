namespace EmojiClassifier
{
    public class EmojiMatch<TEmoji, TVariation> where TEmoji : IEmoji<TEmoji, TVariation>
        where TVariation : IEmojiVariation<TEmoji, TVariation>
    {
        public TEmoji Emoji { get; }
        public TVariation Variation { get; }
        public int Occurrences { get; set; }

        public virtual string VariationName => Variation?.Variation switch
        {
            EmojiVariation.LightSkinTone => "Light Skin Tone",
            EmojiVariation.MediumLightSkinTone => "Medium Light Skin Tone",
            EmojiVariation.MediumSkinTone => "Medium Skin Tone",
            EmojiVariation.MediumDarkSkinTone => "Medium Dark Skin Tone",
            EmojiVariation.DarkSkinTone => "Dark Skin Tone",
            _ => ""
        };

        public EmojiMatch(TVariation variation, int occurrences = 1)
        {
            Emoji = variation.Parent;
            Variation = variation;
            Occurrences = occurrences;
        }

        public EmojiMatch(TEmoji emoji, int occurrences = 1)
        {
            Emoji = emoji;
            Variation = default;
            Occurrences = occurrences;
        }
    }
}