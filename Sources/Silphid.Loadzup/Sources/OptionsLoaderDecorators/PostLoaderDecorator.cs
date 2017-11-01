using Silphid.Loadzup.Http;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class PostLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly WWWForm _form;
        
        public PostLoaderDecorator(ILoader loader, WWWForm form) : base(loader)
        {
            _form = form;
        }

        protected override void UpdateOptions(Options options)
        {
            options.Method = HttpMethod.Post;
            options.PostForm = _form;
        }
    }
}