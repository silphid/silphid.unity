# Symlinker
A simple tool to create multiple symlinks specified in one or more configuration files.

## How it works:

- Recursively scans for `Symlinks.yaml` config files
- Creates symlinks accordingly
- Adds symlinks to `.gitignore` file in root folder

*Important: Paths are relative to each config file's directory.*

## Usage

Install the package via npm, by running the following command from the `Symlinker` directory:
```
npm install -g .
```

Create one or more `Symlinks.yaml` config files your project, ensuring all paths are relative to their config file directory.

Run the following command from your project's root directory:
```
symlinker 
```

Note that, on Windows platform, this command must be ran in Administrator mode.

## Example

Given the following directory structure:
```
\---ProjectRoot
    |   Symlinks.yaml
    |   
    +---Assets
    \---Submodules
        \---ProjectLibrary
            \---Assets
                \---Scripts
```

And the following `Symlinks.yaml` file:
```
- name: ProjectLibrary
  location: Assets/LibraryLink
  target: Submodules/ProjectLibrary/Assets/Scripts
```

The tool will create the `ProjectRoot/Assets/LibraryLink` symlink pointing to `Submodules/ProjectLibrary/Assets/Scripts`:

```
\---ProjectRoot
    |   .gitignore
    |   modules.yml
    |   
    +---Assets
    |   \---LibraryLink
    \---Submodules
        \---ProjectLibrary
            \---Assets
                \---Scripts
```

## Credits

Original script by Emmanuel Jacquier (emmanuel.jacquier@gmail.com)