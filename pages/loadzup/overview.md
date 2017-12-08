---
title: Loadzup
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: loadzup
permalink: /loadzup
---

Typically in *Unity*, depending on the source you want to load your assets from (*WWW*, *Resources*, *AssetBundleManager*...), you need to use a different class, with different syntax and peculiarities. And if you change the type of storage for an asset, you actually need to change your code.

*Loadzup* streamlines the loading of all assets and resources into a uniform, asynchronous pipeline, allowing assets to be addressed by URI.  If you change the type of storage for an asset, you only need to change the URI that points to it, not the code that accesses it.

For example, *Showzup* leverages *Loadzup* in order to load views from prefabs.  Any prefab can be associated with a view simply via its URI, no matter if it is stored in resources or in an asset bundle.

## Features

- Unified, asynchronous, IObservable-based loading pipeline.
- URI-based asset addressing according to schemes: `http://`, `bundle://`, `res://`
- Support for loading from WWW, Resources (and soon AssetBundleManager).
- Support for Content/MIME types and custom request headers.
- Support for Unity's new `WebRequest` (in replacement of `WWW`).
- Complete rewrite of Unity's `AssetBundleManager` (by [crazydadz](https://github.com/crazydadz)).
- Time-out
- Automatic conversion, with built-in converters for Sprite, Texture and Json.NET.
- Advanced caching policies (ETag/Last-Modified, cache/origin-first…).
- Highly modular and extensible (pluggable loaders and converters).

![](/images/Loadzup.png "Loadzup")

## Wishlist

- Cache expiration
- Priority queues
- `CancellationToken` support
- `IProgress` support
- Scene Loading