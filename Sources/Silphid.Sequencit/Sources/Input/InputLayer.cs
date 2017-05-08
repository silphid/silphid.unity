using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Sequencit.Input
{
    public class InputLayer : IInputLayer, IDisposable
    {
        public static bool IsLogEnabled = false;

        private readonly ReactiveProperty<int> _refCount = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<bool> _isEnabled;
        private readonly List<IInputLayer> _children = new List<IInputLayer>();

        public InputLayer(string name, IInputLayer parent = null)
        {
            Name = name;

            var isEnabledObservable = _refCount.Select(x => x == 0);

            // Also take parent's state into account, if any
            if (parent != null)
                isEnabledObservable = isEnabledObservable
                    .CombineLatest(parent.IsEnabled, (x, y) => x && y);

            _isEnabled = isEnabledObservable.ToReactiveProperty();

            if (IsLogEnabled)
                _isEnabled
                    .DistinctUntilChanged()
                    .Subscribe(x => Debug.Log($"#InputLayer# {Name} IsEnabled: {x}"));
        }

        public string Name { get; }

        public IEnumerable<IInputLayer> Children => _children;

        public IEnumerable<IInputLayer> SelfAndDescendants =>
            _children
                .SelectMany(x => x.SelfAndDescendants)
                .Prepend(this);

        public IReadOnlyReactiveProperty<bool> IsEnabled => _isEnabled;

        public IInputLayer CreateChild(string name)
        {
            var child = new InputLayer(name, this);
            _children.Add(child);
            return child;
        }

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
            if (IsLogEnabled)
                Debug.Log($"#InputLayer# {Name} request disable: {reason} (ref count: {_refCount.Value + 1})");

            _refCount.Value++;
        }

        private void DisposeDisable(string reason)
        {
            if (IsLogEnabled)
                Debug.Log($"#InputLayer# {Name} dispose disable: {reason} (ref count: {_refCount.Value - 1})");

            _refCount.Value--;
        }
    }
}