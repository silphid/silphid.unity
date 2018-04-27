using Silphid.Loadzup;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Silphid.Showzup
{
    public class CancellationOnDestroyLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly GameObject _gameObject;
        
        public CancellationOnDestroyLoaderDecorator(ILoader loader, GameObject gameObject) : base(loader)
        {
            _gameObject = gameObject;
        }

        protected override void UpdateOptions(Loadzup.Options options)
        {
            var cancellationToken = new CancellationDisposable();
            
            _gameObject
                .OnDestroyAsObservable()
                .Take(1)
                .Subscribe(_ => cancellationToken.Dispose());
            
            options.CancellationToken = cancellationToken;
        }
    }
}