#!/usr/bin/env node

'use strict';

var fs = require("fs"),
    path = require("path"),
    process = require("process"),
    globule = require("globule"),
    yaml = require("js-yaml"),
    shelljs = require('shelljs/global');;

function make() {
    var configs = globule.find('**/Symlinks.yaml');
    var generated = [];

    console.log("Found files: " + configs);
    configs.forEach(function(configFile) {

        var content = yaml.safeLoad(fs.readFileSync(configFile, 'utf8'));
        content.forEach(function(item) {
            console.log("Parsing dependency: " + item.name);

            // Get config file's directory
            var configDir = configFile.substring(0, configFile.lastIndexOf("/"));

            // Resolve target relative to config file's directory
            item.source = path.resolve(configDir, item.source);
            item.target = path.resolve(configDir, item.target);
            console.log("configFile: " + configFile);
            console.log("configDir: " + configDir);
            console.log("item.target: " + item.target);
            console.log("--");

            createLink(item);
            addToGitIgnore(item);

            generated.push(item);
        }, this);

    }, this);

    var createInfoFile = false;
    if (createInfoFile)
    {
        fs.writeFile("generated-links.yaml", yaml.safeDump(generated), { flag: 'w' }, function(err) {
                if (err)
                    throw err;
                console.log('generated-links.yaml saved');
            });
    }
}

function createLink(item) {
    // Create parent directories recursively if needed
    var parentDir = path.resolve(item.source, "..");
    if (!fs.existsSync(parentDir))
        mkdir('-p', parentDir);

    // Using node fs, source and target are inversed (source is the folder where the link points to)
    fs.symlink(item.target, item.source, "dir", (exception) => {
        if (exception) {
            if (exception.code == "EPERM")
                console.error("EPERM ERROR: Be sure to run command in Administrator mode");
            else if (exception.code == "EEXIST")
                console.warn("Symlink already exists: " + item.source);
            else
                console.error(exception);
        }
        else
            console.log("Symlink created: " + item.source + " -> " + item.target);
    });
}

function addToGitIgnore(item) {
    if (!fs.existsSync(".gitignore")) {
        fs.writeFileSync(".gitignore", "");
    }

    var content = fs.readFileSync('.gitignore').toString();

    //write in git ignore if not already exist
    if (content.indexOf(item.location) == -1) {
        console.log("updating .gitignore");
        content += item.location + "\n";
        fs.writeFileSync(".gitignore", content, { flag: 'w' });
    }
}

make();