---
title: Silphid.Commons
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: commons
permalink: /commons/overview
---

*Silphid.Commons* is a library of various helpers and extension methods for types from *.NET*, *Unity*, *UniRx*, *DOTween*, etc, that provides a concise fluent syntax for many common or more complex operations. This is where I put everything I feel is missing from the APIs I use everyday.

## Features

- Extension methods for `float`, `double`, `int`, `long`, `string`, `DateTime`, `TimeSpan`, `IObservable<T>`, `IEnumerable<T>`, `GameObject`, `Transform`, `Vector2`, `Vector3`, `Quaternion`, `Tween`...
- Maths: `sign`, `abs`, `floor`, `ceiling`...
- Interpolation: linear, quad/cubic Bézier, inversed interpolation (ratio), transposition (from one range to another)...
- Wrapping, clamping, easing, filtering, smoothing, comparing
- `VirtualTimeScheduler` and `TestScheduler` that are currently missing from *UniRx* (*Silphid.Sequencit* uses those for unit testing), but I have submitted a pull request for them to be merged into *UniRx*.
- And much more!