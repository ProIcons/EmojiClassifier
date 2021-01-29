using System.Collections.Generic;

namespace EmojiClassifier
{
    public interface IEmoji<TEmoji, TVariation> where TEmoji : IEmoji<TEmoji, TVariation>
        where TVariation : IEmojiVariation<TEmoji, TVariation>
    {
        public string Unicode { get; }
        public IEnumerable<TVariation> Variations { get; }
    }
}