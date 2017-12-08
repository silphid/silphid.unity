---
title: Injexit
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: injexit
permalink: /injexit/
---

*Injexit* is a sophisticated dependency injection framework that I created specifically for the needs of *Silphid.Unity*. For years, I had been using [Zenject](https://github.com/modesttree/Zenject), a great DI framework that I really love, that is very powerful, robust and mature. However, *Showzup* and *Loadzup* make extensive use of the [Decorator](https://en.wikipedia.org/wiki/Decorator_pattern) and [Composite](https://en.wikipedia.org/wiki/Composite_pattern) patterns and thus require *Injexit*'s fine control over how dependencies are assembled together.

*Injexit* started as a minimalist container (with a single class!) and quickly evolved into a full-fledged framework with a clean and efficient fluent syntax.