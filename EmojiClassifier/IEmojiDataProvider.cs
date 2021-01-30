using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiClassifier
{
    public interface IEmojiDataProvider<TEmoji, TVariation> : IDisposable
        where TEmoji : IEmoji<TEmoji, TVariation> where TVariation : IEmojiVariation<TEmoji, TVariation>
    {
        IEnumerable<TEmoji> GetData();
        Task<IEnumerable<TEmoji>> GetDataAsync(CancellationToken cancellationToken = default);
    }
}