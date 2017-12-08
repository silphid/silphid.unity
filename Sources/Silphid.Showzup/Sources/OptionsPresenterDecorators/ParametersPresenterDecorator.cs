using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    internal class ParametersPresenterDecorator : OptionsPresenterDecoratorBase
    {
        private readonly object[] _instances;

        public ParametersPresenterDecorator(IPresenter presenter, object[] instances) : base(presenter)
        {
            if (instances.Any(x => x == null))
                throw new ArgumentNullException(nameof(instances), "Some instance was null");
                    
            _instances = instances;
        }

        protected override void UpdateOptions(object input, Options options)
        {
            if (_instances.Length == 0)
                return;
            
            if (options.Parameters == null)
                options.Parameters = new Dictionary<Type, object>();
            
            _instances.ForEach(x => options.Parameters[x.GetType()] = x);
        }
    }
}