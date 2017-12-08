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

## Injecting into an existing instance

``` c#
Container.Inject(instance);
```

This will only inject the dependencies of given instance.

## Injecting into all scripts of a game object

```c#
Container.Inject(gameObject);
```

This will recursively inject the dependencies of all `MonoBehaviour` scripts attached to given `GameObject` and its descendants.

## Injecting into all scripts of a scene

After you have set up all your bindings, the `RootInstaller` and `SceneInstaller` will automatically call this method to inject all game objects in your scene:

```c#
Container.InjectScene(gameObject.scene);
```

## Overriding bindings at injection-time

When you want to specify bindings that should only apply to the current injection operation (or that should temporarily override existing bindings), you can create a sub-container on the fly with the `Using()` extension method, just before calling `Inject()`:

``` c#
injector
  .Using(x => x.Bind<IFoo, Foo2>())
  .Inject(obj);
```