using System;
using System.Linq;
using System.Threading;
using log4net;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Loadzup;
using Silphid.Showzup.Recipes;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Showzup
{
    public class ViewLoader : IViewLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ViewLoader));

        private readonly ILoader _loader;

        public ViewLoader(ILoader loader)
        {
            _loader = loader;
        }

        public IObservable<IView> Load(Transform parent,
                                       Recipe recipe,
                                       IContainer container,
                                       CancellationToken cancellationToken)
        {
            container = GetContainer(container, recipe);

            if (recipe.View != null)
                return Load(parent, recipe.ViewModel, recipe.View, container);

            if (recipe.Model != null && recipe.ViewModelType != null && recipe.ViewType != null &&
                recipe.PrefabUri != null)
                return LoadFromModel(
                    parent,
                    recipe.Model,
                    recipe.ModelType,
                    recipe.ViewModelType,
                    recipe.ViewType,
                    recipe.PrefabUri,
                    container,
                    cancellationToken);

            if (recipe.ViewModel != null && recipe.ViewType != null && recipe.PrefabUri != null)
                return LoadFromViewModel(
                    parent,
                    recipe.ViewModel,
                    recipe.ViewType,
                    recipe.PrefabUri,
                    container,
                    cancellationToken);

            if (recipe.ViewModelType != null && recipe.ViewType != null && recipe.PrefabUri != null)
                return LoadFromViewModelType(
                    parent,
                    recipe.ViewModelType,
                    recipe.ViewType,
                    recipe.PrefabUri,
                    container,
                    cancellationToken);

            return Observable.Return<IView>(null);
        }

        private static IContainer GetContainer(IContainer container, Recipe recipe)
        {
            var viewOptions = recipe.Options.GetViewOptions();
            if (recipe.Parameters == null && viewOptions == null)
                return container;

            return container.Using(
                x =>
                {
                    if (recipe.Parameters != null)
                        x.BindInstances(recipe.Parameters);

                    if (viewOptions != null)
                        x.BindInstance(viewOptions);
                });
        }

        private IObservable<IView> Load(Transform parent, IViewModel viewModel, IView view, IContainer container)
        {
            return Observable.Return(view)
                             .Do(x => InjectView(x, viewModel, container))
                             .ContinueWith(
                                  x => LoadLoadable(x)
                                     .ThenReturn(view));
        }

        private IObservable<IView> LoadFromModel(Transform parent,
                                                 object model,
                                                 Type modelType,
                                                 Type viewModelType,
                                                 Type viewType,
                                                 Uri uri,
                                                 IContainer container,
                                                 CancellationToken cancellationToken)
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug(
                        $"Resolving {viewModelType.Name} (with Model {model.GetType().Name}) for View {viewType.Name}");

                var viewModel = (IViewModel) container.UsingInstance(modelType, model)
                                                      .Resolve(viewModelType);

                return LoadFromViewModel(parent, viewModel, viewType, uri, container, cancellationToken);
            }
            catch (DependencyException ex)
            {
                Log.Warn(ex);
                throw new LoadException(
                    $"Failed to resolve {viewModelType.Name} (with Model {modelType.Name}) for View {viewType.Name}",
                    ex);
            }
        }

        private IObservable<IView> LoadFromViewModelType(Transform parent,
                                                         Type viewModelType,
                                                         Type viewType,
                                                         Uri uri,
                                                         IContainer container,
                                                         CancellationToken cancellationToken)
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Resolving {viewModelType.Name} (without Model) for View {viewType.Name}");

                var viewModel = (IViewModel) container.Resolve(viewModelType);
                return LoadFromViewModel(parent, viewModel, viewType, uri, container, cancellationToken);
            }
            catch (DependencyException ex)
            {
                throw new LoadException(
                    $"Failed to resolve {viewModelType.Name} (without Model) for View {viewType.Name}",
                    ex);
            }
        }

        private IObservable<IView> LoadFromViewModel(Transform parent,
                                                     IViewModel viewModel,
                                                     Type viewType,
                                                     Uri uri,
                                                     IContainer container,
                                                     CancellationToken cancellationToken)
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Loading prefab {uri} with {viewType} for {viewModel?.GetType().Name}");

                return LoadPrefabView(parent, viewType, uri, cancellationToken)
                      .Do(view => InjectView(view, viewModel, container))
                      .ContinueWith(
                           view => LoadLoadable(view)
                              .ThenReturn(view));
            }
            catch (Exception ex)
            {
                return Observable.Throw<IView>(
                    new LoadException(
                        $"Failed to loading prefab {uri} with {viewType} for {viewModel?.GetType().Name}",
                        ex));
            }
        }

        private void InjectView(IView view, IViewModel viewModel, IContainer container)
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.Debug($"Initializing {view} with {viewModel}");

                view.ViewModel = viewModel;
                
                // Todo review. If Page itself needs to get injected with PageService, we need its scoped container to inject itself
                var viewContainer = container.Using(((IContainerProvider) view).Container);
                viewContainer.Inject(view.GameObject);
            }
            catch (DependencyException ex)
            {
                throw new LoadException($"Failed injecting {view}", ex);
            }
        }

        private ICompletable LoadLoadable(IView view)
        {
            try
            {
                return (view as ILoadable)?.Load()
                                          ?.Catch<Exception>(
                                                ex => Completable.Throw(
                                                    new LoadException(
                                                        $"Error in view.Load() of {view.GetType().Name}",
                                                        ex))) ?? Completable.Empty();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return Completable.Throw(new LoadException($"Error in view.Load() of {view.GetType().Name}", ex));
            }
        }

        #region Prefab view loading

        private IObservable<IView> LoadPrefabView(Transform parent,
                                                  Type viewType,
                                                  Uri uri,
                                                  CancellationToken cancellationToken)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"LoadPrefabView({viewType}, {uri})");

            return _loader.Load<GameObject>(uri)
                          .Last()
                          .Where(obj => CheckCancellation(cancellationToken))
                          .Select(x => Instantiate(parent, x, cancellationToken))
                          .WhereNotNull()
                          .DoOnError(
                               ex => Log.Error(
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
            view.GameObject.GetComponents<IView>()
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