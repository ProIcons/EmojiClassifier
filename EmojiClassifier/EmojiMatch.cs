namespace EmojiClassifier
{
    public class EmojiMatch
    {
        public Emoji Emoji { get; }
        public Emoji.Variation Variation { get; }
        public int Occurrences { get; set; }

        public virtual string VariationName => Variation?.EmojiVariation switch
        {
            EmojiVariation.LightSkinTone => "Light Skin Tone",
            EmojiVariation.MediumLightSkinTone => "Medium Light Skin Tone",
            EmojiVariation.MediumSkinTone => "Medium Skin Tone",
            EmojiVariation.MediumDarkSkinTone => "Medium Dark Skin Tone",
            EmojiVariation.DarkSkinTone => "Dark Skin Tone",
            _ => ""
        };

        public EmojiMatch(Emoji.Variation variation, int occurrences = 1)
        {
            Emoji = variation.Parent;
            Variation = variation;
            Occurrences = occurrences;
        }

        public EmojiMatch(Emoji emoji, int occurrences = 1)
        {
            Emoji = emoji;
            Variation = null;
            Occurrences = occurrences;
        }
    }
}