using System;
using Silphid.Extensions;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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
        private readonly NavigationHandler _navigationHandler = new NavigationHandler();
        private readonly Subject<object> _presentingContent = new Subject<object>();
        private readonly ReactiveProperty<IView> _contentView = new ReactiveProperty<IView>();
        private Options _lastOptions;
        private int _chosenIndex;
        private ReadOnlyReactiveProperty<PresenterState> _state;

        public float SelectionDelay;
        
        [FormerlySerializedAs("TabSelectionControl")]
        public ListControl TabListControl;
        public PresenterControl ContentTransitionControl;
        public TabPlacement TabPlacement = TabPlacement.Top;
        public bool UseIntuitiveTransitionDirection = true;
        public ReadOnlyReactiveProperty<IView> ContentView => _contentView.ToReadOnlyReactiveProperty();
        public IObservable<object> PresentingContent => _presentingContent;

        public override GameObject SelectableContent => TabListControl.gameObject;

        public void Start()
        {
            _chosenIndex = TabListControl.ChosenIndex.Value ?? 0;

            TabListControl.ChosenIndex
                .WhereNotNull()
                .LazyThrottle(TimeSpan.FromSeconds(SelectionDelay))
                .DistinctUntilChanged()
                .SelectMany(x => ShowContent(x ?? 0))
                .Subscribe(x => _contentView.Value = x)
                .AddTo(this);

            _navigationHandler.BindBidirectional(
                ContentTransitionControl,
                TabListControl,
                (MoveDirection) TabPlacement,
                () => _chosenIndex == TabListControl.ChosenIndex.Value && MutableState.Value != PresenterState.Presenting
                      && ContentTransitionControl.FirstView.Value != null);
            
            _navigationHandler.BindCancel(
                ContentTransitionControl,
                TabListControl);
        }

        protected override IObservable<IView> PresentView(object input, Options options = null) =>
            TabListControl
                .Present(input, _lastOptions = options);

        public override IReadOnlyReactiveProperty<PresenterState> State =>
            _state
            ?? (_state = TabListControl.State
                .CombineLatest(ContentTransitionControl.State, (x, y) =>
                {
                    if (x == PresenterState.Loading || y == PresenterState.Loading)
                        return PresenterState.Loading;

                    if (x == PresenterState.Presenting || y == PresenterState.Presenting)
                        return PresenterState.Presenting;

                    return PresenterState.Ready;
                })
                .ToReadOnlyReactiveProperty());


        public void OnMove(AxisEventData eventData)
        {
            _navigationHandler.OnMove(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            _navigationHandler.OnCancel(eventData);
        }

        private IObservable<IView> ShowContent(int index)
        {
            var view = TabListControl.GetViewAtIndex(index);
            var direction = _chosenIndex > index && UseIntuitiveTransitionDirection
                ? Direction.Backward
                : Direction.Forward;
            
            _chosenIndex = index;
            var model = (view?.ViewModel as IContentProvider)?.GetContent() ?? view?.ViewModel?.Model;
            _presentingContent.OnNext(model);
            return ContentTransitionControl
                .With(direction)
                .Present(model, _lastOptions);
        }
    }
}