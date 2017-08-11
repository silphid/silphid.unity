using System;
using System.Linq;
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

    public class TabControl : PresenterControl, IPresenter, ISelectHandler, IMoveHandler, ICancelHandler
    {
        public float SelectionDelay;
        public SelectionControl TabSelectionControl;
        public TransitionControl ContentTransitionControl;
        public TabPlacement TabPlacement = TabPlacement.Top;
        
        private readonly MoveHandler _moveHandler = new MoveHandler();
        private Options _lastOptions;
        
        private readonly ReactiveProperty<IView> _contentView = new ReactiveProperty<IView>();
        public ReadOnlyReactiveProperty<IView> ContentView => _contentView.ToReadOnlyReactiveProperty();

        public void Start()
        {
            TabSelectionControl.Views
                .Select(x => x.FirstOrDefault())
                .BindTo(TabSelectionControl.SelectedView)
                .AddTo(this);

            TabSelectionControl.SelectedView
                .WhereNotNull() // TODO SelectionControl should keep selection but can't with current unity select system
                .LazyThrottle(TimeSpan.FromSeconds(SelectionDelay))
                .SelectMany(x => (x?.ViewModel as IContentProvider)?.GetContent() ?? Observable.Return(x?.ViewModel?.Model))
                .SelectMany(x => ContentTransitionControl.Present(x, _lastOptions))
                .Subscribe(x => _contentView.Value = x)
                .AddTo(this);
            
            _moveHandler.BindBidirectional(
                ContentTransitionControl,
                TabSelectionControl,
                (MoveDirection) TabPlacement);

            // Combining with view to select gameobject when view is loaded
            _moveHandler.SelectedGameObject
                .CombineLatest(ContentTransitionControl.View, (x, y) => x)
                .Where(x => x == ContentTransitionControl.gameObject)
                .Subscribe(x => ContentTransitionControl.View.Value?.SelectDeferred())
                .AddTo(this);
        }

        public override bool CanPresent(object input, Options options = null) =>
            TabSelectionControl.CanPresent(input, options);

        public override IObservable<IView> Present(object input, Options options = null) =>
            TabSelectionControl.Present(input, _lastOptions = options);

        public void OnSelect(BaseEventData eventData)
        {
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
                _moveHandler.OnMove(new AxisEventData(EventSystem.current) { moveDir = MoveDirection.Up});
                eventData.Use();
            }
        }
    }
}