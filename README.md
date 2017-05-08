# Silphid.Unity

*Silphid.Unity* is a collection of .NET libraries for the development of modern, robust and fluid Unity applications, with advanced sequencing and transitioning, asynchronous data loading, caching and conversion and a dynamic data-driven UI.

These libraries were inspired by the work I have been doing for the past 15 years or so at [Simbioz](http://simbioz.com) and then [LVL Studio](http://lvlstudio.com), initially targeting the WPF framework, but now completely re-thought and rewritten for [Unity](http://unity.com) and [UniRx](https://github.com/neuecc/UniRx) (the Unity version of [ReactiveX](http://reactivex.io)).

Even though they have been used to deliver multiple commercial-grade applications, they are still constantly evolving and improving, and I am just releasing them to the public for the first time, right now. Documentation is still in its early stages and more work still has to be done to make them easier to integrate into your Unity projects. But, as they say, we have to start somewhere! ;)

My sincere thanks to [LVL Studio](http://lvlstudio.com) for believing in the Open Source movement and supporting the development of these libraries. If you are looking for an *exceptional* job opportunity in the Montreal area, make sure to visit our [Careers](http://lvlstudio.com/en/careers) page! :)

## Dependencies

### Inter-Dependencies

The libraries were designed to minimize dependencies upon each other. However, higher level libraries do build upon lower level ones. For example, *Silphid.Extensions*, which is at the bottom of the stack only depends on Unity and can be used on its own, whereas *Silphid.Showzup*, at the top of the stack, builds upon all three other libraries.

![](SilphidDependencies.png "Dependencies")

In other words, there is no dependency from lower level libraries to higher level ones, so you may easily strip those higher level libraries in case you don't need them.

### Dependencies on Third-Parties

Silphid.Unity has optional integration adapters for those great  libraries:

- [UniRx](https://github.com/neuecc/UniRx) (optional for Silphid.Extensions, but required by the others)
- [DOTween](http://dotween.demigiant.com/)
- [Json.NET](http://www.newtonsoft.com/json)
- [TextMesh Pro](https://www.assetstore.unity3d.com/en/#!/content/84126)

These integrations have been isolated into their own sub-libraries (shown in the diagram as dark green boxes). Those can be stripped away without impacting anything else.  For example, *Silphid.Sequencit.DOTween* is an extension to *Silphid.Sequencit* that supports integration with *DOTween*, but is not required for Sequencit to work.

### Dependency on C# 6.0

Silphid.Unity also depends on, and takes advantage of, Alexzzzz's great [CSharp60Support](https://bitbucket.org/alexzzzz/unity-c-5.0-and-6.0-integration/src), which makes it possible to use most of the C# 6.0 language in Unity, even if Unity is still limited to .NET 3.5.

If you do **not** want to integrate CSharp60Support into your project and prefer sticking to C# 4.0 for your own scripts, you may simply use Silphid.Unity's precompiled DLLs, along with CSharp60Support's runtime DLLs.

Note that, on the Windows Store Application (WSA) platform, many of C# 6.0 features won't be available, such as async/await, Caller Info attributes and everything from System.Threading.dll (such as concurrent collections).

# Silphid.Extensions

*Silphid.Extensions* is a library of various helpers and extension methods for .NET, Unity, UniRx, DOTween, etc, that provide a concise fluent syntax for operations such as Abs/Sign/Floor/Ceiling, interpolation, clamping, wrapping, averaging, and much more.  This is where I put everything I feel is missing from the APIs I use everyday.

## Features

- Extension methods for float, double, int, long, DateTime, TimeSpan, string, IObservable<T>, IEnumerable<T>, GameObject, Transform, Vector2, Vector3, Quaternion, DOTween's Tween...
- Maths: sign, abs, floor, ceiling...
- Interpolation: linear, quad/cubic Bézier, inversed, transposition
- Wrapping, clamping, easing, filtering, smoothing, comparison
- UniRx VirtualTimeScheduler and TestScheduler (which Sequencit uses for unit testing)
- And much, much more!

## To Do

- Submit VirtualTimeScheduler/TestScheduler to UniRx 

# Silphid.Sequencit

*Silphid.Sequencit* extends UniRx with the concept of ISequenceable, for coordinating time-based operations in sequence or in parallel, such as playing a sound effect or animation, performing a transition between two pages or loading data from the net. Following the ReactiveX tradition, all those operations are modeled as IObservable<Unit>.

## Features

- Sequential and parallel sequencing
- Sequencer (live sequencing queue)
- Wait until some disposable is disposed, until some observable's OnNext, for some delay...
- Extension methods for IObservable<T> and DOTween's Tween

## To Do

- Submit new VirtualTimeScheduler/TestScheduler to UniRx (which Sequencit uses for unit testing).

# Silphid.Loadzup

*Silphid.Loadzup* streamlines the loading of all assets and resources into a uniform pipeline. Typically in Unity, depending on the source you want to load from (WWW, Resources, AssetBundleManager, etc.) you need to use a different class, with different syntax and peculiarities. And if you change the type of storage for an asset, you actually need to change your code.

All assets can be addressed by URI and if you change the type of storage for an asset, you only need to change the URI that points to it, not the code that accesses it.

## Features

- Unified, asynchronous, IObservable-based loading pipeline
- URI-based asset addressing according to schemes: http://, bundle://, res://
- Support for loading from Resources, WWW, Asset Bundles
- Support for Content/MIME types and custom request headers
- Automatic conversion, with built-in converters for Sprite, Texture and Json.NET
- Advanced caching policies (ETag/Last-Modified, cache/origin-first...)
- Highly modular and extensible (pluggable loaders and converters)

## Under Development

- Complete AssetBundleManager rewrite (by crazydadz)

## To Do

- Support for Unity's new WebRequest (instead of WWW)
- Cache expiration
- Time-out as an option (more robust than IObservable.Timeout())
- Priority queues
- Cancellation Tokens
- IProgress
- Scene Loading
- Asset Bundles

# Silphid.Showzup

*Silphid.Showzup* is a lightweight MVVM framework for dynamic, data-driven UI, asynchronous loading of views, custom visual transitions, multiple variants for each view, etc. It leverages Sequencit and Loadzup for flexible and fluid loading and animations.

## Features

- Abstract, flexible and lightweight
- Independent from GUI framework
- Simple attribute-based mapping of Models, ViewModels and Views
- Data-driven UI (simply assign any Model/ViewModel object to a control and it will resolve and load the proper View to display it)
- Abstract container controls (can be skinned with arbitrary UI)
    - ItemControl (displays a single item in a view)
    - ListControl (displays a collection of items in multiple  views)
    - SelectionControl (extends ListControl with current item awareness)
    - TransitionControl (extends ItemControl with visual transitions between views)
    - NavigationControl (extends ItemControl with browser-like Back/Forward navigation support)
- Views are defined as prefabs
- Each view can have multiple variants
- Customizable transition system based on IObservable
    - Built-in transitions for uGUI (crossfade, slide, zoom, instant)
    - The sequencing of transitions allows phases (load/show/transition/hide...) to take as much time as they need.
- Simple binding extensions for uGUI and TextMeshPro
- And much, much more!

## Under Development

- Customizable multi-phase transitioning

## To Do

- Support for hierarchy of variants
- Fluent syntax for Model/ViewModel/View mapping (instead of attribute-based)

# Experimental Libraries

Some unclassified libraries, still in an early development stage, to be evolved, reorganized and time-tested.

## Silphid.Abstracam

A lightweight system for manipulating and interpolating between virtual cameras.  You can manipulate multiple virtual cameras freely and then interpolate a real camera between them.

- Pluggable camera system: FreeCamera, ReadOnlyCamera, TransitionCamera, Smoothing Camera.
- Cameras can be observed as Observables.

## Silphid.Sequencit.Machines

An experimental UniRx-based state machine, currently included in *Silphid.Sequencit*, but should be externalized eventually.

## Silphid.Sequencit.Input

An experimental input layering/filtering system, currently included in *Silphid.Sequencit*, but should be externalized eventually.

- Allows multiple input layers to be defined in a nested hierarchy.
- Each layer can be disabled, including all its nested descendant layers.
- Multiple parts of the code can request a layer to be disabled and only when all of them have released this request (by disposing their IDisposable) will the layer effectively be reactivated.  This is what I call the "Flag" pattern (an implementation of which can be found in Silphid.Extensions.DataTypes).
- This system is abstract and independent from any input system. When detecting some input, you have to manually query a layer to determine whether it is currently active or not.