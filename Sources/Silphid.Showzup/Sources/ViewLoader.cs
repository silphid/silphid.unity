using System;
using System.Collections.Generic;
using Silphid.Extensions;
using Silphid.Loadzup;
using UniRx;
using UnityEngine;
using CancellationToken = UniRx.CancellationToken;
using Object = UnityEngine.Object;

namespace Silphid.Showzup
{
    public class ViewLoader : IViewLoader
    {
        private readonly ILoader _loader;
        private readonly IInjectionAdapter _injectionAdaptor;
        private readonly ILogger _logger;

        public ViewLoader(ILoader loader, IInjectionAdapter injectionAdaptor, ILogger logger = null)
        {
            _loader = loader;
            _injectionAdaptor = injectionAdaptor;
            _logger = logger;
        }

        public IObservable<IView> Load(ViewInfo viewInfo, CancellationToken cancellationToken)
        {
            if (viewInfo.View != null)
                return Load(viewInfo.ViewModel, viewInfo.View);

            if (viewInfo.Model != null && viewInfo.ViewModelType != null && viewInfo.ViewType != null && viewInfo.PrefabUri != null)
                return Load(viewInfo.Model, viewInfo.ViewModelType, viewInfo.ViewType, viewInfo.PrefabUri, viewInfo.Parameters, cancellationToken);

            if (viewInfo.ViewType != null && viewInfo.PrefabUri != null)
                return Load(viewInfo.ViewModel, viewInfo.ViewType, viewInfo.PrefabUri, cancellationToken);

            return Observable.Return<IView>(null);
        }

        private IObservable<IView> Load(IViewModel viewModel, IView view)
        {
            return Observable.Return(view)
                .Do(x => InjectView(x, viewModel))
                .ContinueWith(x => LoadLoadable(x).ThenReturn(view));
        }

        private IObservable<IView> Load(object model, Type viewModelType, Type viewType, Uri uri, IEnumerable<object> parameters, CancellationToken cancellationToken) =>
            Load((IViewModel) _injectionAdaptor.Resolve(viewModelType, parameters.Prepend(model)), viewType, uri, cancellationToken);

        private IObservable<IView> Load(IViewModel viewModel, Type viewType, Uri uri, CancellationToken cancellationToken)
        {
            _logger?.Log(nameof(ViewLoader), $"Loading prefab {uri} with {viewType} for {viewModel?.GetType().Name}");
            return LoadPrefabView(viewType, uri, cancellationToken)
                .Do(view => InjectView(view, viewModel))
                .ContinueWith(view => LoadLoadable(view).ThenReturn(view));
        }

        private void InjectView(IView view, IViewModel viewModel)
        {
            view.ViewModel = viewModel;
            _logger?.Log(nameof(ViewLoader), $"Initializing {view} with ViewModel {viewModel}");
            _injectionAdaptor.Inject(view.GameObject);
        }

        private IObservable<Unit> LoadLoadable(IView view) =>
            (view as ILoadable)?.Load() ?? Observable.ReturnUnit();

        #region Prefab view loading

        private IObservable<IView> LoadPrefabView(Type viewType, Uri uri, CancellationToken cancellationToken)
        {
            _logger?.Log(nameof(ViewLoader), $"LoadPrefabView({viewType}, {uri})");

            return _loader.Load<GameObject>(uri)
                .Last()
                .Where(obj => CheckCancellation(cancellationToken))
                .Select(x => Instantiate(x, cancellationToken))
                .WhereNotNull()
                .DoOnError(ex => _logger?.LogError(nameof(ViewLoader),
                    $"Failed to load view {viewType} from {uri} with error:{Environment.NewLine}{ex}"))
                .Select(x => GetViewFromPrefab(x, viewType));
        }

        private bool CheckCancellation(CancellationToken cancellationToken, Action cancellationAction = null)
        {
            if (!cancellationToken.IsCancellationRequested)
                return true;

            cancellationAction?.Invoke();
            return false;
        }

        private GameObject Instantiate(GameObject original, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

#if DEBUG
            original = Object.Instantiate(original);
#endif

            DisableAllViews(original);

            var instance = Object.Instantiate(original);
#if DEBUG
            Object.Destroy(original);
#endif

            if (cancellationToken.IsCancellationRequested)
            {
                instance.Destroy();
                return null;
            }

            return instance;
        }

        private void DisableAllViews(GameObject obj)
        {
            obj.GetComponents<IView>().ForEach(x => x.IsActive = false);
        }

        private IView GetViewFromPrefab(GameObject gameObject, Type viewType)
        {
            var view = (IView) gameObject.GetComponent(viewType);

            if (view == null)
                throw new InvalidOperationException(
                    $"Loaded prefab {gameObject.name} has no view component of type {viewType.Name}");

            return view;
        }

        #endregion
    }
}