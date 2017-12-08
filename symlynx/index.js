#!/usr/bin/env node

'use strict';

const fs = require("fs"),
    path = require("path"),
    process = require("process"),
    globule = require("globule"),
    yaml = require("js-yaml"),
    sh = require("shelljs"),
    delSymlinks = require('del-symlinks'),
    yesno = require('yesno');

function make() {

    var args = require('commander');

    args
        .version('1.0.0')
        .option('-c, --create', 'create symlinks')
        .option('-d, --delete', 'delete all symlinks (use -dr to delete recursively)')
        .option('-r, --recursive', 'process recursively')
        .option('-g, --git-ignore', "add symlinks to .gitignore in each config's folder")
        .option('-i, --input <file>', 'config file name', 'links.yaml')
        .option('-l, --log <file>', 'log file name')
        .option('-v, --verbose', 'output extra information to console')
        .parse(process.argv);

    if (args.create)
        createLinks(args);
    else if (args.delete)
        deleteLinksWithPrompt(args);
    else
        args.help();
}

function createLinks(args) {
    const glob = (args.recursive ? '**/' : '') + args.input;
    const configFiles = globule.find(glob);

    var generated = [];

    if (configFiles.length == 0) {
        console.log("No config files found!");
        return;
    }

    if (args.verbose)
        console.log("Using config files: " + configFiles.join("\n"));

    configFiles.forEach(function(configFile) {

        // Get config file's directory
        const configDir = configFile.substring(0, configFile.lastIndexOf("/"));
        
        // Load yaml config file
        const content = yaml.safeLoad(fs.readFileSync(configFile, 'utf8'));

        content.forEach(function(item) {
            // Resolve target relative to config file's directory
            const relativeSource = item.source;
            item.source = path.resolve(configDir, item.source);
            item.target = path.resolve(configDir, item.target);

            createLink(args, item);

            if (args.gitIgnore)
                addToGitIgnore(args, configDir, relativeSource);

            generated.push(item);
        }, this);

    }, this);

    if (args.log)
        fs.writeFile(args.log, yaml.safeDump(generated), { flag: 'w' }, function(err) {
            if (err)
                throw err;
            if (args.verbose)
                console.log('Saved log of created links to: ' + args.log);
        });
}

function deleteLinksWithPrompt(args) {
    if (args.recursive)
        yesno.ask('You are about to remove all symlinks recursively from current folder and all sub-folders!\nAre you sure you want to continue?', true, function(ok) {
            process.stdin.pause();
            if(ok) {
                deleteLinksSilently(args);
            } else {
                console.log("Cancelled.");
            }
        });
    else
        deleteLinksSilently(args);
}

function deleteLinksSilently(args) {
    const glob = args.recursive ? "**/*" : "*";
    delSymlinks([glob]).then(symlinks => {
        if (symlinks.length > 0)
            console.log('Deleted symlinks:\n' + symlinks.join('\n'));
        else
           console.log('No symlinks found.');
    });
}

function createLink(args, item) {
    // Create parent directories recursively if needed
    var parentDir = path.resolve(item.source, "..");
    if (!fs.existsSync(parentDir))
        mkdir('-p', parentDir);

    // Remove symlink if already exists
    if (fs.existsSync(item.source) && fs.lstatSync(item.source).isSymbolicLink()) {
        if (args.verbose)
            console.log("Removing existing symlink: " + item.source);

        fs.unlinkSync(item.source);
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

function addToGitIgnore(args, configDir, relativeSource) {
    const gitignore = path.join(configDir, ".gitignore");
    if (!fs.existsSync(gitignore)) {
        if (args.verbose)
            console.log("Creating new: " + gitignore);
        fs.writeFileSync(gitignore, "");
    }

    var content = fs.readFileSync(gitignore).toString();

    //write in git ignore if not already exist
    if (content.indexOf(relativeSource) == -1) {
        if (args.verbose)
            console.log("Adding entry to " + gitignore + ": " + relativeSource);
        if (content.length > 0 && !content.endsWith("\n"))
            content += "\n";
        content += relativeSource + "\n";
        fs.writeFileSync(gitignore, content, { flag: 'w' });
    }
}

make();