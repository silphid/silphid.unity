using System;
using Silphid.Extensions;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    public enum TabPlacement
    {
        Top = MoveDirection.Up,
        Bottom = MoveDirection.Down,
        Left = MoveDirection.Left,
        Right = MoveDirection.Right
    }

    public class TabControl : PresenterControl, IMoveHandler, ICancelHandler
    {
        private readonly ReactiveProperty<IView> _contentView = new ReactiveProperty<IView>();
        private int _currentIndex;
        private readonly MoveHandler _moveHandler = new MoveHandler();
        private Options _lastOptions;

        public float SelectionDelay;
        public SelectionControl TabSelectionControl;
        public PresenterControl ContentTransitionControl;
        public TabPlacement TabPlacement = TabPlacement.Top;
        public bool UseIntuitiveTransitionDirection = true;

        public ReadOnlyReactiveProperty<IView> ContentView => _contentView.ToReadOnlyReactiveProperty();

        public override GameObject ForwardSelection() => TabSelectionControl.gameObject;

        public void Start()
        {
            _currentIndex = TabSelectionControl.SelectedIndex.Value ?? 0;

            TabSelectionControl.SelectedIndex
                .WhereNotNull()
                .LazyThrottle(TimeSpan.FromSeconds(SelectionDelay))
                .DistinctUntilChanged()
                .SelectMany(x => ShowContent(x ?? 0))
                .Subscribe(x => _contentView.Value = x)
                .AddTo(this);

            _moveHandler.BindBidirectional(
                ContentTransitionControl,
                TabSelectionControl,
                (MoveDirection) TabPlacement,
                () => _currentIndex == TabSelectionControl.SelectedIndex.Value && !IsPresenting.Value &&
                      ContentTransitionControl.FirstView.Value != null);

            _moveHandler.BindCancel(
                ContentTransitionControl,
                TabSelectionControl);
        }

        public override IObservable<IView> Present(object input, Options options = null) =>
            TabSelectionControl.Present(input, _lastOptions = options);

        public override ReadOnlyReactiveProperty<bool> IsLoading =>
            TabSelectionControl.IsLoading
                .Merge(ContentTransitionControl.IsLoading)
                .ToReadOnlyReactiveProperty();

        public override ReadOnlyReactiveProperty<bool> IsPresenting =>
            TabSelectionControl.IsPresenting
                .Merge(ContentTransitionControl.IsPresenting)
                .ToReadOnlyReactiveProperty();

        public void OnMove(AxisEventData eventData)
        {
            _moveHandler.OnMove(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            _moveHandler.OnCancel(eventData);
        }

        private IObservable<IView> ShowContent(int index)
        {
            var view = TabSelectionControl.GetViewAtIndex(index);
            var direction = _currentIndex > index && UseIntuitiveTransitionDirection
                ? Direction.Backward
                : Direction.Forward;
            
            _currentIndex = index;
            
            return Observable.Return(view)
                .Select(x => (x?.ViewModel as IContentProvider)?.GetContent() ?? x?.ViewModel?.Model)
                .SelectMany(x => ContentTransitionControl
                    .With(direction)
                    .Present(x, _lastOptions));
        }
    }
}