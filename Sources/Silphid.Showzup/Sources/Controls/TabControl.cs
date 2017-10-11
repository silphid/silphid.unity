using System;
using System.Linq;
using Silphid.Extensions;
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
        public float SelectionDelay;
        public SelectionControl TabSelectionControl;
        public PresenterControl ContentTransitionControl;
        public TabPlacement TabPlacement = TabPlacement.Top;

        private readonly MoveHandler _moveHandler = new MoveHandler();
        private Options _lastOptions;
        private readonly Subject<object> _presentingContent = new Subject<object>();

        private readonly ReactiveProperty<IView> _contentView = new ReactiveProperty<IView>();
        public ReadOnlyReactiveProperty<IView> ContentView => _contentView.ToReadOnlyReactiveProperty();
        public IObservable<object> PresentingContent => _presentingContent;
        private int _currentIndex;

        public void Start()
        {
            TabSelectionControl.Views
                .Select(x => x.FirstOrDefault())
                .BindTo(TabSelectionControl.SelectedView)
                .AddTo(this);

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


        public override IObservable<IView> Present(object input, Options options = null) =>
            TabSelectionControl.Present(input, _lastOptions = options);

        public override IReadOnlyReactiveProperty<PresenterState> State => TabSelectionControl.State;

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
            return Observable.Return(view)
                .Select(x => (x?.ViewModel as IContentProvider)?.GetContent() ?? x?.ViewModel?.Model)
                .Do(x => _presentingContent.OnNext(x))
                .SelectMany(x => ContentTransitionControl
                    .With(direction)
                    .Present(x, _lastOptions));
        }
    }
}