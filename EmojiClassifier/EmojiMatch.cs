namespace EmojiClassifier
{
    public class EmojiMatch<TEmoji, TVariation> where TEmoji : IEmoji<TEmoji, TVariation>
                                                where TVariation : IEmojiVariation<TEmoji, TVariation>
    {
        public TEmoji Emoji { get; }
        public TVariation Variation { get; }
        public int Occurrences { get; set; }

        public virtual string VariationName
        {
            get
            {
                switch (Variation?.Variation)
                {
                    case EmojiVariation.LightSkinTone:
                        return "Light Skin Tone";
                    case EmojiVariation.MediumLightSkinTone:
                        return "Medium Light Skin Tone";
                    case EmojiVariation.MediumSkinTone:
                        return "Medium Skin Tone";
                    case EmojiVariation.MediumDarkSkinTone:
                        return "Medium Dark Skin Tone";
                    case EmojiVariation.DarkSkinTone:
                        return "Dark Skin Tone";
                    default:
                        return "";
                }
            }
        }

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