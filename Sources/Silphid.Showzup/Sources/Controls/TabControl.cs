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

        public void Start()
        {
            TabSelectionControl.Views
                .Select(x => x.FirstOrDefault())
                .BindTo(TabSelectionControl.SelectedView)
                .AddTo(this);

            TabSelectionControl.SelectedView
                .Select(x => x?.ViewModel?.Model)
                .BindTo(ContentTransitionControl);
            
            _moveHandler.BindBidirectional(
                ContentTransitionControl,
                TabSelectionControl,
                (MoveDirection) TabPlacement);
        }

        public bool CanPresent(object input, Options options = null) =>
            TabSelectionControl.CanPresent(input, options);

        public IObservable<IView> Present(object input, Options options = null) =>
            TabSelectionControl.Present(input, options);

        public void OnSelect(BaseEventData eventData)
        {
            TabSelectionControl.Select();
        }

        public void OnMove(AxisEventData eventData)
        {
            _moveHandler.OnMove(eventData);
        }
    }
}