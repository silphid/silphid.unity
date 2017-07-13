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

    public class TabControl : MonoBehaviour, IPresenter, ISelectHandler, IMoveHandler, ICancelHandler
    {
        public float SelectionDelay = 0f;
        public SelectionControl TabSelectionControl;
        public TransitionControl ContentTransitionControl;
        public TabPlacement TabPlacement = TabPlacement.Top;
        
        private readonly MoveHandler _moveHandler = new MoveHandler();
        private Options _lastOptions;

        public void Start()
        {
            TabSelectionControl.Views
                .Select(x => x.FirstOrDefault())
                .BindTo(TabSelectionControl.SelectedView)
                .AddTo(this);

            TabSelectionControl.SelectedView
                .WhereNotNull() // TODO SelectionControl should keep selection but can't with current unity select system
                .LazyThrottle(TimeSpan.FromSeconds(SelectionDelay))
                .Select(x => x?.ViewModel?.Model)
                .Subscribe(x => ContentTransitionControl.Present(x, _lastOptions).SubscribeAndForget())
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

        public bool CanPresent(object input, Options options = null) =>
            TabSelectionControl.CanPresent(input, options);

        public IObservable<IView> Present(object input, Options options = null) =>
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