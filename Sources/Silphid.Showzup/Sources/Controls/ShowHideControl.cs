using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public class ShowHideControl : TransitionControl
    {
        public bool ShowInitially = true;
        
        private struct InputAndOptions
        {
            public object Input { get; }
            public Options Options { get; }

            public InputAndOptions(object input, Options options)
            {
                Input = input;
                Options = options;
            }
        }
        
        public ReactiveProperty<bool> Show { get; private set; }
        private readonly Subject<IView> _presentedView = new Subject<IView>();
        private readonly Subject<InputAndOptions> _inputAndOptions = new Subject<InputAndOptions>();
        private bool _isFirstPresent = true;

        internal override void Start()
        {
            base.Start();
            
            Show = new ReactiveProperty<bool>(ShowInitially);
            Show.CombineLatest(_inputAndOptions, (show, inputAndOptions) => show
                    ? inputAndOptions
                    : new InputAndOptions(null, inputAndOptions.Options.With(Direction.Backward)))
                .Subscribe(x => PresentInternal(x).SubscribeAndForget(view => _presentedView.OnNext(view)))
                .AddTo(this);
        }

        public override IObservable<IView> Present(object input, Options options = null)
        {
            _inputAndOptions.OnNext(new InputAndOptions(input, options));
            return _presentedView;
        }

        private IObservable<IView> PresentInternal(InputAndOptions x)
        {
            var options = _isFirstPresent && ShowInitially
                ? x.Options.With(InstantTransition.Instance)
                : x.Options;

            _isFirstPresent = false;
            
            return base.Present(x.Input, options);
        }
    }
}