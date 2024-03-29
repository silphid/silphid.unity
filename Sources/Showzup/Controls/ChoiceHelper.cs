﻿using System;
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
    internal class ChoiceHelper
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly ReactiveProperty<IView> _chosenView = new ReactiveProperty<IView>();
        private ReactiveProperty<object> _chosenModel;

        public IReadOnlyReactiveProperty<IView> ChosenView => _chosenView;
        public ReactiveProperty<int?> ChosenIndex { get; } = new ReactiveProperty<int?>();

        private readonly ListControl _list;

        public ChoiceHelper(ListControl list)
        {
            _list = list;
        }

        public void Start()
        {
            if (_list.Orientation == NavigationOrientation.None)
                throw new InvalidOperationException(
                    $"ListControl is missing orientation value on gameObject {_list.gameObject.ToHierarchyPath()}");

            _list.Views.CombineLatest(
                      ChosenIndex,
                      (views, selectedIndex) => new
                      {
                          views,
                          selectedIndex
                      })
                 .Subscribe(
                      x =>
                      {
                          _chosenView.Value = x.views.Where(
                                                    v => !_list.ExcludeInactiveGameObjects ||
                                                         v.GameObject.activeInHierarchy)
                                               .GetAtOrDefault(x.selectedIndex);

                          if (_list.IsSelfOrDescendantSelected.Value)
                              _chosenView.Value?.Select();
                      })
                 .AddTo(_disposables);

            SubscribeToUpdateFocusables(ChosenView);

            _disposables.AddTo(_list);
            _chosenView.AddTo(_list);
            ChosenIndex.AddTo(_list);
        }

        private int? IndexOfView(IView view) => _list.IndexOfView(view);
        private bool HasItems => _list.HasItems;
        private ReadOnlyCollection<IView> Views => _list.Views.Value;
        private int? LastIndex => _list.LastIndex;
        private int? FirstIndex => _list.FirstIndex;
        private int RowsOrColumns => _list.RowsOrColumns;

        public IReactiveProperty<object> ChosenModel
        {
            get
            {
                if (_chosenModel != null)
                    return _chosenModel;

                bool isUpdating = false;

                _chosenModel = new ReactiveProperty<object>();
                _chosenModel.AddTo(_list);

                ChosenView.Where(_ => !isUpdating)
                          .Subscribe(
                               x =>
                               {
                                   isUpdating = true;
                                   _chosenModel.Value = x?.ViewModel.Model;
                                   isUpdating = false;
                               })
                          .AddTo(_disposables);

                _chosenModel.Where(_ => !isUpdating)
                            .Subscribe(
                                 x =>
                                 {
                                     isUpdating = true;
                                     ChooseModel(x);
                                     isUpdating = false;
                                 })
                            .AddTo(_disposables);

                return _chosenModel;
            }
        }

        private void SubscribeToUpdateFocusables<T>(IObservable<T> observable)
        {
            observable.PairWithPreviousOrDefault()
                      .Subscribe(
                           x =>
                           {
                               RemoveFocus(x.Item1 as IChooseable);
                               SetFocus(x.Item2 as IChooseable);
                           })
                      .AddTo(_disposables);
        }

        private void SetFocus(IChooseable chooseable)
        {
            if (chooseable == null)
                return;

            chooseable.IsChosen.Value = true;
        }

        private void RemoveFocus(IChooseable chooseable)
        {
            if (chooseable == null)
                return;

            chooseable.IsChosen.Value = false;
        }

        public void ChooseView(IView view)
        {
            ChosenIndex.Value = IndexOfView(view);
        }

        public void ChooseView<TView>(Func<TView, bool> predicate) where TView : IView
        {
            ChosenIndex.Value = IndexOfView(
                Views.OfType<TView>()
                     .FirstOrDefault(predicate));
        }

        public void ChooseViewModel<TViewModel>(TViewModel viewModel) where TViewModel : IViewModel
        {
            ChosenIndex.Value = IndexOfView(
                Views.Where(x => x.ViewModel is TViewModel)
                     .FirstOrDefault(x => ReferenceEquals(x.ViewModel, viewModel)));
        }

        public void ChooseViewModel<TViewModel>(Func<TViewModel, bool> predicate) where TViewModel : IViewModel
        {
            ChosenIndex.Value = IndexOfView(
                Views.Where(x => x.ViewModel is TViewModel)
                     .FirstOrDefault(x => predicate((TViewModel) x.ViewModel)));
        }

        public void ChooseModel<TModel>(TModel model)
        {
            ChosenIndex.Value = IndexOfView(
                Views.Where(x => x.ViewModel?.Model is TModel)
                     .FirstOrDefault(x => ReferenceEquals(x.ViewModel.Model, model)));
        }

        public void ChooseModel<TModel>(Func<TModel, bool> predicate)
        {
            ChosenIndex.Value = IndexOfView(
                Views.Where(x => x.ViewModel?.Model is TModel)
                     .FirstOrDefault(x => predicate((TModel) x.ViewModel.Model)));
        }

        public bool ChooseIndex(int index)
        {
            if (index >= Views.Count(v => !_list.ExcludeInactiveGameObjects || v.GameObject.activeInHierarchy))
                return false;

            if (!IsMovable(Views.ElementAt(index)))
                return false;

            ChosenIndex.Value = index;

            return true;
        }

        private bool IsMovable(IView view)
        {
            return !_list.ExcludeUnselectableGameObjects || (view as ISelectableContainer)?.SelectableContent != null ||
                   view is IChooseable;
        }

        public bool ChooseFirst()
        {
            if (!HasItems)
                return false;

            ChosenIndex.Value = FirstIndex;
            return true;
        }

        public bool ChooseLast()
        {
            if (!HasItems)
                return false;

            ChosenIndex.Value = LastIndex;
            return true;
        }

        public void ChooseNone()
        {
            ChosenIndex.Value = null;
        }

        public bool ChoosePrevious()
        {
            if (!HasItems)
                return false;

            if (ChosenIndex.Value == FirstIndex)
            {
                if (_list.WrapAround)
                {
                    ChosenIndex.Value = LastIndex;
                    return true;
                }

                return false;
            }

            ChosenIndex.Value--;
            return true;
        }

        public bool ChooseNext()
        {
            if (!HasItems)
                return false;

            if (ChosenIndex.Value == LastIndex)
            {
                if (_list.WrapAround)
                {
                    ChosenIndex.Value = 0;
                    return true;
                }

                return false;
            }

            ChosenIndex.Value++;
            return true;
        }

        public void OnMove(AxisEventData eventData)
        {
            if (!HasItems || ChosenIndex.Value == null)
                return;

            var moveDirection = _list.Orientation == NavigationOrientation.Vertical
                                    ? eventData.moveDir.FlipXY()
                                    : eventData.moveDir;

            if (MoveUp() || MoveDown() || MoveLeftWrapAround() || MoveRightWrapAround() || MoveLeft() || MoveRight())
                eventData.Use();

            bool MoveUp()
            {
                return moveDirection == MoveDirection.Up && ChosenIndex.Value % RowsOrColumns > 0 &&
                       ChooseIndex(ChosenIndex.Value.Value - 1);
            }

            bool MoveDown()
            {
                return moveDirection == MoveDirection.Down && ChosenIndex.Value % RowsOrColumns < RowsOrColumns - 1 &&
                       ChooseIndex(ChosenIndex.Value.Value + 1);
            }

            bool MoveLeftWrapAround()
            {
                return moveDirection == MoveDirection.Left && _list.WrapAround && ChosenIndex.Value == 0 &&
                       ChooseIndex(Views.Count - 1);
            }

            bool MoveRightWrapAround()
            {
                return moveDirection == MoveDirection.Right && _list.WrapAround &&
                       ChosenIndex.Value == Views.Count - 1 && ChooseIndex(0);
            }

            bool MoveLeft()
            {
                return moveDirection == MoveDirection.Left && ChosenIndex.Value >= RowsOrColumns &&
                       ChooseIndex(ChosenIndex.Value.Value - RowsOrColumns);
            }

            bool MoveRight()
            {
                return moveDirection == MoveDirection.Right && ChosenIndex.Value + RowsOrColumns < Views.Count &&
                       ChooseIndex(ChosenIndex.Value.Value + RowsOrColumns);
            }
        }

        public bool Handle(IRequest request)
        {
            if (!_list.HandlesChooseRequest)
                return false;

            var req = request as ChooseRequest;
            if (req == null)
                return false;

            var view = req.Input as IView;
            if (view != null)
            {
                ChooseView(view);
                return true;
            }

            var viewModel = req.Input as IViewModel;
            if (viewModel != null)
            {
                ChooseViewModel(viewModel);
                return true;
            }

            ChooseModel(req.Input);
            return true;
        }
    }
}