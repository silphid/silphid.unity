using System;
using System.Linq;
using Silphid.Extensions;
using Silphid.Requests;
using Silphid.Showzup.Navigation;
using Silphid.Showzup.Requests;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    public class SelectionControl : ListControl, IMoveHandler, IRequestHandler
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

        public bool HandlesSelectRequest;

        protected override void Start()
        {
            if (Orientation == NavigationOrientation.None)
                throw new InvalidOperationException(
                    $"SelectionControl is missing orientation value on gameObject {gameObject.ToHierarchyPath()}");

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
                .PairWithPreviousOrDefault()
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

        public void SelectView<TView>(Func<TView, bool> predicate) where TView : IView
        {
            SelectedView.Value = _views
                .OfType<TView>()
                .FirstOrDefault(predicate);
        }

        public void SelectViewModel<TViewModel>(TViewModel viewModel) where TViewModel : IViewModel
        {
            SelectedView.Value = _views
                .Where(x => x.ViewModel is TViewModel)
                .FirstOrDefault(x => ReferenceEquals(x.ViewModel, viewModel));
        }

        public void SelectViewModel<TViewModel>(Func<TViewModel, bool> predicate) where TViewModel : IViewModel
        {
            SelectedView.Value = _views
                .Where(x => x.ViewModel is TViewModel)
                .FirstOrDefault(x => predicate((TViewModel) x.ViewModel));
        }

        public void SelectModel<TModel>(TModel model)
        {
            SelectedView.Value = _views
                .Where(x => x.ViewModel?.Model is TModel)
                .FirstOrDefault(x => ReferenceEquals(x.ViewModel.Model, model));
        }

        public void SelectModel<TModel>(Func<TModel, bool> predicate)
        {
            SelectedView.Value = _views
                .Where(x => x.ViewModel?.Model is TModel)
                .FirstOrDefault(x => predicate((TModel) x.ViewModel.Model));
        }

        public bool SelectIndex(int index)
        {
            var viewAtIndex = GetViewAtIndex(index);

            if (viewAtIndex == null)
                return false;

            SelectedView.Value = viewAtIndex;

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

        public bool Handle(IRequest request)
        {
            if (!HandlesSelectRequest)
                return false;

            var req = request as SelectRequest;

            if (req == null)
                return false;

            var view = req.Input as IView;

            if (view != null)
            {
                SelectView(view);
                return true;
            }

            var viewModel = req.Input as IViewModel;

            if (viewModel != null)
            {
                SelectViewModel(viewModel);
                return true;
            }

            SelectModel(req.Input);
            return true;
        }
    }
}