using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Loadzup.Resource;
using Silphid.Showzup;
using Silphid.Showzup.Injection;
using UnityEngine;

namespace App
{
    public class Application : MonoBehaviour
    {
        public Manifest Manifest;
        public NavigationControl NavigationControl;
        
        public void Start()
        {
            var container = new Container(Debug.unityLogger);

            container.BindInstance(Debug.unityLogger);
            container.BindInstance(CreateLoader());
            container.BindSingle<IScoreEvaluator, ScoreEvaluator>();
            container.BindSingle<IViewResolver, ViewResolver>();
            container.BindSingle<IViewLoader, ViewLoader>();
            container.BindInstance<IManifest>(Manifest);
            container.BindInstance<IInjector>(new Injector(go => container.Inject(go)));
            container.BindInstance<IViewModelFactory>(CreateViewModelFactory(container));
            container.BindInstance<IVariantProvider>(VariantProvider.From<Display, Form, Platform>());

            container.InjectAllGameObjects();
            
            NavigationControl.Present(new Catalog())
                .SubscribeAndForget(_ => {}, ex => Debug.Log($"Failed to navigate: {ex}"), () => {});
        }

        private static ViewModelFactory CreateViewModelFactory(Container container) =>
            new ViewModelFactory((viewModelType, parameters) =>
                (IViewModel) container.Resolve(viewModelType, new Container().BindInstances(parameters)));

        private ILoader CreateLoader() =>
            new ResourceLoader(new SpriteConverter());
    }
}