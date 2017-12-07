---
title: Injecting
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: injexit
permalink: /injexit/injecting
---

Because the `IContainer` also implements `IInjector`, all the following examples would also work directly with the container. It is just a best practice to work with the most specific interface (for instance, `IInjector`).

#### Injecting an existing instance

``` c#
Container.Inject(instance);
```

This will only inject the dependencies of given instance.

#### Injecting all scripts of a game object

```c#
Container.Inject(gameObject);
```

This will recursively inject the dependencies of all `MonoBehaviour` scripts attached to given `GameObject` and its descendants.

#### Injecting all scripts of a scene

After you have set up all your bindings, the `RootInstaller` and `SceneInstaller` will automatically call this method to inject all game objects in your scene:

```c#
Container.InjectScene(gameObject.scene);
```

#### Overriding bindings at injection-time

When you want to specify bindings that should only apply to the current injection operation (or that should temporarily override existing bindings), you can create a sub-container on the fly with the `Using()` extension method, just before calling `Inject()`:

``` c#
injector
  .Using(binder => Container.Bind<IFoo, Foo2>())
  .Inject(obj);
```