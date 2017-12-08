---
title: Showzup
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: showzup
permalink: /showzup/
---

*Showzup* is a Rx-based MVVM framework for asynchronous loading of views, custom visual transitions, multiple variants for each view, etc.

It leverages other modules of *Silphid*, like [Loadzup](/loadzup) for loading data, views and images asynchronously, [Injexit](/injexit) for dependency injection,  [Sequencit](/sequencit) for sequencing of transitions and animations, [Machina](/machina) for state-machine-based page navigation flows and [Commons](/commons) for all sorts of tidy extension methods.

*Showzup* adopts a *data-driven* approach to UI development. You simply pass the raw data (the *Model*) to a *Control* and let it figure how to display it.  *Showzup* resolves which *ViewModel* to wrap the *Model* with, and which *View* and *Prefab* to show inside the control.  In MVVM, the *ViewModel*'s job is to augment the raw *Model* with all the necessary business and presentation logic, often by resorting to some services to interact with the rest of the application and the outside world.

![](/images/Showzup.png "Showzup Overview")

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

## Configuring dependency injection

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

## Creating *Storyboard* Scene(s)

In order to faciliate editing your view prefabs, you can create a scene with multiple canvases side by side, one canvas for each view prefab. Configure you canvases in World Space and size them according to each view's likely dimensions. This scene is only used at design-time. We call it a *storyboard*, in reference to Xcode's storyboards.

If you multiple developers are designing those prefabs, it is recommended to isolate each prefab in its own scene, but load all those scenes together in the editor by dragging them all into the *Hierarchy* window, using Unity's (rather) new multi-scene editing. You can then lay all your canvases side by side into some pratical layout, even if they are in different scenes.

Note that you may be able to setup your storyboard scenes so that they could be tested in *Play* mode, by loading your main scene in addition to the storyboard scenes. When you hit *Play*, your storyboard canvases will be off-screen and should not interfere. This might however not be ideal, as all storyboard view prefabs will be loaded and active and might introduce side-effects and extra memory consumption. It's just a quick shortcut to avoid switching between your storyboard scenes and the main scene.

## Under Development

- Customizable multi-phase transitioning.

## Wishlist

- Making *Showzup* compatible with other dependency injection frameworks than *Injexit*, such as *Zenject*.
- Allowing controls to dynamically swap views when global variants change, for example to present a more suitable view when orientation switches from landscape to portrait.