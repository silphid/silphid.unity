using Silphid.Loadzup;
using Silphid.Loadzup.Resource;
using Silphid.Showzup;
using Silphid.Showzup.Injection;
using UnityEngine;

namespace App
{
    public class Application : MonoBehaviour
    {
        public void Start()
        {
            var container = new MicroContainer();

            container.Bind<ILoader>(CreateLoader());
            container.Bind<IViewResolver>();
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