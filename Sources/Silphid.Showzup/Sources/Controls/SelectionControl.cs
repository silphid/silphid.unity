using System;
using System.Linq;
using JetBrains.Annotations;
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
        private readonly ReactiveProperty<IView> _selectedView = new ReactiveProperty<IView>();
        private ReactiveProperty<object> _selectedModel;

        public IReadOnlyReactiveProperty<IView> SelectedView => _selectedView;
        public ReactiveProperty<int?> SelectedIndex { get; } = new ReactiveProperty<int?>();

        public bool WrapAround;
        public int RowsOrColumns = 1;
        public bool AutoSelectFirst = true;
        public bool HandlesSelectRequest;
        
        public IReactiveProperty<object> SelectedModel
        {
            get
            {
                if (_selectedModel != null)
                    return _selectedModel;
                
                bool isUpdating = false;
            
                _selectedModel = new ReactiveProperty<object>();

                SelectedView
                    .Where(_ => !isUpdating)
                    .Subscribe(x =>
                    {
                        isUpdating = true;
                        _selectedModel.Value = x?.ViewModel.Model;
                        isUpdating = false;
                    })
                    .AddTo(this);

                _selectedModel
                    .Where(_ => !isUpdating)
                    .Subscribe(x =>
                    {
                        isUpdating = true;
                        SelectModel(x);
                        isUpdating = false;
                    })
                    .AddTo(this);

                return _selectedModel;
            }
        }

        public override GameObject ForwardSelection() => _selectedView.Value?.GameObject;

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

        [Pure]
        protected override IObservable<IView> PresentView(object input, Options options = null) =>
            base.PresentView(input, options)
                .DoOnCompleted(() =>
                {
                    if (AutoSelectFirst)
                        SelectFirst();
                });

        protected override void RemoveAllViews(GameObject container, GameObject except = null)
        {
            base.RemoveAllViews(container, except);

            SelectedIndex.Value = null;
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

        public void SelectView(IView view)
        {
            SelectedIndex.Value = IndexOfView(view);
        }

        public void SelectView<TView>(Func<TView, bool> predicate) where TView : IView
        {
            SelectedIndex.Value = IndexOfView(_views
                .OfType<TView>()
                .FirstOrDefault(predicate));
        }

        public void SelectViewModel<TViewModel>(TViewModel viewModel) where TViewModel : IViewModel
        {
            SelectedIndex.Value = IndexOfView(_views
                .Where(x => x.ViewModel is TViewModel)
                .FirstOrDefault(x => ReferenceEquals(x.ViewModel, viewModel)));
        }

        public void SelectViewModel<TViewModel>(Func<TViewModel, bool> predicate) where TViewModel : IViewModel
        {
            SelectedIndex.Value = IndexOfView(_views
                .Where(x => x.ViewModel is TViewModel)
                .FirstOrDefault(x => predicate((TViewModel) x.ViewModel)));
        }

        public void SelectModel<TModel>(TModel model)
        {
            SelectedIndex.Value = IndexOfView(_views
                .Where(x => x.ViewModel?.Model is TModel)
                .FirstOrDefault(x => ReferenceEquals(x.ViewModel.Model, model)));
        }

        public void SelectModel<TModel>(Func<TModel, bool> predicate)
        {
            SelectedIndex.Value = IndexOfView(_views
                .Where(x => x.ViewModel?.Model is TModel)
                .FirstOrDefault(x => predicate((TModel) x.ViewModel.Model)));
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
                moveDirection == MoveDirection.Left && WrapAround && SelectedIndex.Value == 0 &&
                SelectIndex(Views.Value.Count - 1) ||
                moveDirection == MoveDirection.Right && WrapAround && SelectedIndex.Value == Views.Value.Count - 1 &&
                SelectIndex(0) ||
                moveDirection == MoveDirection.Left && SelectedIndex.Value >= RowsOrColumns &&
                SelectIndex(SelectedIndex.Value.Value - RowsOrColumns) ||
                moveDirection == MoveDirection.Right && SelectedIndex.Value + RowsOrColumns < Views.Value.Count &&
                SelectIndex(SelectedIndex.Value.Value + RowsOrColumns))
                eventData.Use();
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