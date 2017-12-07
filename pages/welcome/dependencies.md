---
title: Dependencies
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: welcome
permalink: welcome/dependencies
---

## Inter-Dependencies

The libraries were designed to minimize dependencies upon each other. However, higher level libraries do build upon lower level ones. For example, *Extensions*, which is at the bottom of the stack, only depends on Unity and can be used on its own, whereas *Showzup*, at the top of the stack, builds upon all the other libraries.

![](Doc/Overview.png "Dependencies")

In other words, there is no dependency from lower level libraries to higher level ones, so you may easily strip those higher level libraries if you don't need them.  Many of the integration modules (shown as dark green boxes) are optional and can be removed (see *Optional Dependencies* below).

## Required Dependencies

### Unity 2017.1 or greater

*Silphid.Unity* requires Unity 2017.1 or greater for its .NET 4.6 and C# 6.0 support.

### UniRx

*Silphid.Unity* also requires (and is distributed with) [UniRx](https://github.com/neuecc/UniRx) 5.5.0, the Unity implementation of the [Reactive Extensions](http://reactivex.io) (*Rx*).  It is a great and thorough library on which *Silphid.Unity* relies heavily (for example, all asynchronous operations return or use some form of `IObservable<T>`).

If you are new to Rx, there are plenty of resources on the Net.  Here are my personal recommendations:

- [The introduction to Reactive Programming you've been missing](https://gist.github.com/staltz/868e7e9bc2a7b8c1f754) - A great intro and overview. The place to start.
- [IntroToRx.com](http://introtorx.com) - The complete online version of a great book by Lee Campbell, very thorough and insightful.  But start with the previous link first! ;)
- [ReactiveX.io](http://reactivex.io) - The official reference hub for implementations of Rx on all platforms.
- [RxMarbles](http://rxmarbles.com) - Interactive diagrams to experiment with and better understand observables and their various operators.
- [UniRx](https://github.com/neuecc/UniRx) - Obviously, the UniRx documentation is another good read, with many good examples specific to Unity.

### DOTween

*Silphid.Unity* can be used with DemiGiant's [DOTween](http://dotween.demigiant.com/), a great high-performance tweening/animation library for Unity with a rich and clean fluent API.  It supports low-level time-based sequencing, which *Sequencit* nicely complements with higher-level observable-based sequencing.

## Optional Dependencies

All optional integrations with third-parties have been isolated into their own sub-libraries or script sub-folders, that can be stripped away without impacting anything else.  For example, *Sequencit.DOTween* is an extension to *Sequencit* that supports integration with *DOTween*, but is not required for *Sequencit* to work.

### Json.NET

Newtonsoft's [Json.NET](http://www.newtonsoft.com/json) is a very useful library for (de)serializing objects to/from JSON format.  However, it had issues with Unity, especially on iOS because of AOT, and therefore has been ported to Unity by [ParentElement](https://www.assetstore.unity3d.com/en/#!/content/11347) (there's also a version by [SaladLab](https://github.com/SaladLab/Json.Net.Unity3D), but I haven't tried it).

#### How to enable the Silphid.Unity integration scripts

Because *Json.NET* is a paid asset, it is not distributed with *Silphid.Unity* and you must therefore explicitly enable the integration scripts by adding the `JSON_NET` define to your Unity project (`Edit` > `Project Settings` > `Player` > `Other Settings` > `Scripting Define Symbols`).