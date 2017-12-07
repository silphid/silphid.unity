---
title: Showzup
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: showzup
permalink: /showzup
---

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
- `NavigationControl` extends `TransitionControl` to add browser-like Back/Forward navigation support.

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