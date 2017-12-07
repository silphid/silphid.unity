---
title: Binding
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: injexit
permalink: /injexit/binding
---

Because the `IContainer` also implements `IBinder`, all the following examples would also work directly with the container. It is just a best practice to work with the most specific interface (for instance, `IBinder`).

#### Binding concrete types

This is the best type of binding, because it lets *Injexit* create the object and resolve all sub-dependencies:

```c#
Container.Bind<IFoo, Foo>();
Container.Bind<IGoo, Goo>();
Container.Bind<IBar, Bar>();
```

Here, all `Bar` instances will automatically have their constructor injected with instances of `Foo` and `Goo`, because the `Bar` constructor has `IFoo` and `IGoo` parameters, which are respectively bound to the `Foo` and `Goo` concrete classes.

#### Binding instances

In cases where you already have an instance and simply want to inject it into other objects, use this form:

```c#
Foo fooInstance = ...
Container.BindInstance<IFoo>(fooInstance);
```

Or if your instance is already downcasted to the proper interface type, you may omit that type for brevity:

```c#
IFoo fooInstance = ...
Container.BindInstance(fooInstance);
```

*Warning: If you don't specify the generic type, as in the example above, make sure that the variable is exactly of the type that needs to be injected.*

Instance binding is useful when you need to inject a specific game object or component into another object. Just expose a field on you script (that you may assign via the Inspector) and bind that field with ```BindInstance()```:

```c#
using Silphid.Injexit;
using UnityEngine;

public class AppInstaller : RootInstaller
{
  // Assign a value to this field via the Inspector
  public MyComponent myComponent;

  protected override void Configure(IBinder binder)
  {
    Container.BindInstance(myComponent);
    
    // Equivalent to:
    // Container.BindInstance<MyComponent>(myComponent);
  }
}
```

For anything else than MonoBehaviours, the downside of binding a specific instance is that the instance will not have its own dependencies injected, because *Injexit* did not instantiate it. For MonoBehaviours part of your scene, that's OK, because they get injected automatically anyway by the Installer.

#### Binding shared objects

By default, *Injexit* will create a new instance of a class for every other class that depends on it. This is a good pattern, because it limits side-effects between objects. However, sometimes you want an object to be shared, so that changes made to it will be visible to all other objects sharing it as a dependency. This is achieved using the `.AsSingle()` suffix:

```c#
Container.Bind<IFoo, Foo>().AsSingle();
```

Note that instance binding using `BindInstance()` does not support the `.AsSingle()` suffix, because instances specified explicitly are forcibly always shared.

#### Binding lists of objects

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
var foo3 = new Foo3();

Container.Bind<IFoo, Foo1>().AsList();
Container.Bind<IFoo, Foo2>().AsList();
Container.BindInstance<IFoo>(foo3).AsList();
```

Here, the `Bar` constructor will be injected with a `List<IFoo>` containing instances of the `Foo1`, `Foo2` and `Foo3` classes.

Note that *Injexit* will automatically combine the injected objects into a `List<T>`, an `IEnumerable<T>` or a `T[]`, depending on the type that is expected at the injection point.

A more convenient syntax to avoid repetition, which is equivalent to the example above, is:

```c#
Container.BindAsListOf<IFoo>(x =>
{
    x.Bind<Foo1>();
    x.Bind<Foo2>();
    x.BindInstance(foo3);
});
```

#### Binding all implementations/derivatives as a list

```c#
Container.BindAllAsListOf<IFoo>();
```

This will scan the assembly where `IFoo` is defined for all classes implementing that interface (for instance, `Foo1`, `Foo2` and `Foo3`) and bind them as a list of `IFoo`.

You may also specify the assembly to scan explicitly:

```c#
Container.BindAllAsListOf<IFoo>(GetType().Assembly);
```

#### Qualifying bindings with identifiers

When you need finer control over what gets injected where, for instance when you need different implementations of an interface to be injected in various places, you will have to tag your bindings with identifiers:

```c#
Container.Bind<IFoo, Foo1>().WithId("Fire");
Container.Bind<IFoo, Foo2>().WithId("Rain");
Container.BindInstance<IFoo>(foo3).WithId("Ice");
```

And then also tag your injection points with the same identifiers using `[Id]` attributes:

```c#
public class Bar : IBar
{
  // Field injection
  [Inject]
  [Id("Fire")]
  public IFoo Fire;

  // Constructor injection
  public Bar([Id("Rain")] IFoo rain)
  {  
  }
  
  // Method injection
  [Inject]
  public void Init([Id("Ice")] IFoo ice)
  {
  }
}
```

Identifiers are also very helpful for lists:

```c#
Container.BindAsList<IFoo>(x =>
  {
    x.Bind<Foo1>();
    x.Bind<Foo1>();
    x.Bind<Foo2>();
  })
  .WithId("List1");

Container.BindAsList<IFoo>(x =>
  {
    x.Bind<Foo2>();
    x.Bind<Foo3>();
  })
  .WithId("List2");

// You can even mix the two syntaxes.
// This will be added to "List2"
Container.Bind<IFoo, Foo3>().AsList().WithId("List2");
```

Finally, simply inject those two lists like so:

```c#
public class Bar : IBar
{
  public Bar(
      [Id("List1")] List<IFoo> list1,
      [Id("List2")] List<IFoo> list2)
  {
  }
}
```

#### Optional injection

When injection is not required, you may tag your injection points with the `[Optional]` attribute:

```c#
public class Bar : IBar
{
  // Field injection
  [Inject]
  [Optional]
  public IFoo Foo;

  // Constructor injection
  public Bar([Optional] IFoo foo)
  {
  }
  
  // Method injection
  [Inject]
  public void Init([Optional] IFoo foo)
  {
  }
}
```

The `[Optional]` attribute is not necessary for parameters with default values.

```c#
TODO: Example with optional int parameters...
```

#### Controlling the composition tree explicitly

I am a big fan of the [Composite](https://en.wikipedia.org/wiki/Composite_pattern) and [Decorator](https://en.wikipedia.org/wiki/Decorator_pattern) design patterns and I use them abundantly, notably in *Loadzup*. The problem they raise with dependency injection is that they require precise control over their composition tree. You cannot just register each class individually and expect the container to figure how you want all of them to be assembled together. Let's take an example from *Loadzup*, where the `ILoader` interface is implemented by `HttpLoader`, `ResourceLoader` and `CompositeLoader`. The responsibility of `CompositeLoader` is to delegate to the proper child loader according to the URI scheme, so its constructor expects a list of `ILoader` child objects to delegate to:

```c#
public class CompositeLoader : ILoader
{
  // ...
  
  public CompositeLoader(params ILoader[] children)
  {
    // ...
  }

  // ...
}
```

In order to tell the container specifically how to assemble those three classes, we can use the `Using()` method: 

```c#
Container.Bind<ILoader, CompositeLoader>().AsSingle().Using(x =>
{
    x.Bind<ILoader, HttpLoader>().AsList();
    x.Bind<ILoader, ResourceLoader>().AsList();
});
```

In this case, the `HttpLoader` and `ResourceLoader` classes are only exposed to the `CompositeLoader` and when the rest of the application will require an `ILoader`, it will be injected a `CompositeLoader`.

I love this syntax because it shows the composition tree in a very visual way.

However, in order to maximize the benefits of dependency injection, only use this syntax when you need explicit control over the composition tree. Otherwise, you better let the container figure the composition tree for you.

#### Forwarding multiple interfaces to same binding

Sometimes the same class implements multiple interfaces and you want all those interfaces to be bound to the same object, with the same binding options.

```c#
public class FooGooBar : IFoo, IGoo, IBar {}
```

Simply use the `Alias()` operator to specify those extra interfaces:

```c#
Container.Bind<IFoo, FooGooBar>().Alias<IGoo>().Alias<IBar>().AsSingle();
```

Here, the `IFoo`, `IGoo` and `IBar` interfaces will all be bound to the same `FooGooBar` instance. 

#### Self-binding a type

Even though it is a best practice to access most of your concrete classes through abstract interfaces, especially in the context of dependency injection, in some cases you might only have a concrete type with no interface and prefer to inject it as is.

``` c#
Container.BindToSelf<Foo>();
Container.BindToSelf<Goo>();
```

*Showzup* uses that approach for models and view models, which do not have each their own interface.

*This approach however defeats multiple advantages of dependency injection, such as being able to swap an implementation of an interface with some other arbitrary implementation, so try to avoid it, unless it's part of your design.*

#### Self-binding all of a type's derivatives

```c#
Container.BindToSelfAll<IFoo>();
```

This will scan the assembly where `IFoo` is defined for all classes implementing that interface (for instance, `Foo1`, `Foo2` and `Foo3`) and self-bind them. In this case, the `IFoo` interface serves only as a marker. This approach is used in *Showzup* to automatically bind all view model classes.

You may also specify the assembly to scan explicitly:

```c#
Container.BindToSelfAll<IFoo>(GetType().Assembly);
```

#### Self-binding multiple instances

If you want multiple instances of different types, that will only be known at run-time, to be bound to their own type, you can use the `BindInstances()` extension method:

``` c#
IFoo foo = new Foo();
IGoo goo = new Goo();
Container.BindInstances(foo, goo);
```

Or:

```c#
var objects = new object[] { new Foo(), new Goo() };
Container.BindInstances(objects);
```

This is equivalent to:

``` c#
Container.BindInstance<Foo>(foo);
Container.BindInstance<Goo>(goo);
```

*Note that, even if `foo` and `goo` are passed as interfaces `IFoo` and `IGoo` or as an array of `object`s , the `BindInstances()` method always binds each object to its own concrete type (ie: `Foo` or `Goo`). That means that the dependent object must also declare its dependencies using those concrete types.*