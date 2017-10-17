using UnityEngine;

namespace Silphid.Loadzup
{
    public class PostFieldLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly string _key;
        private readonly string _value;
        
        public PostFieldLoaderDecorator(ILoader loader, string key, string value) : base(loader)
        {
            _key = key;
            _value = value;
        }

        protected override void UpdateOptions(Options options)
        {
            options.Method = HttpMethod.Post;

            var form = options.PostForm;
            if (form == null)
            {
                form = new WWWForm();
                options.PostForm = form;
            }
            
            form.AddField(_key, _value);
        }
    }
}