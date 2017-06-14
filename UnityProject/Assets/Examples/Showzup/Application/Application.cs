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
            var container = new MicroContainer();

            container.BindInstance<ILoader>(CreateLoader());
            container.BindSingle<IViewResolver, ViewResolver>();
            container.BindInstance<IManifest>(Manifest);
            container.BindSingle<IVariantProvider, VariantProvider>();

            NavigationControl.Present(new Catalog())
                .SubscribeAndForget();
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