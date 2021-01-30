namespace EmojiClassifier
{
    public interface IEmojiVariation<TEmoji, TVariation> where TVariation : IEmojiVariation<TEmoji, TVariation> where TEmoji : IEmoji<TEmoji, TVariation>
    {
        TEmoji Parent { get; }
        EmojiVariation Variation { get; }
        string Unicode { get; }
    }
}