using Silphid.Loadzup;
using Silphid.Loadzup.Resource;
using Silphid.Showzup.Injection;
using UnityEngine;

namespace App
{
    public class Application : MonoBehaviour
    {
        public void Start()
        {
            var container = new MicroContainer();
        
            var compositeConverter = new CompositeConverter(
                new SpriteConverter());
        
            var compositeLoader = new CompositeLoader(
                new ResourceLoader(compositeConverter));
        
            container.Bind<ILoader>(compositeLoader);
        }
    }
}