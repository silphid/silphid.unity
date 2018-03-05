using System;
using System.Collections.ObjectModel;
using System.Linq;
using Silphid.Extensions;
using Silphid.Requests;
using Silphid.Showzup.Navigation;
using Silphid.Showzup.Requests;
using UniRx;
using UnityEngine.EventSystems;

namespace Silphid.Showzup
{
    internal class SelectionHelper : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly ReactiveProperty<IView> _selectedView = new ReactiveProperty<IView>();
        private ReactiveProperty<object> _selectedModel;

        public IReadOnlyReactiveProperty<IView> SelectedView => _selectedView;
        public ReactiveProperty<int?> SelectedIndex { get; } = new ReactiveProperty<int?>();

        private readonly ListControl _list;

        public SelectionHelper(ListControl list)
        {
            if (list.Orientation == NavigationOrientation.None)
                throw new InvalidOperationException(
                    $"SelectionControl is missing orientation value on gameObject {_list.gameObject.ToHierarchyPath()}");

            _list = list;

            _list.Views
                .CombineLatest(SelectedIndex, (views, selectedIndex) => new {views, selectedIndex})
                .Subscribe(x =>
                {
                    _selectedView.Value = x.views.GetAtOrDefault(x.selectedIndex);

                    if (_list.IsSelfOrDescendantSelected.Value)
                        _selectedView.Value?.Select();
                })
                .AddTo(_disposables);

            SubscribeToUpdateFocusables(SelectedView);
        }

        private int? IndexOfView(IView view) => _list.IndexOfView(view);
        private bool HasItems => _list.HasItems;
        private ReadOnlyCollection<IView> Views => _list.Views.Value;
        private int? LastIndex => _list.LastIndex;
        private int? FirstIndex => _list.FirstIndex;
        private int RowsOrColumns => _list.RowsOrColumns;

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
                    .AddTo(_disposables);

                _selectedModel
                    .Where(_ => !isUpdating)
                    .Subscribe(x =>
                    {
                        isUpdating = true;
                        SelectModel(x);
                        isUpdating = false;
                    })
                    .AddTo(_disposables);

                return _selectedModel;
            }
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
                .AddTo(_disposables);
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
            SelectedIndex.Value = IndexOfView(Views
                .OfType<TView>()
                .FirstOrDefault(predicate));
        }

        public void SelectViewModel<TViewModel>(TViewModel viewModel) where TViewModel : IViewModel
        {
            SelectedIndex.Value = IndexOfView(Views
                .Where(x => x.ViewModel is TViewModel)
                .FirstOrDefault(x => ReferenceEquals(x.ViewModel, viewModel)));
        }

        public void SelectViewModel<TViewModel>(Func<TViewModel, bool> predicate) where TViewModel : IViewModel
        {
            SelectedIndex.Value = IndexOfView(Views
                .Where(x => x.ViewModel is TViewModel)
                .FirstOrDefault(x => predicate((TViewModel) x.ViewModel)));
        }

        public void SelectModel<TModel>(TModel model)
        {
            SelectedIndex.Value = IndexOfView(Views
                .Where(x => x.ViewModel?.Model is TModel)
                .FirstOrDefault(x => ReferenceEquals(x.ViewModel.Model, model)));
        }

        public void SelectModel<TModel>(Func<TModel, bool> predicate)
        {
            SelectedIndex.Value = IndexOfView(Views
                .Where(x => x.ViewModel?.Model is TModel)
                .FirstOrDefault(x => predicate((TModel) x.ViewModel.Model)));
        }

        public bool SelectIndex(int index)
        {
            if (index >= Views.Count)
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
                if (_list.WrapAround)
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
                if (_list.WrapAround)
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

            var moveDirection = _list.Orientation == NavigationOrientation.Vertical
                ? eventData.moveDir.FlipXY()
                : eventData.moveDir;

            if (moveDirection == MoveDirection.Up && SelectedIndex.Value % RowsOrColumns > 0 && SelectIndex(SelectedIndex.Value.Value - 1) ||
                moveDirection == MoveDirection.Down && SelectedIndex.Value % RowsOrColumns < RowsOrColumns - 1 && SelectIndex(SelectedIndex.Value.Value + 1) ||
                moveDirection == MoveDirection.Left && _list.WrapAround && SelectedIndex.Value == 0 && SelectIndex(Views.Count - 1) ||
                moveDirection == MoveDirection.Right && _list.WrapAround && SelectedIndex.Value == Views.Count - 1 && SelectIndex(0) ||
                moveDirection == MoveDirection.Left && SelectedIndex.Value >= RowsOrColumns && SelectIndex(SelectedIndex.Value.Value - RowsOrColumns) ||
                moveDirection == MoveDirection.Right && SelectedIndex.Value + RowsOrColumns < Views.Count && SelectIndex(SelectedIndex.Value.Value + RowsOrColumns))
                eventData.Use();
        }

        public bool Handle(IRequest request)
        {
            if (!_list.HandlesSelectRequest)
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

        public void Dispose()
        {
            _disposables?.Dispose();
            _selectedView?.Dispose();
            _selectedModel?.Dispose();
            SelectedIndex?.Dispose();
        }
    }
}