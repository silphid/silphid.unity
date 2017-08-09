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

        protected override void OnBind(IBinder binder)
        {
            binder.BindInstance(CreateLoader());

            binder.Bind<IConverter, CompositeConverter>().AsSingle().Using(x =>
            {
                x.Bind<IConverter, SpriteConverter>().AsList();
                x.Bind<IConverter, TextureConverter>().AsList();
            });
            binder.Bind<ILoader, CompositeLoader>().AsSingle().Using(x =>
            {
                x.Bind<ILoader, HttpLoader>().AsList();
                x.Bind<ILoader, ResourceLoader>().AsList();
            });
            binder.Bind<IScoreEvaluator, ScoreEvaluator>().AsSingle();
            binder.Bind<IViewResolver, ViewResolver>().AsSingle();
            binder.Bind<IViewLoader, ViewLoader>().AsSingle();
            binder.BindInstance<IManifest>(Manifest);
            binder.BindInstance<IInjectionAdapter>(new InjectionAdapter(Container));
            binder.BindInstance(VariantProvider.From<Display, Form, Platform>());
            binder.BindToSelfAll<IViewModel>(GetType().Assembly);
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