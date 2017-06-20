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

            container.BindInstance<ILoader>(CreateLoader());
            container.BindSingle<IScoreEvaluator, ScoreEvaluator>();
            container.BindSingle<IViewResolver, ViewResolver>();
            container.BindSingle<IViewLoader, ViewLoader>();
            container.BindInstance<IManifest>(Manifest);
            container.BindInstance<IInjector>(new Injector(go => container.Inject(go)));
            container.BindInstance<IViewModelFactory>(CreateViewModelFactory(container));
            container.BindInstance<IVariantProvider>(new VariantProvider(Display.Group, Form.Group, Platform.Group));

            container.InjectAllGameObjects();
            
            NavigationControl.Present(new Catalog())
                .SubscribeAndForget(_ => {}, ex => Debug.Log($"Failed to navigate: {ex}"), () => {});
        }

        private static ViewModelFactory CreateViewModelFactory(Container container)
        {
            return new ViewModelFactory((viewModelType, parameters) =>
                (IViewModel) container.Resolve(viewModelType, new Container().BindInstances(parameters)));
        }

        private CompositeLoader CreateLoader()
        {
            var compositeConverter = new CompositeConverter(
                new SpriteConverter());

            return new CompositeLoader(
                new ResourceLoader(compositeConverter));
        }
    }
}