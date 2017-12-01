using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public class ShowHideControl : TransitionControl
    {
        public bool ShowInitially;
        public bool ShowInstantlyInitially;
        
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
        
        public IReactiveProperty<bool> Show { get; } = new ReactiveProperty<bool>();
        private readonly ISubject<IView> _presentedView = new Subject<IView>();
        private readonly ISubject<InputAndOptions> _inputAndOptions = new ReplaySubject<InputAndOptions>(1);
        private bool _isFirstPresent = true;

        internal void Start()
        {
            Show.Value = ShowInitially;
            Show.CombineLatest(_inputAndOptions, (show, inputAndOptions) => show
                    ? inputAndOptions
                    : new InputAndOptions(null, inputAndOptions.Options.With(Direction.Backward)))
                .Subscribe(x => PresentInternal(x).SubscribeAndForget(view => _presentedView.OnNext(view)))
                .AddTo(this);
        }

        protected override IObservable<IView> PresentView(object input, Options options = null)
        {
            _inputAndOptions.OnNext(new InputAndOptions(input, options));
            return _presentedView;
        }

        private IObservable<IView> PresentInternal(InputAndOptions x)
        {
            var options = _isFirstPresent && ShowInstantlyInitially
                ? x.Options.With(InstantTransition.Instance)
                : x.Options;

            _isFirstPresent = false;
            
            return base.PresentView(x.Input, options);
        }
    }
}