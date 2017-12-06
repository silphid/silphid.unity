---
title: Introduction
keywords: 
last_updated: 
tags: []
summary:
sidebar: overview
permalink: overview/intro
folder: overview
---

*Silphid.Unity* is a high quality *.NET* library for the development of modern and fluid *Unity* applications leveraging *Reactive Extensions* (*Rx*) and the *MVVM* pattern, in order to achieve a fully dynamic, data-driven UI, advanced animation sequencing and transitioning, streamlined dependency injection, as well as asynchronous data loading, caching and conversion.

*Silphid.Unity* was inspired by my work of the past 17 years at [Simbioz](http://simbioz.com) and then [LVL Studio](http://lvlstudio.com), initially targeting the WPF framework, but now completely rethought and redesigned for [Unity](http://unity.com) and [Rx](http://reactivex.io).

Even though it has been used to deliver multiple commercial-grade applications, it is still constantly evolving and improving, and I am just now releasing it to the public for the first time. Documentation and examples are still in their early stages and more work still has to be done to make the whole thing easier to integrate into your Unity projects. But, as they say, we have to start somewhere! ;)  All your comments are more than welcomed!

My sincere thanks to [LVL Studio](http://lvlstudio.com) for supporting this effort and being such a great place to work at. If you are looking for an outstanding job opportunity in the Montreal area, make sure to visit our [Careers](http://lvlstudio.com/en/careers) page! :)

# Modules

- [Extensions](#Extensions) - Extension methods (and more) for *.NET*, *Unity*, *UniRx*, *DOTween*, etc, providing a concise fluent syntax for many useful operations.
- [Sequencit](#Sequencit) - Rx-based sequencing of elements with dynamic durations.
- [Injexit](#Injexit) - Dependency injection framework with a clean and efficient fluent syntax.
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
- You can now open the Unity project, located at `/UnityProject`.