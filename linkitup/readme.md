# linkitup

A simple tool to create multiple symlinks specified in one or more configuration files.

## How it works:

- Recursively scans for `links.yaml` config files
- Creates symlinks accordingly
- Adds symlinks to `.gitignore` file in root folder

*Important: Paths are relative to each config file's directory.*

## Usage

Install the package via npm:
```
npm install -g linkitup
```

Create one or more `links.yaml` config files in your project, ensuring all paths are relative to each config file.

Run the following command from your project's root directory to create symlinks recursively, add them to .gitignore, and display verbose messages:
```
linkitup -crgv
```

> Note that, on Windows platform, this command must be ran in Administrator mode.

Display usage with:
```
linkitup -h
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

Original script by Emmanuel Jacquier (emmanuel.jacquier@gmail.com)