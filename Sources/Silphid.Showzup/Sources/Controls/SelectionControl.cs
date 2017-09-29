using System;
using Silphid.Extensions;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    public class SelectionControl : ListControl, IMoveHandler
    {
        private bool _isSynching;
        private readonly ReactiveProperty<IView> _lastSelectedView = new ReactiveProperty<IView>();
        private ReadOnlyReactiveProperty<IView> _lastSelectedViewReadOnly;

        public ReactiveProperty<IView> SelectedView { get; } = new ReactiveProperty<IView>();
        public ReactiveProperty<int?> SelectedIndex { get; } = new ReactiveProperty<int?>();

        public ReadOnlyReactiveProperty<IView> LastSelectedView =>
            _lastSelectedViewReadOnly
            ?? (_lastSelectedViewReadOnly = _lastSelectedView.ToReadOnlyReactiveProperty());

        public NavigationOrientation Orientation;
        public bool WrapAround;
        public bool KeepLastSelection = true;
        public int RowsOrColumns = 1;
        
        protected override void Start()
        {
            if (Orientation == NavigationOrientation.None)
                throw new InvalidOperationException(
                    $"SelectionControl is missing orientation value on gameObject {gameObject.ToHierarchyPath()}");

            Views
                .Select(x => x.GetAtOrDefault(SelectedIndex.Value ?? -1))
                .CombineLatest(IsSelfOrDescendantFocused.WhereTrue(), (selectedView, _) => selectedView)
                .Subscribe(selectedView =>
                {
                    if (selectedView != null)
                        selectedView.Focus();
                    else
                        this.Focus();
                })
                .AddTo(this);

            if (KeepLastSelection)
                SelectedView
                    .BindTo(_lastSelectedView)
                    .AddTo(this);

            SubscribeToSynchOther(SelectedView, () =>
                SelectedIndex.Value = IndexOfView(SelectedView.Value));

            SubscribeToSynchOther(SelectedIndex, () =>
                SelectedView.Value = GetViewAtIndex(SelectedIndex.Value));
        }

        protected override void RemoveAllViews(GameObject container, GameObject except = null)
        {
            base.RemoveAllViews(container, except);

            SelectedView.Value = null;
        }

        private void SubscribeToSynchOther<T>(IObservable<T> observable, Action synchAction)
        {
            observable
                .Subscribe(x =>
                {
                    if (_isSynching)
                        return;

                    _isSynching = true;
                    synchAction();
                    _isSynching = false;
                })
                .AddTo(this);
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
            SelectedView.Value = null;
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