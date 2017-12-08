---
title: Types of Injection
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: injexit
permalink: /injexit/injection-types
---

In most dependency injection frameworks, there are three different approaches for injecting dependencies into an object, with their pros and cons, as well as limitations.

For the examples in the following section, we will use those simple dummy types: 

```c#
public interface IFoo {}
public interface IGoo {}
public interface IBar {}

public class Foo : IFoo {}
public class Goo : IGoo {}

public class Bar : IBar
{
  public Bar(IFoo foo, IGoo goo)
  {
  }
}
```

## Constructor injection

```c#
public class Bar : IBar
{
  private readonly IFoo _foo;
  private readonly IGoo _goo;
  
  public Bar(IFoo foo, IGoo goo)
  {
    _foo = foo;
    _goo = goo;
    
    // Initialization code can go here, because all dependencies are received at once.
  }
}
```

This is the best way to inject dependencies because:

- All dependencies are clearly visible as constructor parameters.
- It is easy to create and initialize the object without using any dependency injection framework, for example in your unit tests. When an object is instantiated manually in this way, it is instantly ready to be used.
- All initialization code can be placed directly into the constructor, because it receives all dependencies at once.
- There is no need for exposing fields or writable properties.
- There is no need for injection attribute annotations; the framework can figure dependencies simply by looking at constructor parameter types.
- Fields can be marked `readonly` and are garanteed to be constant throughout the object's life-time.

However, this approach is simply not possible for instances that are already created (or at least not created by the container), which is the case for all game objects and components, as they are created by Unity (when a scene or prefab is loaded).

*Use this approach for everything that is not a MonoBehaviour.*

If a class has multiple constructors, you must mark only one of them with the `[Inject]` attribute to let *Injexit* know which one to inject.

``` c#
public class Bar : IBar
{
  public Bar()
  {
    // This constructor will be ignored
  }
  
  [Inject]
  public Bar(IFoo foo, IGoo goo)
  {
    // This constructor will be injected
  }
}
```

## Method injection

When you cannot use *constructor injection*, this approach is a good compromise, with some of the same advantages.

```c#
public class Bar : IBar
{
  private IFoo _foo;
  private IGoo _goo;
  
  [Inject]
  public void Init(IFoo foo, IGoo goo)
  {
    _foo = foo;
    _goo = goo;
    
    // Initialization code can go here, because all dependencies are received at once.
  }
}
```

- There is a single and clean entry point for all dependencies.
- However, fields *cannot* be marked `readonly`.

*Use this approach for all MonoBehaviours.*

## Field or property injection

```c#
public class Bar : IBar
{
  [Inject]
  public IFoo Foo;

  [Inject]
  public IGoo Goo;

  // This part is only needed when you have initialization code that relies on Foo and Goo
  [Inject]
  public void Init()
  {
    // Initialization code can go here, because fields and properties are injected before methods.
  }
}
```

*Avoid this approach, as it is messier and more error prone, and rather use method injection for MonoBehaviours.*