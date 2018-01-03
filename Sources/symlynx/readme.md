# symlynx

A simple tool to create multiple symlinks specified in one or more configuration files.

## How it works:

- Recursively scans for `links.yaml` config files
- Creates symlinks accordingly
- Adds symlinks to `.gitignore` file in root folder

*Important: Paths are relative to each config file's directory.*

## Installing

Install the package via npm:
```
npm install -g symlynx
```

## Usage

Create one or more `links.yaml` config files in your project, ensuring all paths are relative to their defining config file:

```
- source: Relative/Path/To/Symlink1
  target: Relative/Path/To/TargetFolder1
- source: Relative/Path/To/Symlink2
  target: Relative/Path/To/TargetFolder2
[...]
```

Run the following command from your project's root directory to create symlinks recursively, add them to .gitignore, and display verbose messages:
```
symlynx -crgv
```

> Note that, on Windows platform, this command must be ran in Administrator mode.

## Syntax

```
symlynx [options]
```

Options:
```
-V, --version       output the version number
-c, --create        create symlinks
-d, --delete        delete all symlinks (use -dr to delete recursively)
-r, --recursive     process recursively
-g, --git-ignore    add symlinks to .gitignore in each config's folder
-i, --input <file>  config file name (default: links.yaml)
-l, --log <file>    log file name
-v, --verbose       output extra information to console
-h, --help          output usage information
```

## Example

Given the following directory structure:
```
\---ProjectRoot
    |   links.yaml
    |   
    +---Folder1
    |   \---SubFolder1
    |       |---(assuming we need a `SourceFolder1` symlink here targetting `TargetFolder1`)
    |       \---(and a `SourceFolder2` symlink here targetting `TargetFolder2`)
    |   
    \---Folder2
        \---SubFolder2
            |---TargetFolder1
            \---TargetFolder2
```

In the root `links.yaml` file, put:
```
- source: Folder1/SubFolder1/SourceFolder1
  target: Folder2/SubFolder2/TargetFolder1
- source: Folder1/SubFolder1/SourceFolder2
  target: Folder2/SubFolder2/TargetFolder2
```

## Credits

Inspired from a script by Emmanuel Jacquier (emmanuel.jacquier@gmail.com)