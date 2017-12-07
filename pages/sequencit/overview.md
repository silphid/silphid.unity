---
title: Sequencit
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: sequencit
permalink: /sequencit
---

*Sequencit* extends *UniRx* with the concept of `ISequencer` for coordinating time-based operations in sequence or in parallel, such as playing sound effects and animations, performing transitions between pages or loading data from the Net. In the Rx tradition, all such operations are represented as `IObservable<Unit>`.

## Features

- `Sequence` - an *observable* sequencer to which you can add multiple observables to be executed sequentially.  It completes once its last child has completed.
- `Parallel` - an *observable* sequencer to which you can add multiple observables to be executed in parallel.  It completes once all its children have completed.
- `SequenceQueue` - a *non-observable* sequencer to which you can add multiple observables to be executed sequentially, as soon as possible.  Because it is not an observable it does not require subscribing to it and never completes.  As soon as it is created, it starts waiting for operations to be added to it, then executes those operations in order and, when it reaches the end of its queue, simply waits for more operations to be added to it.
- Various waiting and synchronization options:
  - Wait for a disposable to be disposed;
  - Wait for an observable's `OnNext`/`OnCompleted`;
  - Wait for some time interval...
- Extension methods for `IObservable<T>` and DOTween's `Tween`.

![](Doc/Sequencit.png "Sequencit")