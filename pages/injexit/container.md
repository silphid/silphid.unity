---
title: The Container
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: injexit
permalink: /injexit/container
---

The `Container` class is the central class in *Injexit*. You create an instance of it at startup, use it to configure all your type *bindings*, and then let it perform its wonders.  That class implements the `IContainer` interface, which itself derives from the `IBinder`, `IResolver` and `IInjector` interface. Those interfaces correspond to the three types of operations you may need to perform in your application. We will explore those three types of operations in the following sections respectively named *Binding*, *Resolving* and *Injecting*.

![](Doc/InjexitContainer.png "IContainer")

In the spirit of SOLID principles, those three interfaces were kept as small and simple as possible, and all of the sophisticated operations that you can do with them have been externalized into extension methods.

### IBinder

From an application development perspective, this is the most important part of the container. It allows to map all your interfaces to concrete classes at startup, which mappings (*bindings*) are then used at run-time to resolve, instantiate and inject the proper instances.

We will demonstrate that interface and its extension methods in action in the *Binding* section below.

### IResolver

You typically won't need to use this interface, as it is mainly used internally by *Injexit* to resolve the dependencies of each object, but in more advanced scenarios it can prove very useful, as we will see in the *Resolving* section below.

### IInjector

This interface allows to manually inject dependencies into already created instances, which is necessary for example with game objects, as they are instantiated by Unity when a scene or prefab is loaded. More on that in the *Injecting* section below.

For manually injecting dependencies, you cannot use *constructor injection*, because the instance has already been create, and you must instead rely on *field* or *property injection*, as described in the *Types of injection* section below.
