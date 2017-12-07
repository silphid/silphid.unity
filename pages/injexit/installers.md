---
title: Installers
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: injexit
permalink: /injexit/installers
---

An *installer* is a script responsible for configuring bindings and injecting dependencies into a scene. *Injexit* provides two installer classes that you can extend.

#### RootInstaller

The `RootInstaller` allows to configure most of your bindings in your main scene.  Simply extend the `RootInstaller` class, override its `OnBind()` method to specify your bindings, and (because it's a `MonoBehaviour`) attach it to some root game object in your main scene:

```c#
using Silphid.Injexit;

public class AppInstaller : RootInstaller
{
  protected override void Configure()
  {
    // Add your bindings here
    Container.Bind<IFoo, Foo>();
    Container.Bind<IGoo, Goo>();
    Container.Bind<IBar, Bar>();
  }
  
  protected override void Complete()
  {
    // All game objects in current scene have now been injected
  }
}
```

#### SceneInstaller

Only in *multi-scene* scenarios, the `SceneInstaller` allows your secondary scenes to specify their own bindings and have their game objects injected when they are loaded.  You must extend that class and specify a parent installer type as generic parameter, which it will look for in the root game objects of some other scene and use as parent to inherit all its bindings.

```c#
using Silphid.Injexit;

public class MySceneInstaller : SceneInstaller<AppInstaller>
{
  protected override void Configure()
  {
    // Add your scene-specific bindings (or overrides) here, if any
    Container.Bind<IFoo, Foo2>();
  }
  
  protected override void Complete()
  {
    // All game objects in current scene have now been injected
  }
}
```

Because it inherits from `SceneInstaller<AppInstaller>`, it will look for an `AppInstaller` in some other scene  and use it as parent. It will therefore inherit all the bindings defined in `AppInstaller` and override the `IFoo` binding to `Foo2` (instead of `Foo`), but only for that scene. Finally, it will inject all game objects in the current scene and call the `OnReady()` method.
