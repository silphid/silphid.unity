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

    public class TabControl : MonoBehaviour, IPresenter, ISelectHandler, IMoveHandler
    {
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
                .Select(x => x?.ViewModel?.Model)
                .Subscribe(x => ContentTransitionControl.Present(x, _lastOptions).SubscribeAndForget())
                .AddTo(this);
            
            _moveHandler.BindBidirectional(
                ContentTransitionControl,
                TabSelectionControl,
                (MoveDirection) TabPlacement);

            _moveHandler.SelectedGameObject
                .Where(x => x == ContentTransitionControl.gameObject)
                .Subscribe(x => ContentTransitionControl.View.Value.SelectDeferred())
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
    }
}