using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using Silphid.Requests;
using Silphid.Showzup.Navigation;
using Silphid.Showzup.Requests;
using UniRx;

namespace Silphid.Showzup.Virtual
{
    internal class ChoiceHelper
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly ReactiveProperty<IView> _chosenView = new ReactiveProperty<IView>();
        private ReactiveProperty<object> _chosenModel;
        private List<Entry> _entries;

        public IReadOnlyReactiveProperty<IView> ChosenView => _chosenView;
        private ReactiveProperty<int?> _chosenIndex { get; } = new ReactiveProperty<int?>();

        private readonly ListControl _list;

        public ChoiceHelper(ListControl list)
        {
            _list = list;
        }

        public void ChooseView(IView view)
        {
            Select(_entries.First(x => x.View == view));
        }

        public void ChooseViewModel<TViewModel>(TViewModel viewModel) where TViewModel : IViewModel
        {
            Select(_entries.First(x => x.View.ViewModel == (IViewModel) viewModel));
        }

        public void ChooseModel<TModel>(TModel model)
        {
            Select(_entries.First(x => x.Model == (object) model));
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

        private void RemoveAllFocus()
        {
            _entries.Select(x => x.View as IChooseable)
                    .WhereNotNull()
                    .ForEach(x => x.IsChosen.Value = false);
        }

        private void RemoveAllFocus(IChooseable chooseable)
        {
            if (chooseable == null)
                return;

            chooseable.IsChosen.Value = false;
        }

        private void Select(Entry entry)
        {
            var view = entry.View;

            RemoveFocus(_chosenView.Value as IChooseable);
            _chosenView.Value = view;
            _chosenIndex.Value = _entries.IndexOf(entry);

            // if (_list.IsSelfOrDescendantSelected.Value)
            view?.Select();
            SetFocus(view as IChooseable);
        }

        public bool Handle(IRequest request, List<Entry> entries, bool handlesChooseRequest, bool consumeRequest)
        {
            _entries = entries; // Find a better solution, copied from old listControl

            if (!handlesChooseRequest)
                return false;

            if (!(request is ChooseRequest req))
                return false;

            // if (_chosenView == null)
            RemoveAllFocus();

            if (req.Input is IView view)
            {
                ChooseView(view);
                return consumeRequest;
            }

            if (req.Input is IViewModel viewModel)
            {
                ChooseViewModel(viewModel);
                return consumeRequest;
            }

            ChooseModel(req.Input);
            return consumeRequest;
        }
    }
}