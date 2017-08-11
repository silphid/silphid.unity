using System;
using UniRx;

namespace Silphid.Showzup.Requests
{
    public interface IRequestHandler
    {
        bool CanHandle(IRequest request);
        
        /// <returns>Null if cannot handle request.</returns>
        IObservable<Unit> Handle(IRequest request);
    }
}