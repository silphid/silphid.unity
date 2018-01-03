using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Loadzup;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Showzup
{
    public class ViewLoader : IViewLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ViewLoader));
        
        private readonly ILoader _loader;
        private readonly IInjectionAdapter _injectionAdaptor;

        public ViewLoader(ILoader loader, IInjectionAdapter injectionAdaptor)
        {
            _loader = loader;
            _injectionAdaptor = injectionAdaptor;
        }

        public IObservable<IView> Load(Transform parent, ViewInfo viewInfo, CancellationToken cancellationToken)
        {
            if (viewInfo.View != null)
                return Load(parent, viewInfo.ViewModel, viewInfo.View, viewInfo.Parameters);

            if (viewInfo.Model != null && viewInfo.ViewModelType != null && viewInfo.ViewType != null && viewInfo.PrefabUri != null)
                return LoadFromModel(parent, viewInfo.Model, viewInfo.ViewModelType, viewInfo.ViewType, viewInfo.PrefabUri, viewInfo.Parameters, cancellationToken);

            if (viewInfo.ViewModel != null && viewInfo.ViewType != null && viewInfo.PrefabUri != null)
                return LoadFromViewModel(parent, viewInfo.ViewModel, viewInfo.ViewType, viewInfo.PrefabUri, viewInfo.Parameters, cancellationToken);

            if (viewInfo.ViewModelType != null && viewInfo.ViewType != null && viewInfo.PrefabUri != null)
                return LoadFromViewModelType(parent, viewInfo.ViewModelType, viewInfo.ViewType, viewInfo.PrefabUri, viewInfo.Parameters, cancellationToken);

            return Observable.Return<IView>(null);
        }

        private IObservable<IView> Load(Transform parent, IViewModel viewModel, IView view, IDictionary<Type, object> parameters)
        {
            return Observable.Return(view)
                .Do(x => InjectView(x, viewModel, parameters))
                .ContinueWith(x => LoadLoadable(x).ThenReturn(view));
        }

        private IObservable<IView> LoadFromModel(Transform parent, object model, Type viewModelType, Type viewType, Uri uri, IDictionary<Type, object> parameters, CancellationToken cancellationToken)
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Resolving {viewModelType.Name} (with Model {model.GetType().Name}) for View {viewType.Name}");

                // Clone or create dictionary with extra parameter
                parameters = parameters != null
                    ? new Dictionary<Type, object>(parameters)
                    : new Dictionary<Type, object>();
                parameters[model.GetType()] = model;

                var viewModel = (IViewModel) _injectionAdaptor.Resolve(viewModelType, parameters);
                return LoadFromViewModel(parent, viewModel, viewType, uri, parameters, cancellationToken);

            }
            catch (DependencyException ex)
            {
                throw new LoadException($"Failed to resolve {viewModelType.Name} (with Model {model.GetType().Name}) for View {viewType.Name}", ex);
            }
        }

        private IObservable<IView> LoadFromViewModelType(Transform parent, Type viewModelType, Type viewType, Uri uri, IDictionary<Type, object> parameters, CancellationToken cancellationToken)
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Resolving {viewModelType.Name} (without Model) for View {viewType.Name}");
                
                var viewModel = (IViewModel) _injectionAdaptor.Resolve(viewModelType, parameters);
                return LoadFromViewModel(parent, viewModel, viewType, uri, parameters, cancellationToken);

            }
            catch (DependencyException ex)
            {
                throw new LoadException($"Failed to resolve {viewModelType.Name} (without Model) for View {viewType.Name}", ex);
            }
        }

        private IObservable<IView> LoadFromViewModel(Transform parent, IViewModel viewModel, Type viewType, Uri uri, IDictionary<Type, object> parameters, CancellationToken cancellationToken)
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Loading prefab {uri} with {viewType} for {viewModel?.GetType().Name}");
                
                return LoadPrefabView(parent, viewType, uri, parameters, cancellationToken)
                    .Do(view => InjectView(view, viewModel, parameters))
                    .ContinueWith(view => LoadLoadable(view).ThenReturn(view));
            }
            catch (Exception ex)
            {
                return Observable.Throw<IView>(new LoadException($"Failed to loading prefab {uri} with {viewType} for {viewModel?.GetType().Name}", ex));
            }
        }

        private void InjectView(IView view, IViewModel viewModel, IDictionary<Type, object> parameters)
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Initializing {view} with {viewModel}");
                
                view.ViewModel = viewModel;
                _injectionAdaptor.Inject(view.GameObject, parameters);

            }
            catch (DependencyException ex)
            {
                throw new LoadException($"Failed injecting {view} with parameters: {parameters.JoinAsString(", ")}", ex);
            }
        }

        private IObservable<Unit> LoadLoadable(IView view)
        {
            try
            {
                return (view as ILoadable)?.Load()?
                       .Catch<Unit, Exception>(ex =>
                           Observable.Throw<Unit>(new LoadException($"Error in view.Load() of {view.GetType().Name}", ex)))
                       ?? Observable.ReturnUnit();
            }
            catch (Exception ex)
            {
                return Observable.Throw<Unit>(new LoadException($"Error in view.Load() of {view.GetType().Name}", ex));
            }
        }

        #region Prefab view loading

        private IObservable<IView> LoadPrefabView(Transform parent, Type viewType, Uri uri, IDictionary<Type, object> parameters, CancellationToken cancellationToken)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"LoadPrefabView({viewType}, {uri})");

            return _loader.Load<GameObject>(uri)
                .Last()
                .Where(obj => CheckCancellation(cancellationToken))
                .Select(x => Instantiate(parent, x, cancellationToken))
                .WhereNotNull()
                .DoOnError(ex => Log.Error(
                    $"Failed to load {viewType} from {uri} with error:{Environment.NewLine}{ex}"))
                .Select(x => GetViewFromPrefab(x, viewType))
                .Do(DisableOtherViews);
        }

        private bool CheckCancellation(CancellationToken cancellationToken, Action cancellationAction = null)
        {
            if (!cancellationToken.IsCancellationRequested)
                return true;

            cancellationAction?.Invoke();
            return false;
        }

        private GameObject Instantiate(Transform parent, GameObject original, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var instance = Object.Instantiate(original, parent);

            if (cancellationToken.IsCancellationRequested)
            {
                instance.Destroy();
                return null;
            }

            return instance;
        }

        private void DisableOtherViews(IView view)
        {
            view.GameObject
                .GetComponents<IView>()
                .Except(view)
                .OfType<MonoBehaviour>()
                .ForEach(x => x.enabled = false);
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