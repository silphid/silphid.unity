using System;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    public class SelectionControl : ListControl, IMoveHandler
    {
        private readonly ReactiveProperty<IView> _selectedView = new ReactiveProperty<IView>();

        public ReadOnlyReactiveProperty<IView> SelectedView => _selectedView.ToReadOnlyReactiveProperty();
        public ReactiveProperty<int?> SelectedIndex { get; } = new ReactiveProperty<int?>();

        public NavigationOrientation Orientation;
        public bool WrapAround;
        public int RowsOrColumns = 1;

        public bool AutoSelectFirst = true;

        public override GameObject ForwardSelection()
        {
            return _selectedView.Value?.GameObject;
        }

        protected override void Start()
        {
            if (Orientation == NavigationOrientation.None)
                throw new InvalidOperationException(
                    $"SelectionControl is missing orientation value on gameObject {gameObject.ToHierarchyPath()}");

            Views
                .CombineLatest(SelectedIndex, (views, selectedIndex) => new {views, selectedIndex})
                .Subscribe(x =>
                {
                    _selectedView.Value = x.views.GetAtOrDefault(x.selectedIndex);

                    if (IsSelfOrDescendantSelected.Value)
                        _selectedView.Value?.Select();
                })
                .AddTo(this);
            
            SubscribeToUpdateFocusables(SelectedView);
        }
        
        private void SubscribeToUpdateFocusables<T>(IObservable<T> observable)
        {
            observable
                .PairWithPreviousOrDefault()
                .Subscribe(x =>
                {
                    RemoveFocus(x.Item1 as IFocusable);
                    SetFocus(x.Item2 as IFocusable);
                })
                .AddTo(this);
        }
        
        private void SetFocus(IFocusable focusable)
        {
            if (focusable == null)
                return;

            focusable.IsFocused.Value = true;
        }

        private void RemoveFocus(IFocusable focusable)
        {
            if (focusable == null)
                return;

            focusable.IsFocused.Value = false;
        }

        [Pure]
        public override IObservable<IView> Present(object input, Options options = null) =>
            base.Present(input, options).DoOnCompleted(() =>
            {
                if (AutoSelectFirst)
                    SelectFirst();
            });

        protected override void RemoveAllViews(GameObject container, GameObject except = null)
        {
            base.RemoveAllViews(container, except);

            SelectedIndex.Value = null;
        }

        public bool SelectIndex(int index)
        {
            if (index >= Views.Value.Count)
                return false;

            SelectedIndex.Value = index;

            return true;
        }

        public bool SelectFirst()
        {
            if (!HasItems)
                return false;

            SelectedIndex.Value = FirstIndex;
            return true;
        }

        public bool SelectLast()
        {
            if (!HasItems)
                return false;

            SelectedIndex.Value = LastIndex;
            return true;
        }

        public void SelectNone()
        {
            SelectedIndex.Value = null;
        }

        public bool SelectPrevious()
        {
            if (!HasItems)
                return false;

            if (SelectedIndex.Value == FirstIndex)
            {
                if (WrapAround)
                {
                    SelectedIndex.Value = LastIndex;
                    return true;
                }

                return false;
            }

            SelectedIndex.Value--;
            return true;
        }

        public bool SelectNext()
        {
            if (!HasItems)
                return false;

            if (SelectedIndex.Value == LastIndex)
            {
                if (WrapAround)
                {
                    SelectedIndex.Value = 0;
                    return true;
                }

                return false;
            }

            SelectedIndex.Value++;
            return true;
        }

        public void OnMove(AxisEventData eventData)
        {
            if (!HasItems || SelectedIndex.Value == null)
                return;

            var moveDirection = Orientation == NavigationOrientation.Vertical
                ? eventData.moveDir.FlipXY()
                : eventData.moveDir;

            if (moveDirection == MoveDirection.Up && SelectedIndex.Value % RowsOrColumns > 0 &&
                SelectIndex(SelectedIndex.Value.Value - 1) ||
                moveDirection == MoveDirection.Down && SelectedIndex.Value % RowsOrColumns < RowsOrColumns - 1 &&
                SelectIndex(SelectedIndex.Value.Value + 1) ||
                moveDirection == MoveDirection.Left && SelectedIndex.Value >= RowsOrColumns &&
                SelectIndex(SelectedIndex.Value.Value - RowsOrColumns) ||
                moveDirection == MoveDirection.Right && SelectedIndex.Value + RowsOrColumns < Views.Value.Count &&
                SelectIndex(SelectedIndex.Value.Value + RowsOrColumns))
                eventData.Use();
        }
    }
}