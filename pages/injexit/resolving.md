---
title: Resolving
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: injexit
permalink: /injexit/resolving
---

You typically won't need to resolve dependencies manually, as *Injexit* will do it automatically for you behind the scenes. *It is considered a best practice to **only** expose the container to bootstrapping code and **abstract it completely away** from the rest of your application. If some of your classes need to create or resolve objects on-the-fly, use a factory classes or factory lambdas to hide the container from them (see* Factories *section).*

Because the `IContainer` also implements `IResolver`, all the following examples would also work directly with the container. It is just a best practice to work with the most specific interface (for instance, `IResolver`).

## Resolving a type known at compile-time

```c#
resolver.Resolve<IFoo>();
```

## Resolving a type only known at run-time

```c#
Container.Resolve(type);
```

## Overriding bindings at resolution-time

When you want to specify bindings that should only apply to the current resolution operation (or that should temporarily override existing bindings), you can create a sub-container on the fly with the `Using()` extension method, just before calling `Resolve()`:

```c#
resolver
  .Using(x =>
    {
      x.Bind<IFoo, Foo2>())
      x.Bind<IGoo, Goo2>())
    })
  .Resolve<IBar>();
```

When you need to override bindings with specifics instances...

```c#
resolver
  .Using(x =>
    {
      x.BindInstance(foo))
      x.BindInstance(goo))
    })
  .Resolve<IBar>();
```

…there is an equivalent shorthand syntax that supports from one to three instances:

```c#
resolver.UsingInstance(foo).Resolve<IBar>();
resolver.UsingInstances(foo, goo).Resolve<IBar>();
resolver.UsingInstances(foo, goo, hoo).Resolve<IBar>();
```



We will see this syntax in action in the *Factories* section below.

## Factories

When a class needs to create or resolve objects on-the-fly, you can use factory classes or lambdas. Here we will only look at the simplest approach, using factory lambda functions. Let's consider the following class, that gets injected a `Func<IFoo>` factory lambda function and uses it at run-time to create instances of `IFoo` on-the-fly:

```c#
public class Bar : IBar
{
  private readonly Func<IFoo> _fooFactory
  
  public Bar(Func<IFoo> fooFactory)
  {
    _fooFactory = fooFactory;
  }
  
  // Some method that will be called at run-time that needs to create IFoo instances on-the-fly
  public void SomeMethod()
  {
    var foo = _fooFactory();
  }
}
```

Now, let's configure that factory lambda and bind it:

```c#
Container.Bind<IFoo, Foo>(); // This is still needed, as the factory needs to resolve IFoo

Container.BindInstance<Func<IFoo>>(
  () => Container.Resolve<IFoo>());
```

Note that what we are binding here is an instance of the *lambda*, **not** an instance of IFoo.

Because this simple type of factory with no parameters is quite common, there is a shorthand syntax, equivalent to the above binding:

```c#
Container.BindDefaultFactory<IFoo>();
```

### Parametrized factories

In the previous example, because we are using `Container.Resolve<IFoo>()` to resolve our object, any dependency that it might have will also be resolved automatically for you. That's why you should avoid using the `new` keyword in your factories. But what about if you need to pass a specific parameter at run-time? Let's say our `Foo` class needs to know its parent `IBar` that created it, and maybe some other `IGoo` object:

```c#
public class Foo : IFoo
{
  public Foo(IBar parentBar, IGoo someGoo)
  {
    // ...
  }
}
```

Then we can add an `IBar` parameter to the factory lambda and temporarily add that object to the resolver with `.Using()` just before calling `.Resolve()`:

```c#
Container.Bind<IFoo, Foo>();
Container.Bind<IGoo, Goo>();
Container.Bind<IBar, Bar>();

Container.BindInstance<Func<IBar, IFoo>>(
  (IBar parentBar) => Container
    .UsingInstance(parentBar)
    .Resolve<IFoo>());
```

Notice that `IGoo` will be resolved and injected automatically behind the scene. In this particular scenario, we only want to pass `IBar` explicitly to the factory.  So, let's call our factory with the `IBar` parameter (in this case, `this` is the parent `IBar` we want to inject):

```c#
public class Bar : IBar
{
  private readonly Func<IBar, IFoo> _fooFactory
  
  public Bar(Func<IBar, IFoo> fooFactory)
  {
    _fooFactory = fooFactory;
  }
  
  public void SomeMethod()
  {
    var foo = _fooFactory(this);
  }
}
```

There is a shorthand syntax for default factories with zero up to three parameters:

```c#
Container.BindDefaultFactory<IFoo>();                      // Func<IFoo>
Container.BindDefaultFactory<IBar, IFoo>();                // Func<IBar1, IFoo>
Container.BindDefaultFactory<IBar1, IBar2, IFoo>();        // Func<IBar1, IBar2, IFoo>
Container.BindDefaultFactory<IBar1, IBar2, IBar3, IFoo>(); // Func<IBar1, IBar2, IBar3, IFoo>
```

### Typed factories

```c#
Container.BindTypedFactory<IFoo>();                        // Func<Type, IFoo>
Container.BindTypedFactory<IBar, IFoo>();                  // Func<Type, IBar, IFoo>
Container.BindTypedFactory<IBar1, IBar2, IFoo>();          // Func<Type, IBar1, IBar2, IFoo>
Container.BindTypedFactory<IBar1, IBar2, IBar3, IFoo>();   // Func<Type, IBar1, IBar2, IBar3, IFoo>
```