using System;
using System.Linq;
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
        private readonly SerialDisposable _focusDisposable = new SerialDisposable();
        private readonly ReactiveProperty<IView> _lastSelectedView = new ReactiveProperty<IView>();
        private ReadOnlyReactiveProperty<IView> _lastSelectedViewReadOnly;

        public ReactiveProperty<IView> SelectedView { get; } = new ReactiveProperty<IView>();
        public ReactiveProperty<int?> SelectedIndex { get; } = new ReactiveProperty<int?>();

        public ReadOnlyReactiveProperty<IView> LastSelectedView =>
            _lastSelectedViewReadOnly
            ?? (_lastSelectedViewReadOnly = _lastSelectedView.ToReadOnlyReactiveProperty());

        public NavigationOrientation Orientation;
        public bool AutoFocus = true;
        public float FocusDelay;
        public bool WrapAround;

        protected override void Start()
        {
            if (Orientation == NavigationOrientation.None)
                throw new InvalidOperationException($"SelectionControl is missing orientation value on gameObject {gameObject.ToHierarchyPath()}");

            if (AutoSelect)
                Views
                    .CombineLatest(IsSelected.WhereTrue(), (x, y) => x)
                    .Subscribe(x => SelectView(_lastSelectedView.Value ?? x.FirstOrDefault()))
                    .AddTo(this);
            
            SubscribeToUpdateFocusables(SelectedView);

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

        private void SubscribeToUpdateFocusables<T>(IObservable<T> observable)
        {
            observable
                .PairWithPrevious()
                .Subscribe(x =>
                {
                    RemoveFocus(x.Item1 as IFocusable);
                    SetFocus(x.Item2 as IFocusable);
                    AutoSelectView(x.Item2 as IView);                        
                })
                .AddTo(this);
        }

        private void AutoSelectView(IView view)
        {
            if (AutoSelect && view != null)
                base.SelectView(view);
        }

        private void SetFocus(IFocusable focusable)
        {
            if (!AutoFocus || focusable == null)
                return;

            if (FocusDelay.IsAlmostZero())
            {
                focusable.IsFocused.Value = true;
                return;
            }

            _focusDisposable.Disposable = Observable
                .Timer(TimeSpan.FromSeconds(FocusDelay))
                .Subscribe(_ => focusable.IsFocused.Value = true);
        }

        private void RemoveFocus(IFocusable focusable)
        {
            if (!AutoFocus || focusable == null)
                return;

            focusable.IsFocused.Value = false;
        }

        private void SubscribeToSynchOther<T>(IObservable<T> observable, Action synchAction)
        {
            observable.Subscribe(x =>
                {
                    if (_isSynching)
                        return;

                    _isSynching = true;
                    synchAction();
                    _isSynching = false;
                })
                .AddTo(this);
        }

        protected override void SelectView(IView view)
        {
            if (SelectedView.Value == view)
                SelectedView.Value = null;
            
            SelectedView.Value = view;
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
            if (Orientation == NavigationOrientation.Horizontal)
            {
                if (eventData.moveDir == MoveDirection.Left && SelectPrevious() ||
                    eventData.moveDir == MoveDirection.Right && SelectNext())
                    eventData.Use();
            }
            else if (Orientation == NavigationOrientation.Vertical)
            {
                if (eventData.moveDir == MoveDirection.Up && SelectPrevious() ||
                    eventData.moveDir == MoveDirection.Down && SelectNext())
                    eventData.Use();
            }
        }
    }
}