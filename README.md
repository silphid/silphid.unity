# Silphid.Unity

*Silphid.Unity* is a collection of high quality *.NET* libraries for the development of modern and fluid *Unity* applications leveraging *Reactive Extensions* and the *MVVM* pattern to achieve a fully dynamic, data-driven UI, advanced animation sequencing and transitioning, streamlined dependency injection, as well as asynchronous data loading, caching and conversion.

*Silphid.Unity* was inspired by my work of the past 17 years at [Simbioz](http://simbioz.com) and then [LVL Studio](http://lvlstudio.com), initially targeting the WPF framework, but now completely rethought and redesigned for [Unity](http://unity.com) and [Reactive Extensions](http://reactivex.io).

Even though it has been used to deliver multiple commercial-grade applications, it is still constantly evolving and improving, and I am just now releasing it to the public for the first time. Documentation and examples are still in their early stages and more work still has to be done to make the whole thing easier to integrate into your Unity projects. But, as they say, we have to start somewhere! ;)  All your comments are more than welcomed!

My sincere thanks to [LVL Studio](http://lvlstudio.com) for supporting this effort and being such a great place to work at. If you are looking for an outstanding job opportunity in the Montreal area, make sure to visit our [Careers](http://lvlstudio.com/en/careers) page! :)

# Libraries

- [Extensions](#Extensions) - Extension methods (and more) for *.NET*, *Unity*, *UniRx*, *DOTween*, etc, providing a concise fluent syntax for many useful operations.
- [Sequencit](#Sequencit) - Rx-based sequencing of elements with dynamic durations.
- [Injexit](#Injexit) - Lightweight dependency injection framework with a clean and efficient fluent syntax.
- [Machina](#Machina) - Lightweight Rx-based state machine with polymorphic states.
- [Loadzup](#Loadzup) - Rx-based asynchronous asset/resource/object loading and conversion, with simple URI-addressing.
- [Showzup](#Showzup) - Full-fledged MVVM framework with asynchronous data-driven UI, custom transitions and dynamic view variants.

# Getting started

- Download the latest release from [GitHub](https://github.com/Silphid/Silphid.Unity/releases).
- Drag the package file onto your Project window in Unity.
- Uncheck the files you do not need, while respecting the dependencies described in the *Dependencies* section.
    - I recommend initially importing all files, as it is easier to remove them afterwards, once  your code compiles (and is under source control, in case you need to revert or recover anything).
    - If you are not targeting the Windows Store, you may uncheck the `Plugins/WSA` folder.
- Try the examples and look at the scripts for those examples (there are lots of comments there).  More examples coming soon.

# Building from source

- If you intend to target the Windows Store / Xbox One, ensure your machine is configured properly by following these [instructions](https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/development-environment-setup) first.
- Clone the repository from [GitHub](https://github.com/Silphid/Silphid.Unity.git).
- Do **not** open the Unity project just yet.  The `dll`s and `pdb`s must be compiled first, otherwise Unity will remove their corresponding `.meta` files and therefore lose their platform configs and placeholder mappings.
- Open and build this solution: `/Sources/Silphid.Unity.sln`
  - If not targeting the Windows Store, you may ignore all projects ending with `.UWP`
- All projects have a post-build step that copies their output `dll` and `pdb` to:
    - `/UnityProject/Assets/Plugins` (for regular .NET 3.5 projects)
    - `/UnityProject/Assets/Plugins/WSA` (for `.UWP` projects)
- You can now open the Unity project at `/UnityProject`.

# Dependencies

## Inter-Dependencies

The libraries were designed to minimize dependencies upon each other. However, higher level libraries do build upon lower level ones. For example, *Extensions*, which is at the bottom of the stack, only depends on Unity and can be used on its own, whereas *Showzup*, at the top of the stack, builds upon all the other libraries.

![](Doc/Overview.png "Dependencies")

In other words, there is no dependency from lower level libraries to higher level ones, so you may easily strip those higher level libraries if you don't need them.  Many of the integration modules (shown as dark green boxes) are optional and can be removed (see *Optional Dependencies* below).

## Required Dependencies

### Unity 2017.1

*Silphid.Unity* requires Unity 2017.1 (still in beta, as of this writing) for its .NET 4.6 and C# 6.0 support.

### UniRx

Silphid.Unity also requires (and is distributed with) [UniRx](https://github.com/neuecc/UniRx) 5.5.0, the Unity implementation of the [Reactive Extensions](reactivex.io) (*Rx*).  It is a great and thorough library on which *Silphid.Unity* relies heavily (for example, all asynchronous operations return or use some form of `IObservable<T>`).

If you are new to Rx, there are plenty of resources on the Net.  Here are my personal recommendations:

- [The introduction to Reactive Programming you've been missing](https://gist.github.com/staltz/868e7e9bc2a7b8c1f754) - A great intro and overview. The place to start.
- [IntroToRx.com](introtorx.com) - The complete online version of a great book by Lee Campbell, very thorough and insightful.  But start with the previous link first! ;)
- [ReactiveX.io](reactivex.io) - The official reference hub for implementations of Rx on all platforms.
- [RxMarbles](rxmarbles.com) - Interactive diagrams to experiment with and better understand observables and their various operators.
- [UniRx](https://github.com/neuecc/UniRx) - Obviously, the UniRx documentation is another good read, with many good examples specific to Unity.

## Optional Dependencies

All optional integrations with third-parties have been isolated into their own sub-libraries or script sub-folders, that can be stripped away without impacting anything else.  For example, *Sequencit.DOTween* is an extension to *Sequencit* that supports integration with *DOTween*, but is not required for *Sequencit* to work.

### DOTween (only required for *Showzup*)

*Silphid.Unity* can be used with DemiGiant's [DOTween](http://dotween.demigiant.com/), a great high-performance tweening/animation library for Unity with a rich and clean fluent API.  It supports low-level time-based sequencing, which *Sequencit* nicely complements with higher-level observable-based sequencing.

*Note: The latest version of DOTween is distributed with Silphid.Unity in binary form, including a version that has been converted to UWP for Windows Store support (required for building the `.DOTween.UWP` projects from source).*

#### How to remove it, if you don't need it

Simply remove all `dll`s and `pdb`s with `DOTween` in their name from the `Assets/Plugins` and `Assets/Plugins/WSA` folders.

### Json.NET

Newtonsoft's [Json.NET](http://www.newtonsoft.com/json) is a very useful library for (de)serializing objects to/from JSON format.  However, it had issues with Unity, especially on iOS because of AOT, and therefore has been ported to Unity by [ParentElement](https://www.assetstore.unity3d.com/en/#!/content/11347) (there's also a version by [SaladLab](https://github.com/SaladLab/Json.Net.Unity3D), but I haven't tried it).

#### How to enable the Silphid.Unity integration scripts

Because *Json.NET* is a paid asset, it is not distributed with *Silphid.Unity* and you must therefore explicitly enable the integration scripts by adding the `JSON_NET` define to your Unity project (`Edit` > `Project Settings` > `Player` > `Other Settings` > `Scripting Define Symbols`).

# Extensions<a id="Extensions"></a>

*Extensions* is a library of various helpers and extension methods for types from *.NET*, *Unity*, *UniRx*, *DOTween*, etc, that provides a concise fluent syntax for many common or more complex operations. This is where I put everything I feel is missing from the APIs I use everyday.

## Features

- Extension methods for `float`, `double`, `int`, `long`, `string`, `DateTime`, `TimeSpan`, `IObservable<T>`, `IEnumerable<T>`, `GameObject`, `Transform`, `Vector2`, `Vector3`, `Quaternion`, `Tween`...
- Maths: `sign`, `abs`, `floor`, `ceiling`...
- Interpolation: linear, quad/cubic Bézier, inversed interpolation (ratio), transposition (from one range to another)...
- Wrapping, clamping, easing, filtering, smoothing, comparing
- `VirtualTimeScheduler` and `TestScheduler` that are currently missing from *UniRx* (*Silphid.Sequencit* uses those for unit testing), but I have submitted a pull request for them to be merged into *UniRx*.
- And much more!

# Sequencit

*Sequencit* extends *UniRx* with the concept of `ISequencer` for coordinating time-based operations in sequence or in parallel, such as playing sound effects and animations, performing transitions between pages or loading data from the Net. In the Rx tradition, all such operations are represented as `IObservable<Unit>`.

## Features

- `Sequence` - an *observable* sequencer to which you can add multiple observables to be executed sequentially.  It completes once its last child has completed.
- `Parallel` - an *observable* sequencer to which you can add multiple observables to be executed in parallel.  It completes once all its children have completed.
- `SequenceQueue` - a *non-observable* sequencer to which you can add multiple observables to be executed sequentially, as soon as possible.  Because it is not an observable it does not require subscribing to it and never completes.  As soon as it is created, it starts waiting for operations to be added to it, then executes those operations in order and, when it reaches the end of its queue, simply waits for more operations to be added to it.
- Various waiting and synchronization options:
  - Wait for a disposable to be disposed;
  - Wait for an observable's `OnNext`/`OnCompleted`;
  - Wait for some time interval...
- Extension methods for `IObservable<T>` and DOTween's `Tween`.

![](Doc/Sequencit.png "Sequencit")

# Injexit

*Injexit* is a simple dependency injection framework that I created as an alternative to [Zenject](https://github.com/modesttree/Zenject), a great dependency injection framework for Unity.  I love *Zenject*, it is very powerful, robust and mature, however I needed *Showzup* to be independent from *Zenject* and also needed some specific features that *Zenject* was lacking.  *Injexit* started as a minimalist container (with a single class!) and quickly evolved into a full-fledged framework with a clean and efficient fluent syntax.

I am planning to refactor *Showzup* to make it plug-and-play compatible with both *Zenject* and *Injexit*, so that you may choose either framework.

### Dummy example types

For the examples in the following section, we will use those simple dummy types: 

```c#
public interface IFoo {}
public interface IGoo {}
public interface IBar {}

public class Foo : IFoo {}
public class Goo : IGoo {}

public class Bar : IBar
{
  public Bar(IFoo foo, IGoo goo)
  {
  }
}
```

### Types of injection

In most dependency injection frameworks, there are three different approaches for injecting dependencies into an object, with their pros and cons, as well as limitations.

#### Constructor injection

```c#
public class Bar : IBar
{
  private readonly IFoo _foo;
  private readonly IGoo _goo;
  
  public Bar(IFoo foo, IGoo goo)
  {
    _foo = foo;
    _goo = goo;
    
    // Initialization code can go here, because all dependencies are received at once.
  }
}
```

This is the best way to inject dependencies because:

- All dependencies are clearly visible as constructor parameters.
- It is easy to create and initialize the object without using any dependency injection framework, for example in your unit tests. When an object is instantiated manually in this way, it is instantly ready to be used.
- All initialization code can be placed directly into the constructor, because it receives all dependencies at once.
- There is no need for exposing fields or writable properties.
- There is no need for injection attribute annotations; the framework can figure dependencies simply by looking at constructor parameter types.
- Fields can be marked `readonly` and are garanteed to be constant throughout the object's life-time.

However, this approach is simply not possible for instances that are already created (or at least not created by the container), which is the case for all game objects and components, as they are created by Unity (when a scene or prefab is loaded).

*Use this approach for everything that is not a MonoBehaviour.*

If a class has multiple constructors, you must mark only one of them with the `[Inject]` attribute to let *Injexit* know which one to inject.

``` c#
public class Bar : IBar
{
  public Bar()
  {
    // This constructor will be ignored
  }
  
  [Inject]
  public Bar(IFoo foo, IGoo goo)
  {
    // This constructor will be injected
  }
}
```

#### Method injection

When you cannot use *constructor injection*, this approach is a good compromise, with some of the same advantages.

```c#
public class Bar : IBar
{
  private IFoo _foo;
  private IGoo _goo;
  
  [Inject]
  public void Init(IFoo foo, IGoo goo)
  {
    _foo = foo;
    _goo = goo;
    
    // Initialization code can go here, because all dependencies are received at once.
  }
}
```

- There is a single and clean entry point for all dependencies.
- However, fields *cannot* be marked `readonly`.

*Use this approach for all MonoBehaviours.*

#### Field or property injection

```c#
public class Bar : IBar
{
  [Inject]
  public IFoo Foo;

  [Inject]
  public IGoo Goo;

  // This part is only needed when you have initialization code that relies on Foo and Goo
  [Inject]
  public void Init()
  {
    // Initialization code can go here, because fields and properties are injected before methods.
  }
}
```

*Avoid this approach, as it is messier and more error prone, and rather use method injection for MonoBehaviours.*

### The Container

The `Container` class is the central class in *Injexit*. You create an instance of it at startup, use it to configure all your type *bindings*, and then let it perform its wonders.  That class implements the `IContainer` interface, which itself derives from the `IBinder`, `IResolver` and `IInjector` interface. Those interfaces correspond to the three types of operations you may need to perform in your application. We will explore those three types of operations in the following sections named respectively *Binding*, *Resolving* and *Injecting*.

![](Doc/InjexitContainer.png "IContainer")

In the spirit of SOLID principles, those three interfaces were kept as small and simple as possible, and all of the sophisticated operations that you can do with them have been externalized into extension methods.

### IBinder

From an application development perspective, this is the most important part of the container. It allows to map all your interfaces to concrete classes at startup, which mappings (*bindings*) are then used at run-time to resolve, instantiate and inject the proper instances.

We will demonstrate that interface and its extension methods in action in the *Binding* section below.

### IResolver

You typically won't need to use this interface, as it is mainly used internally by *Injexit* to resolve the dependencies of each object, but in more advanced scenarios it can prove very useful, as we will see in the *Resolving* section below.

### IInjector

This interface allows to manually inject dependencies into already created instances, which is necessary for example with game objects, as they are instantiated by Unity when a scene or prefab is loaded. More on that in the *Injecting* section below.

For manually injecting dependencies, you cannot use *constructor injection*, because the instance has already been create, and you must instead rely on *field* or *property injection*, as described in the *Types of injection* section below.

### Setting up dependencies at startup

Simply extend the `RootInstaller` class, override the `OnBind()` method and place your bindings in it, and attach it to some root game object in your scene (it's a `MonoBehaviour`):

```c#
using Silphid.Injexit;
using UnityEngine;

public class AppInstaller : RootInstaller
{
  protected override void OnBind(IBinder binder)
  {
    // Add your bindings here
    binder.Bind<IFoo, Foo>();
    binder.Bind<IGoo, Goo>();
    binder.Bind<IBar, Bar>();
  }
  
  protected override void OnReady()
  {
    // All game objects in current scene have now been injected
  }
}
```

### Binding

Because the `IContainer` also implements `IBinder`, all the following examples would also work directly with the container. It is just a best practice to work with the most specific interface (for instance, `IBinder`).

#### Binding to a concrete type

This is the best type of binding, because it lets *Injexit* create the object and resolve all sub-dependencies:

```c#
binder.Bind<IFoo, Foo>();
binder.Bind<IGoo, Goo>();
binder.Bind<IBar, Bar>();
```

Here, all `Bar` instances will automatically have their constructor injected with instances of `Foo` and `Goo`, because the `Bar` constructor has `IFoo` and `IGoo` parameters, which are respectively bound to the `Foo` and `Goo` concrete classes.

#### Binding to an instance

In cases where you already have an instance and simply want to inject it into other objects, use this form:

```c#
Foo fooInstance = ...
binder.BindInstance<IFoo>(fooInstance);
```

Or if your instance is already downcasted to the proper interface type, you may omit that type for brevity:

```c#
IFoo fooInstance = ...
binder.BindInstance(fooInstance);
```

Instance binding is useful when you need to inject a specific game object or component into another object. Just expose a field on you script (that you may assign via the Inspector) and bind that field with ```BindInstance()```:

```c#
using Silphid.Injexit;
using UnityEngine;

public class AppInstaller : RootInstaller
{
  // Assign a value to this field via the Inspector
  public MyComponent myComponent;

  protected override void OnBind(IBinder binder)
  {
    binder.BindInstance(myComponent);
    
    // Equivalent to:
    // binder.BindInstance<MyComponent>(myComponent);
  }
}
```

For anything else than MonoBehaviours, the downside of binding a specific instance is that the instance will not have its own dependencies injected, because *Injexit* did not instantiate it. For MonoBehaviours part of your scene, that's OK, because they get injected automatically anyway by the Installer.

#### Binding to a shared object

By default, *Injexit* will create a new instance of a class for every other class that depends on it. This is a good pattern, because it limits side-effects between objects. However, sometimes you want an object to be shared, so that changes made to it will be visible to all other objects sharing it as a dependency. This is achieved using the `.AsSingle()` suffix:

```c#
binder.Bind<IFoo, Foo>().AsSingle();
```

Note that instance binding using `BindInstance()` does not support the `.AsSingle()` suffix, because instances specified explicitly are forcibly always shared.

#### Binding to a list of objects

Sometimes, you want many objects implementing the same interface to be injected together as a `List<T>`, `IEnumerable<T>` or `T[]`:

```c#
public class Foo1 : IFoo {}
public class Foo2 : IFoo {}
public class Foo3 : IFoo {}

public class Bar : IBar
{
  public Bar(List<IFoo> foos)
  {
  }
}
```

In that case, you would bind them with the `.AsList()` suffix:

```c#
binder.Bind<IFoo, Foo1>().AsList();
binder.Bind<IFoo, Foo2>().AsList();
binder.Bind<IFoo, Foo3>().AsList();
```

Here, the `Bar` constructor will be injected with a `List<IFoo>` containing instances of the `Foo1`, `Foo2` and `Foo3` classes.

Note that `.AsList()` will work, no matter if the dependent object expects a `List<T>`, an `IEnumerable<T>` or a `T[]`.

#### Forwarding multiple interfaces to same binding

Sometimes the same class implements multiple interfaces and you want all those interfaces to be bound to the same object, with the same binding options.

```c#
public class FooGooBar : IFoo, IGoo, IBar {}
```

Simply bind the first interface the regular way and then use `BindForward()` to forward the other interfaces to the first one:

```c#
binder.Bind<IFoo, FooGooBar>().AsSingle();
binder.BindForward<IGoo, IFoo>();
binder.BindForward<IBar, IFoo>();
```

Here, the `IFoo`, `IGoo` and `IBar` interfaces will all be bound to the same `FooGooBar` instance. 

#### Self-binding a type

Even though it is a best practice to access most of your concrete classes through abstract interfaces, especially in the context of dependency injection, in some cases you might only have a concrete type with no interface and prefer to inject it as is.

``` c#
binder.BindToSelf<Foo>();
binder.BindToSelf<Goo>();
```

*Showzup* uses that approach for models and view models, which do not have each their own interface.

*This approach however defeats multiple advantages of dependency injection, such as being able to swap an implementation of an interface with some other arbitrary implementation, so try to avoid it, unless it's part of your design.*

#### Self-binding all of a type's derivatives

```c#
binder.BindToSelfAll<IFoo>();
```

This will scan the assembly where `IFoo` is defined for all classes implementing that interface (for instance, `Foo1`, `Foo2` and `Foo3`) and self-bind them. In this case, the `IFoo` interface serves only as a marker. This approach is used in *Showzup* to automatically bind all view model classes.

You may also specify the assembly to scan explicitly:

```c#
binder.BindToSelfAll<IFoo>(GetType().Assembly);
```

#### Self-binding multiple instances

If you want multiple instances of different types, that will only be known at run-time, to be bound to their own type, you can use the `BindInstances()` extension method:

``` c#
IFoo foo = new Foo();
IGoo goo = new Goo();
binder.BindInstances(foo, goo);
```

Or:

```c#
var objects = new object[] { new Foo(), new Goo() };
binder.BindInstances(objects);
```

This is equivalent to:

``` c#
Container.BindInstance<Foo>(foo);
binder.BindInstance<Goo>(goo);
```

*Note that, even if `foo` and `goo` are downcast to interfaces, `BindInstances()` always binds each object to its own type and disregards the interfaces it implements. That means that the dependent object must also declare its dependencies using those concrete types.*

### Resolving

You typically won't need to resolve dependencies manually, as *Injexit* will do it automatically for you behind the scenes. *It is considered a best practice to **only** use such explicit dependency resolution in your bootstrapping code and **avoid it completely** everywhere else in your application.*

Because the `IContainer` also implements `IResolver`, all the following examples would also work directly with the container. It is just a best practice to work with the most specific interface (for instance, `IResolver`).

#### Resolving a type known at compile-time

```c#
resolver.Resolve<IFoo>();
```

#### Resolving a type only known at run-time

```c#
resolver.Resolve(type);
```

#### Overriding bindings at resolution-time

When you want to specify bindings that should only apply to the current resolution operation (or that should temporarily override existing bindings), you can create a sub-container on the fly with the `Using()` extension method, just before calling `Resolve()`:

```c#
resolver
  .Using(binder => binder.Bind<IFoo, Foo2>())
  .Resolve<IBar>();
```

### Injecting

Because the `IContainer` also implements `IInjector`, all the following examples would also work directly with the container. It is just a best practice to work with the most specific interface (for instance, `IInjector`).

#### Injecting an existing instance

``` c#
injector.Inject(instance);
```

This will only inject the dependencies of given instance.

#### Injecting all scripts of a game object

```c#
injector.Inject(gameObject);
```

This will recursively inject the dependencies of all `MonoBehaviour` scripts attached to given `GameObject` and its descendants.

#### Injecting all scripts of a scene

After you have set up all your bindings, the `RootInstaller` and `SceneInstaller` will automatically call this method to inject all game objects in your scene:

```c#
injector.InjectScene(gameObject.scene);
```

#### Overriding bindings at injection-time

When you want to specify bindings that should only apply to the current injection operation (or that should temporarily override existing bindings), you can create a sub-container on the fly with the `Using()` extension method, just before calling `Inject()`:

``` c#
injector
  .Using(binder => binder.Bind<IFoo, Foo2>())
  .Inject(obj);
```

# Machina

*Machina* is a simple Rx-based polymorphic state machine.  It is said to be *polymorphic* in that it builds upon the general principle that a state machine should be in one and only one state at any given time, and extends it to allow some states to be polymorphically equivalent to others (in other words, inherit from others).

## State polymorphism example

The addition of polymorphism greatly simplifies the modelling of states and the handling of changes between them. For example, here are a few states for objects in a game I am working on: 

![](Doc/MachinaPolymorphism.png "Machina Polymorphism")

An object can either be *Idle* or *Busy*.  But *Moving* and *Falling* are both considered equivalent to being *Busy*.  There are also two ways of being considered *Moving*, by *Pushing* some other object or being *Pushed* by it.

It is then easy to handle whenever an object enters or leaves the *Busy* state, no matter the specific state that made it busy. The same applies to *Moving*, which can be handled generically, independently from the specific kind of movement.

## Wishlist

- Allow some states to be marked as *Abstract*, to prevent entering those states directly (only their derived states) and make intents clearer.

# Loadzup

Typically in *Unity*, depending on the source you want to load your assets from (*WWW*, *Resources*, *AssetBundleManager*...), you need to use a different class, with different syntax and peculiarities. And if you change the type of storage for an asset, you actually need to change your code.

*Loadzup* streamlines the loading of all assets and resources into a uniform, asynchronous pipeline, allowing assets to be addressed by URI.  If you change the type of storage for an asset, you only need to change the URI that points to it, not the code that accesses it.

For example, *Showzup* leverages *Loadzup* in order to load views from prefabs.  Any prefab can be associated with a view simply via its URI, no matter if it is stored in resources or in an asset bundle.

## Features

- Unified, asynchronous, IObservable-based loading pipeline.
- URI-based asset addressing according to schemes: ```http://```, ```bundle://```, ```res://```
- Support for loading from WWW, Resources (and soon AssetBundleManager).
- Support for Content/MIME types and custom request headers.
- Automatic conversion, with built-in converters for Sprite, Texture and Json.NET.
- Advanced caching policies (ETag/Last-Modified, cache/origin-first…).
- Highly modular and extensible (pluggable loaders and converters).

![](Doc/Loadzup.png "Loadzup")

## Under Development

- Complete rewrite of Unity's `AssetBundleManager` (by [crazydadz](github.com/crazydadz)).
- AssetBundleManager support.

## Wishlist

- Support for Unity's new `WebRequest` (as replacement for `WWW`).
- Cache expiration
- Time-out as an option (more robust than `IObservable.Timeout()`).
- Priority queues
- `CancellationToken` support
- `IProgress` support
- Scene Loading

# Showzup

*Showzup* is a Rx-based MVVM framework for dynamic, data-driven UI, asynchronous loading of views, custom visual transitions, multiple variants for each view, etc. It leverages Sequencit and Loadzup for flexible and fluid loading and animations.

## Features

- Abstract, flexible and lightweight
- Independent from GUI framework
- Automatic mapping of *Models*, *ViewModels*, *Views* and *Prefabs*.
- Data-driven UI (simply pass any *Model* object to a control and it will resolve and load the proper ViewModel and View to display it)
- Built-in, reusable, skinable controls (single item, list, selection...)
- Views are defined as prefabs
- Each ViewModel and View can have multiple variants (ie: for different platforms, form-factors, etc.)
- Customizable Rx-based transition system
    - Built-in transitions for uGUI (crossfade, slide, zoom, instant)
    - The sequencing of transitions allows phases (load/show/transition/hide...) to take as much time as they need.
- Simple extension methods for binding uGUI objects to *ViewModel* properties.
- Automatic routing of presentation requests (simply specify a `Target` variant in your `Options` and let `RoutingPresenter`s forward your request to the proper control).
- And much more!

## Overview

*Showzup* adopts a *data-driven* approach to UI development. You simply pass the raw data (the *Model*) to a *Control* and let it figure how to display it.  *Showzup* resolves which *ViewModel* to wrap the *Model* with, and which *View* and *Prefab* to show inside the control.  In MVVM, the *ViewModel*'s job is to augment the raw *Model* with all the necessary business and presentation logic, often by resorting to some services to interact with the rest of the application and the outside world.

![](Doc/Showzup.png "Showzup Overview")

## Models

Any object can be used as a *model*. Those objects should be as simple as possible and are therefore considered *POCO*s (Plain Old CLR Objects). Ideally, they even consist only of data, without much or any functionality, in which case they can be considered *DTO*s (Data Transfer Objects), as they only carry data between the different layers of your app (i.e.: from the network or disk).  They might sometimes require special attributes for deserializing from JSON or XML and might have to make design trade-offs in order to be compatible with those serialization methods; that's all right, as *ViewModels* will wrap those objects to provide a friendlier API for *Views*. 

## ViewModels

*ViewModel* classes must inherit from `ViewModel<TModel>`, where *TModel* is the type of the *Model* it wraps, and must also provide a public constructor with at least one parameter of type *TModel*, into which the model will be injected. Other dependencies, such as *Services*, can also be injected via other parameters of the constructor (this is what allows the *ViewModel* to interact with the rest of the application and the outside world).

The base class defines a handy `Model` property of type *TModel*, which you can use to access the underlying model's data.

## Views

*View* classes must inherit from `View<TViewModel>`, where *TViewModel* is the type of the *ViewModel* it wraps. *Views* are MonoBehaviours that you must attach to some *GameObjects* in your view *Prefabs* and, as such, cannot benefit from constructor injection like *ViewModels* do.  They must instead rely on property injection.

The base class defines a handy `ViewModel` property of type *TViewModel*, which you can use to bind data to your UI GameObjects.

Typically, all views override the `Load()` method, which is where binding are made and where resources are loaded asynchronously:

```c#
public class FooView : View<FooViewModel>
{
  public Image Photo; // Must be assigned an Image game object in Inspector window
  public Text Title;  //  "          "    a Text          "           "
  
  public override IObservable<Unit> Load()
  {
    Bind(Title, ViewModel.Title);
    return BindAsync(Photo, ViewModel.PhotoUri); // View will be presented only *after* photo is finished loading.
  }
}
```

If no asynchronous binding are performed in the Load() method (or if you want the view to be presented immediately, without waiting for its content to load first), just return `Observable.Empty<Unit>` or `null`, like in the following example, where the view will be presented immediately, and the image it contains will load asynchronously and appear in a second time:

```c#
  public override IObservable<Unit> Load()
  {
    Bind(Title, ViewModel.Title);
    Bind(Photo, ViewModel.PhotoUri);
    return null; // View will be presented immediately, without waiting for photo to load.
  }
```

## Prefabs

You define the visual representation of your views as simple prefabs.  On the root game object of that prefab you must attach a view-derived mono behaviour that will be the link between your UI and code. In that prefab you create your layout and place different UI objects.  You then drag your various UI objects onto the properties of the view in the Inspector window, in order to make them accessible to your view's code.

All your view prefabs must be contained within your resources or asset bundles, so that they may be loaded at run-time (more on that in the *Manifest* section).

## Controls

*Controls* are the generic containers responsible for presenting arbitrary raw data (the *model*) in a visual way (the *view+prefab*). *Showzup* provides a series of useful *controls*, which can be skinned freely:

- `ItemControl` displays a single item as a single *view*;
- `ListControl` displays a collection of items as multiple *views* (potentially of different types); 
- `SelectionControl` extends `ListControl` to add current item awareness;
- `TransitionControl` extends `ItemControl` to add visual transitions between *views*;
- `NavigationControl` extends `ItemControl` to add browser-like Back/Forward navigation support.

### IPresenter

All *controls* implement the *IPresenter* interface, which allows to asynchronously present any object (*model*), with optional *Options*. Because the loading and presenting of *views* is asynchronous, you must always subscribe to the returned observable:

```c#
control.Present(myModelObject, options).AutoDetach().Subscribe();
```

## Manifest

The *Manifest* is an asset file that stores the mappings between the different models, view models, views and prefabs.

![](Doc/ShowzupManifest.png "Showzup Manifest")

To create a manifest from the main menu, select *Assets > Create > Showzup > Manifest*. The manifest will then be automatically selected and displayed in the Inspector window. You must then configure the path within your assets where your view prefabs are located and the URI prefix that will be prepended to each prefab's relative path, in order to load it at run-time. Finally, press the *Build* button to regenerate the manifest using those new configurations.

### How the Manifest is generated

- All *ViewModel*-derived classes are scanned and their base class declaration (`... : ViewModel<TModel>`) is used to determine the *Model* > *ViewModel* mappings. 
- Similarly, all *View*-derived classes are scanned and their base class declaration (`... : View<TViewModel>`) is used to determine the *ViewModel* > *View* mappings.
- Finally, all prefabs in the given folder and sub-folders are scanned for any *View*-derived MonoBehaviours attached to them to determine the *View* > *Prefab* mappings.
- *Variants* may be associated with each of those mappings (as described in following section *Variants*, under *Variant Mapping*).

## Variants

You might want the same model object, depending on context, to be displayed in different ways, possibly with different presentation logics. In *Showzup*, those different contexts are named *Variants* and are used to select the proper view model, view and prefab, whenever there is more than one that matches.

### Examples of variants

The following are only examples of the kinds of variants that might be required in some arbitrary application.  There are no built-in variants, so you must define yourself those that make sense to your particular application.

#### Display variants

- **Page**: Large image, with a large title, a smaller sub-title and a full description paragraph.
- **Popup**: Large image with a thin title underneath.
- **List Item**: Only a small thumbnail next to a title.
- **Menu Item**: Only title.

#### Platform variants

- **iOS**: Top nav-bar, with iOS-like back button in the upper-left corner.
- **Android**: No back button, as OS provides its own navigation for that.

#### Form-factor variants

- **Tablet**: Many panels, with more details in each one.
- **Mobile**: Fewer panels, with fewer details in each one.
- **TV**: Similar to tablet, but with larger fonts for 10-foot viewing.

#### Orientation variants

- **Portrait**: Vertical layout, with everything stacked in a single column.
- **Landscape**: Two columns, with an overview on the left and details on the right.

#### Theme variants

- **Christmas**: Christmas-themed skin
- **Halloween**: Halloween-themed skin

### How to define variants

As in the following example, variants are defined by creating a class named according to the variant group (*Display* in this case) and adding a `public static readonly` field for each variant in the group: 

```C#
public class Display : Variant<Display>
{
    public static readonly Display Page = Create();
    public static readonly Display Popup = Create();
    public static readonly Display ListItem = Create();
    public static readonly Display MenuItem = Create();
}
```

The *Page* variant can be accessed (just as any static field) via `Display.Page` and the variant group itself is `Display.Group`, or from the `Group` property on any variant, for example `Display.Page.Group`.

All your variant groups must be registered at startup with the `VariantProvider` in order for Showzup to know about them. The following code creates a new `VariantProvider` from the variant groups *Display*, *Form* and *Platform* and binds it in the dependency injection container (the actual binding syntax here is specific to *Injexit* and will vary from one dependency injection framework to another):

```c#
var variantProvider = VariantProvider.From<Display, Form, Platform>();
container.BindInstance(variantProvider);
```

### How to tag prefabs with variants

Simply put the prefab in a sub-folder (underneath the root view prefab folder) with a name that matches a variant name.

For example, if all your root view prefab folder is *Assets/Prefabs/Resources* and you place some prefabs in the *Assets/Prefabs/Resources/* **iOS/Lanscape/Page** folder, those prefabs will be tagged with the following variants (assuming you have defined the variants in the example above):

- Platform.iOS
- Orientation.Landscape
- Display.Page

Note that you may also have other arbitrary sub-folder names that do not match any variant, for purely organizational purposes.

### How to tag view models and views with variants

Simply apply the `[Variant]` attribute to any view model or view class: 

```c#
[Variant("Page")]
public class FooView : View<FooViewModel>
{
}
```

It is however recommended to use the C# 6 `nameof()` operator, so that if you rename your variants, all references to them will stay in sync:

```c#
[Variant(nameof(Display.Page))]
public class FooView : View<FooViewModel>
{
}
```

Note that tagging view models and views with variants is less common than tagging prefabs. Most of the time, the same view models and views will work for all variants and only prefabs need to be customized for each variant. For example, the same view class can often be reused with multiple prefabs, even if you don't bind all its UI fields.  You can also have conditional logic in the view when those UI fields are null, which increases the reusability of the view.

### Distinction between *Supported variants* and *Requested variants*

If you tag some view models, views or prefabs with one or more variants, those are said to be their *supported variants*.

When time comes to display a model, we need to specify the variants to be used. Those are the *requested variants*. *Showzup* resolves the proper view models, views and prefabs to use by going through all candidate mappings and comparing their *supported variants* with the variants you requested. 

### Variants are grouped

Each variant is part of a *Variant Group*. In the above examples, the *Page* variant would be part of the *Display* group.

### No variant means *generic*

If a view model, view or prefab is not tagged with any variant from a given group, it is considered generic and can be used for any variant of that group.  However, such a generic variant is considered less prioritary than a specific variant match, if there is one. 

### Variants can be combined across groups

Variants of different groups can become very powerful when they are combined together.  For example, you could have prefabs for the *Mobile + Landscape* variant combination that are different from those from  the *Tablet + Landscape* or *Mobile + Portrait* combinations.

### Variant mapping in manifest

We saw earlier how *Showzup* generates mappings and stores them in the manifest. During this process, for each mapping, it also takes the variants of both mapped items (if any) and combines them into the mapping. For example, if FooViewModel is tagged with *Page* and FooView with *iOS*, the mapping *FooViewModel > FooView* will be tagged with both *Page + iOS*.

![](Doc/ShowzupVariantMapping.png "Showzup Variant Mapping")

### Requesting variants at run-time

There are three ways of requesting specific variants at run-time (shown as dark-grey boxes in diagram below).

![](Doc/ShowzupVariantPresentation.png "Showzup Variant Presentation")

#### Options.Variants

This is the least common way of requesting variants. The optional `Options` object, that you can pass as second parameter to the `IPresenter.Present()` method, has a `Variants` property for requesting specific variants on the spot.

#### Control.Variants

This is the most useful and common way of requesting variants. You simply specify the requested variant(s) in the `Variants` property of the control in the *Inspector* window. Whenever you will present a model object in that control, it will request those variants to be fulfilled.

#### IVariantProvider.GlobalVariants

Variants that are global to the entire application, such as *Platform*, *FormFactor*, *Orientation* and *Theme* in our previous example, can be set via the `GlobalVariants` property of the `IVariantProvider`.

### Configuring dependency injection

This is an excerpt from the example project that configures the various dependencies using *Injexit*:

```c#
using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Loadzup.Resource;
using Silphid.Showzup;
using Silphid.Injexit;
using UniRx;
using UnityEngine;

namespace App
{
    public class Application : MonoBehaviour
    {
        public Manifest Manifest;
        public NavigationControl NavigationControl;
        
        public void Start()
        {
            var container = new Container(Debug.unityLogger);

            container.BindInstance(Debug.unityLogger);
            container.BindInstance(CreateLoader());
            container.Bind<IScoreEvaluator, ScoreEvaluator>().AsSingle();
            container.Bind<IViewResolver, ViewResolver>().AsSingle();
            container.Bind<IViewLoader, ViewLoader>().AsSingle();
            container.BindInstance<IManifest>(Manifest);
            container.BindInstance<IInjectionAdapter>(new InjectionAdapter(container));
            container.BindInstance(VariantProvider.From<Display, Form, Platform>());

            container.InjectScene(gameObject.scene);
        }

        private ILoader CreateLoader() =>
            new ResourceLoader(new SpriteConverter());
    }
}
```

### Create *storyboard* Scene(s)

In order to faciliate editing your view prefabs, you can create a scene with multiple canvases side by side, one canvas for each view prefab. Configure you canvases in World Space and size them according to each view's likely dimensions. This scene is only used at design-time. We call it a *storyboard*, in reference to Xcode's storyboards.

If you multiple developers are designing those prefabs, it is recommended to isolate each prefab in its own scene, but load all those scenes together in the editor by dragging them all into the *Hierarchy* window, using Unity's (rather) new multi-scene editing. You can then lay all your canvases side by side into some pratical layout, even if they are in different scenes.

Note that you may be able to setup your storyboard scenes so that they could be tested in *Play* mode, by loading your main scene in addition to the storyboard scenes. When you hit *Play*, your storyboard canvases will be off-screen and should not interfere. This might however not be ideal, as all storyboard view prefabs will be loaded and active and might introduce side-effects and extra memory consumption. It's just a quick shortcut to avoid switching between your storyboard scenes and the main scene.

## Under Development

- Customizable multi-phase transitioning.

## Wishlist

- Making *Showzup* compatible with other dependency injection frameworks than *Injexit*, such as *Zenject*.
- Allowing controls to dynamically swap views when global variants change, for example to present a more suitable view when orientation switches from landscape to portrait.

# Experimental Libraries

Some unclassified libraries, still in an early development stage, to be evolved, reorganized and time-tested.

## Abstracam

A lightweight system for manipulating and interpolating between virtual cameras.  You can manipulate multiple virtual cameras freely, interpolate between them and synchronize a real camera with them.

- Pluggable camera system: `FreeCamera`, `ReadOnlyCamera`, `TransitionCamera`, `SmoothingCamera`.
- Cameras can be observed as Observables.

## Silphid.Sequencit.Input

An experimental input layering/filtering system, currently included in *Silphid.Sequencit*, but should be externalized eventually.

- Allows multiple input layers to be defined in a nested hierarchy.
- Each layer can be disabled, including all its nested descendant layers.
- Multiple parts of the code can request a layer to be disabled and only when all of them have released this request (by disposing their IDisposable) will the layer effectively be reactivated.  This is what I call the "Flag" pattern (an implementation of which can be found in Silphid.Extensions.DataTypes).
- This system is abstract and independent from any input system. When detecting some input, you have to manually query a layer to determine whether it is currently active or not.