using log4net;
using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Loadzup.Resource;
using Silphid.Showzup;
using Silphid.Injexit;
using Silphid.Loadzup.Http;
using UniRx;

namespace App
{
    public class AppInstaller : RootInstaller
    {   
        // We use Log4net to log everything in Silphid.Unity and the app itself.
        // The level of verbosity for each namespace or class can be configured in the
        // Log4net.xml file in Assets/Examples/Showzup/Resources.
        //
        // For more information about Log4net see:
        // https://logging.apache.org/log4net
        
        private static readonly ILog Log = LogManager.GetLogger(typeof(AppInstaller));
        
        // The following fields must be specified in the inspector
        
        public Manifest Manifest;
        public NavigationControl NavigationControl;

        protected override void OnBind()
        {
            // These are Injexit bindings that configure Dependency Injection.
            // They are like mappings that specify which class to actually
            // instantiate behind the hood when something needs 
            //
            // For more information about Dependency Injection, see:
            // https://en.wikipedia.org/wiki/Dependency_injection
            
            BindLoadzup();
            BindShowzup();
        }

        private void BindShowzup()
        {
            Log.Info("Binding Showzup");
            
            // These bindings are rather standard for all projects and you 
                
            Container.Bind<IScoreEvaluator, ScoreEvaluator>().AsSingle();
            Container.Bind<IViewResolver, ViewResolver>().AsSingle();
            Container.Bind<IViewLoader, ViewLoader>().AsSingle();
            Container.BindInstance<IInjectionAdapter>(new InjectionAdapter(Container));

            // This binding registers the Manifest asset, which contains mappings between
            // all Models > ViewModels > Views > Prefabs.
            
            Container.BindInstance<IManifest>(Manifest);

            // This binding scans the currently assembly and registers all view models
            // so that they can be instantiated automatically at run-time
            
            Container.BindToSelfAll<IViewModel>(GetType().Assembly);

            // This binding defines the groups of variants used by the app to refine visuals depending
            // on context. Variant groups are totally custom for each app (in this case, we define
            // Display, Form and Platform), but it is recommended to have at least a Display group (or
            // some equivalent).  Have a look at these three classes to see what variants are part of
            // each group.
            
            Container.BindInstance(VariantProvider.From<Display, Form, Platform>());
        }

        private void BindLoadzup()
        {
            Log.Info("Binding Loadzup");
                
            // Main "composite" converter (used by all loaders) that dispatches to appropriate converter
            // when a loader needs to convert loaded bytes into an actual object, based on target Type to
            // load data into.  For example "loader.Load<Sprite>(uri)" will automatically use SpriteConverter
            // below.
            //
            // For more info on Composite design pattern see:
            // https://en.wikipedia.org/wiki/Composite_pattern
            
            Container.Bind<IConverter, CompositeConverter>().AsSingle().Using(x =>
            {
                x.Bind<IConverter, SpriteConverter>().AsList();
                x.Bind<IConverter, TextureConverter>().AsList();
            });

            // Main "composite" loader (used to load any resource or prefab) that dispatches to appropriate
            // loader, based on scheme (res://, http://...) of Uri to load. 
            
            Container.Bind<ILoader, CompositeLoader>().AsSingle().Using(x =>
            {
                x.Bind<ILoader, HttpLoader>().AsList();
                x.Bind<ILoader, ResourceLoader>().AsList();
            });
        }

        protected override void OnReady()
        {
            // This method is automatically called (after all bindings have been configured) to
            // launch the app. Here we simply ask the main NavigationControl (responsible for
            // displaying pages) to display the Catalog model. Because Catalog is just pure data,
            // it will automatically be wrapped into a CatalogViewModel, which will then be
            // displayed with a CatalogView (MonoBehaviour) in a CatalogPage prefab. The
            // NavigationControl has also been tagged with a "Page" variant, which will ensure
            // that anything that gets displayed in it will use the "Page" variant among possible
            // prefab visuals.
            
            Log.Info("Presenting Catalog (model) -> CatalogViewModel -> CatalogView -> CatalogPage (prefab)");

            NavigationControl
                .Present(new Catalog())
                .AutoDetach()
                .Subscribe(_ => {}, ex => Log.Error("Failed to navigate", ex), () => {});
            
            // If you wonder about the .AutoDetach().Subscribe() part, that's because
            // the .Present() method returns an Rx observable that needs to be subscribed to.
            //
            // For more information about Rx (Reactive Extensions or ReactiveX) in general see:
            // http://reactivex.io/
            //
            // For more information about UniRx (the Unity version of Rx) see:
            // https://github.com/neuecc/UniRx
        }
    }
}