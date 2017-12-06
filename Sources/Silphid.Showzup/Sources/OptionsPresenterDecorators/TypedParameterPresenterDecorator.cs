using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    internal class TypedParameterPresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly Type _type;
        private readonly object _instance;

        public TypedParameterPresenterDecorator(IPresenter presenter, Type type, object instance) : base(presenter)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            
            _type = type;
            _instance = instance;
        }

        protected override void UpdateOptions(object input, Options options)
        {
            if (options.Parameters == null)
                options.Parameters = new Dictionary<Type, object>();
            
            options.Parameters[_type] = _instance;
        }
    }
}