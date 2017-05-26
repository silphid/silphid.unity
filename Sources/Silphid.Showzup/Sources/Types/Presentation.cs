using System;
using UniRx;

namespace Silphid.Showzup
{
    public class Presentation
    {
        public object SourceViewModel => SourceView?.ViewModel;
        public object TargetViewModel { get; }
        public IView SourceView { get; }
        public IView TargetView { get; set; }
        public Type SourceViewType => SourceView?.GetType();
        public Type TargetViewType { get; }
        public Options Options { get; }
        public Transition Transition { get; set; }
        public Direction Direction => Options?.Direction ?? Direction.Forward;
        public float Duration { get; set; }
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        public Presentation(object viewModel, IView sourceView, Type targetViewType, Options options)
        {
            TargetViewModel = viewModel;
            SourceView = sourceView;
            TargetViewType = targetViewType;
            Options = options;
        }

        public override string ToString() =>
            $"{nameof(SourceViewModel)}: {SourceViewModel}, {nameof(TargetViewModel)}: {TargetViewModel}, " +
            $"{nameof(SourceView)}: {SourceView}, {nameof(TargetView)}: {TargetView}, {nameof(SourceViewType)}: {SourceViewType}, " +
            $"{nameof(TargetViewType)}: {TargetViewType}, {nameof(Options)}: {Options}, {nameof(Transition)}: {Transition}, " +
            $"{nameof(Duration)}: {Duration}";
    }
}