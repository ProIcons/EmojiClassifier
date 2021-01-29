using System;
using System.Collections.Generic;

namespace EmojiClassifier
{
    public record Emoji
    {
        public virtual string Name { get; init; }
        public virtual string Unicode { get; init; }
        public virtual IEnumerable<Variation> Variations { get; init; }
        
        public record Variation
        {
            public virtual Emoji Parent { get; init; }
            public virtual String Unicode { get; init; }
            public virtual EmojiVariation EmojiVariation { get; init; }
        }
    }
}