using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using UniRx;

namespace Silphid.Showzup.InputLayers
{
    public class InputLayer : IInputLayer, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InputLayer));

        private readonly ReactiveProperty<int> _refCount = new ReactiveProperty<int>(0);
        private readonly ReadOnlyReactiveProperty<bool> _isEnabled;
        private readonly List<IInputLayer> _children = new List<IInputLayer>();

        public InputLayer(string name, IInputLayer parent = null)
        {
            Name = name;

            var isEnabledObservable = _refCount.Select(x => x == 0);

            // Also take parent's state into account, if any
            if (parent != null)
            {
                ((InputLayer) parent)._children.Add(this);

                isEnabledObservable = isEnabledObservable.CombineLatest(parent.IsEnabled, (x, y) => x && y);
            }

            _isEnabled = isEnabledObservable.ToReadOnlyReactiveProperty();

            if (Log.IsDebugEnabled)
                _isEnabled.DistinctUntilChanged()
                          .Subscribe(x => Log.Debug($"{Name} IsEnabled: {x}"));
        }

        public string Name { get; }

        public IEnumerable<IInputLayer> Children => _children;

        public IEnumerable<IInputLayer> SelfAndDescendants =>
            _children.SelectMany(x => x.SelfAndDescendants)
                     .Prepend(this);

        public IReadOnlyReactiveProperty<bool> IsEnabled => _isEnabled;

        public IDisposable Disable(string reason)
        {
            RequestDisable(reason);

            return Disposable.Create(() => DisposeDisable(reason));
        }

        public void Dispose()
        {
            _refCount.Dispose();
            _isEnabled.Dispose();
        }

        private void RequestDisable(string reason)
        {
            _refCount.Value++;
            Log.Debug($"{Name} requested disable: {reason} (ref count: {_refCount.Value})");
        }

        private void DisposeDisable(string reason)
        {
            _refCount.Value--;
            Log.Debug($"{Name} disposed disable: {reason} (ref count: {_refCount.Value})");
        }
    }
}