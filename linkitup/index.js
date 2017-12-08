#!/usr/bin/env node

'use strict';

const fs = require("fs"),
    path = require("path"),
    process = require("process"),
    globule = require("globule"),
    yaml = require("js-yaml"),
    delSymlinks = require('del-symlinks');

function make() {

    var args = require('commander');

    args
        .version('1.0.0')
        .option('-c, --create', 'Create symlinks')
        .option('-u, --unlink', 'Unlink (remove) all symlinks in folder')
        .option('-r, --recurse', 'Process command recursively')
        .option('-g, --git-ignore', 'Add symlinks to .gitignore file in root')
        .option('-i, --input <file>', 'Config file name', 'links.yaml')
        .option('-o, --output [file]', 'Output list of created symlinks to specified yaml file', 'output-links.yaml')
        .option('-v, --verbose', 'Output extra information to console')
        .parse(process.argv);

    if (args.create)
        createLinks(args);
    else if (args.unlink)
        deleteLinks(args);
    else
        args.help();
}

function createLinks(args) {
    var glob = (args.recurse ? '**/' : '') + args.input;
    var configFiles = globule.find(glob);

    var generated = [];

    if (args.verbose)
        console.log("Found config files: " + configFiles);

    configFiles.forEach(function(configFile) {

        var content = yaml.safeLoad(fs.readFileSync(configFile, 'utf8'));
        content.forEach(function(item) {
            if (args.verbose)
                console.log("Parsing dependency: " + item.name);

            // Get config file's directory
            var configDir = configFile.substring(0, configFile.lastIndexOf("/"));

            // Resolve target relative to config file's directory
            item.source = path.resolve(configDir, item.source);
            item.target = path.resolve(configDir, item.target);

            createLink(args, item);

            if (args.gitIgnore)
                addToGitIgnore(args, item);

            generated.push(item);
        }, this);

    }, this);

    if (args.output)
        fs.writeFile(args.output, yaml.safeDump(generated), { flag: 'w' }, function(err) {
            if (err)
                throw err;
            if (args.verbose)
                console.log('Saved list of created links to ' + args.output);
        });
}

function unlink(args) {
    const glob = args.recurse ? "**/*" : "*";
    delSymlinks([glob]).then(symlinks => {
        if (args.verbose)
            console.log('Deleted symlinks:\n', symlinks.join('\n'));
    });
}

function createLink(args, item) {
    // Create parent directories recursively if needed
    var parentDir = path.resolve(item.source, "..");
    if (!fs.existsSync(parentDir))
        mkdir('-p', parentDir);

    // Remove symlink if already exists
    if (fs.exists(item.source)) {
        if (args.verbose)
            console.log("Creating new .gitignore");

        fs.unlink(item.source);
    }

    fs.symlink(item.target, item.source, "dir", (error) => {
        if (error) {
            if (error.code == "EPERM")
                console.error("EPERM ERROR: Be sure to run command in Administrator mode");
            else if (error.code == "EEXIST")
                console.warn("Symlink already exists: " + item.source);
            else
                console.error(error);
        }
        else if (args.verbose)
            console.log("Symlink created: " + item.source + " -> " + item.target);
    });
}

function addToGitIgnore(args, item) {
    if (!fs.existsSync(".gitignore")) {
        if (args.verbose)
            console.log("Creating new .gitignore");
        fs.writeFileSync(".gitignore", "");
    }

    var content = fs.readFileSync('.gitignore').toString();

    //write in git ignore if not already exist
    if (content.indexOf(item.source) == -1) {
        if (args.verbose)
            console.log("Adding entry to .gitignore: " + item.source);
        if (!content.endsWith("\n"))
            content += "\n";
        content += item.source + "\n";
        fs.writeFileSync(".gitignore", content, { flag: 'w' });
    }
}

make();