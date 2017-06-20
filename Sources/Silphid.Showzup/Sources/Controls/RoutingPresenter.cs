using System;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    public class RoutingPresenter : MonoBehaviour, IPresenter
    {
        public GameObject[] SiblingPresenters = new GameObject[0];
        private IPresenter[] _siblingPresenters;
        
        public bool CanPresent(object input, Options options = null) =>
            true;

        internal void Start()
        {
            if (SiblingPresenters == null || SiblingPresenters.Length == 0)
                throw new NotSupportedException();

            _siblingPresenters = SiblingPresenters
                .Select(go =>
                {
                    var presenters = go
                        .GetComponents<IPresenter>()
                        .Where(presenter => !ReferenceEquals(presenter, this))
                        .ToArray();
                    
                    if (presenters.Length != 1)
                        throw new InvalidOperationException($"GameObject {go.name} should have one and only one IPresenter component.");
                    
                    return presenters.First();
                })
                .ToArray();
        }

        private IPresenter GetValidSiblingPresenter(object input, Options options = null) =>
            _siblingPresenters.FirstOrDefault(x => x.CanPresent(input, options));

        private IPresenter GetAncestorPresenter() =>
            this.Ancestors<RoutingPresenter>().FirstOrDefault();
        
        public IObservable<IView> Present(object input, Options options = null)
        {
            if (options?.Target == null)
                throw new InvalidOperationException("RoutingPresenter requires options to specify a Target variant.");
            
            var presenter = GetValidSiblingPresenter(input, options) ?? GetAncestorPresenter();
            if (presenter == null)
                throw new InvalidOperationException($"Failed to resolve routing for target: {options.Target}");

            return presenter.Present(input, options);
        }
    }
}