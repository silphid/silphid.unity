---
title: Variants
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: showzup
permalink: /showzup/variants
---

You might want the same model object, depending on context, to be displayed in different ways, possibly with different presentation logics. In *Showzup*, those different contexts are named *Variants* and are used to select the proper view model, view and prefab, whenever there is more than one that matches.

## Examples of variants

The following are only examples of the kinds of variants that might be required in some arbitrary application.  There are no built-in variants, so you must define yourself those that make sense to your particular application.

### Display variants

- **Page**: Large image, with a large title, a smaller sub-title and a full description paragraph.
- **Popup**: Large image with a thin title underneath.
- **List Item**: Only a small thumbnail next to a title.
- **Menu Item**: Only title.

### Platform variants

- **iOS**: Top nav-bar, with iOS-like back button in the upper-left corner.
- **Android**: No back button, as OS provides its own navigation for that.

### Form-factor variants

- **Tablet**: Many panels, with more details in each one.
- **Mobile**: Fewer panels, with fewer details in each one.
- **TV**: Similar to tablet, but with larger fonts for 10-foot viewing.

### Orientation variants

- **Portrait**: Vertical layout, with everything stacked in a single column.
- **Landscape**: Two columns, with an overview on the left and details on the right.

### Theme variants

- **Christmas**: Christmas-themed skin
- **Halloween**: Halloween-themed skin

## How to define variants

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

## How to tag prefabs with variants

Simply put the prefab in a sub-folder (underneath the root view prefab folder) with a name that matches a variant name.

For example, if all your root view prefab folder is *Assets/Prefabs/Resources* and you place some prefabs in the *Assets/Prefabs/Resources/* **iOS/Lanscape/Page** folder, those prefabs will be tagged with the following variants (assuming you have defined the variants in the example above):

- Platform.iOS
- Orientation.Landscape
- Display.Page

Note that you may also have other arbitrary sub-folder names that do not match any variant, for purely organizational purposes.

## How to tag view models and views with variants

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

## Distinction between *Supported variants* and *Requested variants*

If you tag some view models, views or prefabs with one or more variants, those are said to be their *supported variants*.

When time comes to display a model, we need to specify the variants to be used. Those are the *requested variants*. *Showzup* resolves the proper view models, views and prefabs to use by going through all candidate mappings and comparing their *supported variants* with the variants you requested. 

## Variants are grouped

Each variant is part of a *Variant Group*. In the above examples, the *Page* variant would be part of the *Display* group.

## No variant means *generic*

If a view model, view or prefab is not tagged with any variant from a given group, it is considered generic and can be used for any variant of that group.  However, such a generic variant is considered less prioritary than a specific variant match, if there is one. 

## Variants can be combined across groups

Variants of different groups can become very powerful when they are combined together.  For example, you could have prefabs for the *Mobile + Landscape* variant combination that are different from those from  the *Tablet + Landscape* or *Mobile + Portrait* combinations.

## Variant mapping in manifest

We saw earlier how *Showzup* generates mappings and stores them in the manifest. During this process, for each mapping, it also takes the variants of both mapped items (if any) and combines them into the mapping. For example, if FooViewModel is tagged with *Page* and FooView with *iOS*, the mapping *FooViewModel > FooView* will be tagged with both *Page + iOS*.

![](/images/ShowzupVariantMapping.png "Showzup Variant Mapping")

## Requesting variants at run-time

There are three ways of requesting specific variants at run-time (shown as dark-grey boxes in diagram below).

![](/images/ShowzupVariantPresentation.png "Showzup Variant Presentation")

### Options.Variants

This is the least common way of requesting variants. The optional `Options` object, that you can pass as second parameter to the `IPresenter.Present()` method, has a `Variants` property for requesting specific variants on the spot.

### Control.Variants

This is the most useful and common way of requesting variants. You simply specify the requested variant(s) in the `Variants` property of the control in the *Inspector* window. Whenever you will present a model object in that control, it will request those variants to be fulfilled.

### IVariantProvider.GlobalVariants

Variants that are global to the entire application, such as *Platform*, *FormFactor*, *Orientation* and *Theme* in our previous example, can be set via the `GlobalVariants` property of the `IVariantProvider`.