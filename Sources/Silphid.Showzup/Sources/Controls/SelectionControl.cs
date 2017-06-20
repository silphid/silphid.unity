using System;
using Silphid.Extensions;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    public class SelectionControl : ListControl, IMoveHandler
    {
        private bool _isSynching;
        private readonly SerialDisposable _focusDisposable = new SerialDisposable();

        public ReactiveProperty<object> SelectedItem { get; } = new ReactiveProperty<object>();
        public ReactiveProperty<IView> SelectedView { get; } = new ReactiveProperty<IView>();
        public ReactiveProperty<int?> SelectedIndex { get; } = new ReactiveProperty<int?>();

        public NavigationOrientation Orientation;
        public bool AutoFocus = true;
        public float FocusDelay;
        public bool WrapAround;

        public virtual void Start()
        {
            SubscribeToUpdateSelectable(SelectedItem);
            SubscribeToUpdateSelectable(SelectedView);

            SubscribeToSynchOthers(SelectedItem, () =>
            {
                SelectedView.Value = GetViewForViewModel(SelectedItem.Value);
                SelectedIndex.Value = IndexOfView(SelectedView.Value);
            });

            SubscribeToSynchOthers(SelectedView, () =>
            {
                SelectedItem.Value = SelectedView.Value?.ViewModel;
                SelectedIndex.Value = IndexOfView(SelectedView.Value);
            });

            SubscribeToSynchOthers(SelectedIndex, () =>
            {
                SelectedView.Value = GetViewAtIndex(SelectedIndex.Value);
                SelectedItem.Value = SelectedView.Value?.ViewModel;
            });
        }

        private void SubscribeToUpdateSelectable<T>(IObservable<T> observable)
        {
            observable
                .PairWithPrevious()
                .Subscribe(x =>
                {
                    RemoveFocus(x.Item1 as IFocusable);
                    SetFocus(x.Item2 as IFocusable);
                })
                .AddTo(this);
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

        private void SubscribeToSynchOthers<T>(IObservable<T> observable, Action synchAction)
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
            SelectedItem.Value = null;
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