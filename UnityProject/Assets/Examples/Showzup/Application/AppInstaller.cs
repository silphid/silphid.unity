using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Loadzup.Resource;
using Silphid.Showzup;
using Silphid.Injexit;
using Silphid.Loadzup.Http;
using UniRx;
using UnityEngine;

namespace App
{
    public class AppInstaller : RootInstaller
    {    
        public Manifest Manifest;
        public NavigationControl NavigationControl;

        protected override void OnBind()
        {
            Container.BindInstance(CreateLoader());

            Container.Bind<IConverter, CompositeConverter>().AsSingle().Using(x =>
            {
                x.Bind<IConverter, SpriteConverter>().AsList();
                x.Bind<IConverter, TextureConverter>().AsList();
            });
            Container.Bind<ILoader, CompositeLoader>().AsSingle().Using(x =>
            {
                x.Bind<ILoader, HttpLoader>().AsList();
                x.Bind<ILoader, ResourceLoader>().AsList();
            });
            Container.Bind<IScoreEvaluator, ScoreEvaluator>().AsSingle();
            Container.Bind<IViewResolver, ViewResolver>().AsSingle();
            Container.Bind<IViewLoader, ViewLoader>().AsSingle();
            Container.BindInstance<IManifest>(Manifest);
            Container.BindInstance<IInjectionAdapter>(new InjectionAdapter(Container));
            Container.BindInstance(VariantProvider.From<Display, Form, Platform>());
            Container.BindToSelfAll<IViewModel>(GetType().Assembly);
        }

        protected override void OnReady()
        {
            NavigationControl
                .Present(new Catalog())
                .AutoDetach()
                .Subscribe(_ => {}, ex => Debug.Log($"Failed to navigate: {ex}"), () => {});
        }

        private ILoader CreateLoader() =>
            new ResourceLoader(new SpriteConverter());
    }
}