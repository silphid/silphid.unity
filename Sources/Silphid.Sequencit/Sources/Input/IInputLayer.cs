using System;
using System.Collections.Generic;
using UniRx;

namespace Silphid.Sequencit.Input
{
    /// <summary>
    /// Object responsible for the gate-keeping of all user inputs. All input related functions should handle input only
    /// when IsEnabled is true.  All navigations and transitions should temporarily disable their input layer by calling
    /// Disable() on it and then disposing the returned IDisposable when the navigation or transition completes. Child
    /// input layers can be created for allowing only sub-sections of the interface to be disabled, which are also
    /// subordinated to their parent layers.  In other words, disabling a layer has the effect of automatically
    /// disabling all its child layers recursively.
    /// </summary>
    public interface IInputLayer
    {
        string Name { get; }

        IEnumerable<IInputLayer> Children { get; }

        IEnumerable<IInputLayer> SelfAndDescendants { get; }

        /// <summary>
        /// Whether this input layer (and all its children recursively) is
        /// </summary>
        IReadOnlyReactiveProperty<bool> IsEnabled { get; }

        /// <summary>
        /// Creates a subordinated child input layer, which will automatically be disabled when the present layer gets
        /// disabled.
        /// </summary>
        IInputLayer CreateChild(string name);

        /// <summary>
        /// Temporarily disables this input layer and all of its children recursively.  The returned disposable must be
        /// disposed in order to release that disabling.  Note that multiple consumers can request the input layer to be
        /// disabled and those requests are additive.  In other words, all requests must be disposed in order for the
        /// layer (and its children) to become enabled again.
        /// </summary>
        IDisposable Disable(string reason);
    }
}