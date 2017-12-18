---
title: Integrating as a Git submodule
keywords: git submodule symlynx advanced
last_updated: 
tags: []
summary:
sidebar: main
folder: welcome
permalink: welcome/integrating-submodule
toc: false
---

## Note

*The following are advanced instructions for integrating the framework into your project's Git repository as a Git submoldule, which will allow you to contribute to the framework or evolve it in your own fork, while keeping it independant from your project's source code. This is a rather complex approach, so if you prefer a simplest way, just follow the instructions in the [Getting Started]({{"welcome/getting-started" | relative_url}}) section instead.*

## About the folder structure

The following instructions assume a specific folder structure:

```
\---SuperProject (replace by your own project name)
    |   
    +---Unity.Silphid (the framework as a Git submodule)
    |   
    \---UnityProject (your Unity project)
```

> NOTE: If you want to use a different structure or different folder names, you will have to modify the *links.yaml* config file to match those relative paths. More on this later.

## Setting up your Unity project

- Create your root Git repository and clone it locally.  We will refer to that folder as your *SuperProject*.
- Create your Unity project in a subfolder named *UnityProject*.
- Configure your Unity project:
  - In the Unity Editor menu, click *Edit > Project Settings > Player* and in *Other Settings > Scripting Runtime Version*, and select *Experimental (.NET 4.6 Equivalent)*.
  - When prompted, select *Restart*.

## Adding the framework submodule

Add the framework as a Git submodule (as a subfolder named *Silphid.Unity*) by executing the following command from your *SuperProject* folder:

```
git submodule add https://github.com/silphid/silphid.unity.git Silphid.Unity
```

Now update the framework's own submodules (therefore, sub-submodules):

```
cd Silphid.Unity
git submodule init
git submodule update
```

## Installing the *symlynx* tool

I created a Node.js tool, named *symlynx*, to automate the creation of symbolic links in your Unity project's `Assets/Plugins` folder that point to the proper folders in the framework submodule.

### Install Node.js and npm

- On OSX, [follow these instructions](https://changelog.com/posts/install-node-js-with-homebrew-on-os-x).
- On Windows, [follow these instructions](http://blog.teamtreehouse.com/install-node-js-npm-windows).
- If you already have Node.js and npm installed, make sure they are the latest versions.

### Install the *symlynx* package

The following command will install *symlynx* as a globally available command:

```
npm install -g symlynx
```

## Creating the symbolic links

- Copy the `_template_links.yaml` file from the *Silphid.Unity* subfolder into your *SuperProject* folder.
- Rename it to `links.yaml`.
- In your *SuperProject* folder, execute the following command:

```
symlynx -crg
```

> The `-crg` options mean respectively: *create* symbolic links by looking for *links.yaml* config files *recursively* and add those links to *.gitignore* files.
>
> For more details about the syntax of that tool, visit its [npm page](https://www.npmjs.com/package/symlynx).

## More information about symbolic links

### How it works

There is two *links.yaml* config files, the one you copied into your *SuperProject* folder and the one already present in the framework's subfolder. The tool recursively looks for those files and then creates all the symlinks they define. Those paths are specified *relatively* to each config file's folder. The result is that symlinks will be created in your `UnityProject/Assets/Plugins`, as well as within `Silphid.Unity/Assets/Plugins` (in case you also want to work in the framework's project on its own).

### How to use a different folder structure

To use your own arbitrary folder structure, before running the *symlynx* tool, simply edit the *links.yaml* file in your *SuperProject* folder, making sure to adjust all relative paths accordingly.  You do *not* need to edit the other *links.yaml* file that sits in the framework's folder.

### Troubleshooting

To display more verbose messages with the *symlynx* tool, add the `-v` option:

```
symlynx -crgv
```

If you want to delete all symlinks recursively to start over from fresh, use the following command:

```
symlynx -dr
```

> WARNING: This is a dangerous command that must **only** be executed from within your *SuperProject* folder structure. **Never** execute it elsewhere, as it will remove all symlinks it finds in the current folder and *all* its subfolders recursively.

