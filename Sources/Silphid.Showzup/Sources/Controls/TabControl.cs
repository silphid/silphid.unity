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
        public float SelectionDelay;
        public SelectionControl TabSelectionControl;
        public PresenterControl ContentTransitionControl;
        public TabPlacement TabPlacement = TabPlacement.Top;

        private readonly MoveHandler _moveHandler = new MoveHandler();
        private Options _lastOptions;

        private readonly ReactiveProperty<IView> _contentView = new ReactiveProperty<IView>();
        public ReadOnlyReactiveProperty<IView> ContentView => _contentView.ToReadOnlyReactiveProperty();
        private int _currentIndex;

        public override GameObject ForwardSelection() => TabSelectionControl.gameObject;

        public void Start()
        {
            /*IsFocused
                .WhereTrue()
                .Subscribe(_ => TabSelectionControl.Focus());*/

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
                (MoveDirection) TabPlacement);

            _moveHandler.BindCancel(
                ContentTransitionControl,
                TabSelectionControl);
        }

        public override IObservable<IView> Present(object input, Options options = null) =>
            TabSelectionControl.Present(input, _lastOptions = options);

        public override ReadOnlyReactiveProperty<bool> IsLoading => TabSelectionControl.IsLoading;

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
            var direction = _currentIndex > index ? Direction.Backward : Direction.Forward;
            _currentIndex = index;
            return Observable.Return(view)
                .Select(x => (x?.ViewModel as IContentProvider)?.GetContent() ?? x?.ViewModel?.Model)
                .SelectMany(x => ContentTransitionControl
                    .With(direction)
                    .Present(x, _lastOptions));
        }
    }
}