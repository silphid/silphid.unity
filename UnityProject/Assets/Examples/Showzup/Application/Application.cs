using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Loadzup.Resource;
using Silphid.Showzup;
using Silphid.Injexit;
using UniRx;
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
            container.Bind<IScoreEvaluator, ScoreEvaluator>().AsSingle();
            container.Bind<IViewResolver, ViewResolver>().AsSingle();
            container.Bind<IViewLoader, ViewLoader>().AsSingle();
            container.BindInstance<IManifest>(Manifest);
            container.BindInstance<IInjectionAdapter>(new InjectionAdapter(container));
            container.BindInstance(VariantProvider.From<Display, Form, Platform>());

            container.InjectScene(gameObject.scene);
            
            NavigationControl
                .Present(new Catalog())
                .AutoDetach()
                .Subscribe(_ => {}, ex => Debug.Log($"Failed to navigate: {ex}"), () => {});
        }

        private ILoader CreateLoader() =>
            new ResourceLoader(new SpriteConverter());
    }
}