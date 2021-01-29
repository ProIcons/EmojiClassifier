using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiClassifier
{
    public interface IEmojiDataProvider : IDisposable
    {
        IEnumerable<Emoji> GetData();
        Task<IEnumerable<Emoji>> GetDataAsync(CancellationToken cancellationToken = default);
    }
}