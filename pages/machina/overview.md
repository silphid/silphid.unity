---
title: Machina
keywords: 
last_updated: 
tags: []
summary:
sidebar: main
folder: machina
permalink: /machina
---

*Machina* is a simple Rx-based polymorphic state machine.  It is said to be *polymorphic* in that it builds upon the general principle that a state machine should be in one and only one state at any given time, and extends it to allow some states to be polymorphically equivalent to others (in other words, inherit from others).

## State polymorphism example

The addition of polymorphism greatly simplifies the modelling of states and the handling of changes between them. For example, here are a few states for objects in a game I am working on: 

![](/images/MachinaPolymorphism.png "Machina Polymorphism")

An object can either be *Idle* or *Busy*.  But *Moving* and *Falling* are both considered equivalent to being *Busy*.  There are also two ways of being considered *Moving*, by *Pushing* some other object or being *Pushed* by it.

It is then easy to handle whenever an object enters or leaves the *Busy* state, no matter the specific state that made it busy. The same applies to *Moving*, which can be handled generically, independently from the specific kind of movement.