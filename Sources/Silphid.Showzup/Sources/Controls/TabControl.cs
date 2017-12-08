using System;
using Silphid.Extensions;
using Silphid.Requests;
using Silphid.Showzup.Navigation;
using UniRx;
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
        private readonly MoveHandler _moveHandler = new MoveHandler();
        private readonly Subject<object> _presentingContent = new Subject<object>();
        private readonly ReactiveProperty<IView> _contentView = new ReactiveProperty<IView>();
        private Options _lastOptions;
        private int _currentIndex;
        private ReadOnlyReactiveProperty<PresenterState> _state;

        public float SelectionDelay;
        public SelectionControl TabSelectionControl;
        public PresenterControl ContentTransitionControl;
        public TabPlacement TabPlacement = TabPlacement.Top;
        public ReadOnlyReactiveProperty<IView> ContentView => _contentView.ToReadOnlyReactiveProperty();
        public IObservable<object> PresentingContent => _presentingContent;

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
                (MoveDirection) TabPlacement);

            // Combining with view to select gameobject when view is loaded
            _moveHandler.SelectedGameObject
                .WhereNotNull()
                .CombineLatest(ContentTransitionControl.FirstView, (x, y) => x)
                .Where(x => x == ContentTransitionControl.gameObject)
                .Subscribe(x => ContentTransitionControl.FirstView.Value?.SelectDeferred())
                .AddTo(this);
        }

        protected override IObservable<IView> PresentView(object input, Options options = null) =>
            TabSelectionControl
                .Present(input, _lastOptions = options);

        public override IReadOnlyReactiveProperty<PresenterState> State =>
            _state
            ?? (_state = TabSelectionControl.State
                .CombineLatest(ContentTransitionControl.State, (x, y) =>
                {
                    if (x == PresenterState.Loading || y == PresenterState.Loading)
                        return PresenterState.Loading;

                    if (x == PresenterState.Presenting || y == PresenterState.Presenting)
                        return PresenterState.Presenting;

                    return PresenterState.Ready;
                })
                .ToReadOnlyReactiveProperty());

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            TabSelectionControl.SelectFirst();
        }

        public void OnMove(AxisEventData eventData)
        {
            _moveHandler.OnMove(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (!TabSelectionControl.IsSelfOrDescendantSelected())
            {
                _moveHandler.OnMove(new AxisEventData(EventSystem.current) {moveDir = MoveDirection.Up});
                eventData.Use();
            }
        }

        private IObservable<IView> ShowContent(int index)
        {
            var view = TabSelectionControl.GetViewAtIndex(index);
            var direction = _currentIndex > index ? Direction.Backward : Direction.Forward;
            _currentIndex = index;
            var model = (view?.ViewModel as IContentProvider)?.GetContent() ?? view?.ViewModel?.Model;
            _presentingContent.OnNext(model);
            return ContentTransitionControl
                .With(direction)
                .Present(model, _lastOptions);
        }
    }
}