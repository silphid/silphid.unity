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

            //get config file path root without filename
            var configFileRoot = configFile.substring(0, configFile.lastIndexOf("/"));

            //Resolve the target depending on config file root
            //Ex: Resolve "target:Submodules/Lib2"" to "UnityProject\Submodules\Lib1\Submodules\Lib2"
            var newAbsoluteTarget = path.resolve(configFileRoot, item.target);

            item.target = newAbsoluteTarget;
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
    var absTarget = path.resolve(item.target);
    var relSourcePath = item.location;
    var linkName = item.name;

    //create parent directory recursively if needed
    var parentPath = path.resolve(relSourcePath, "..");
    if (!fs.existsSync(parentPath))
        mkdir('-p', parentPath);

    //using node fs, source and target are inversed (source is the folder where the link points to)
    fs.symlink(absTarget, relSourcePath, "dir", (exception) => {
        if (exception) {
            if (exception.code == "EPERM")
                console.error("ERROR EPERM - Be sure to run command in Administrator mode");
            else if (exception.code == "EEXIST")
                console.warn("WARN EEXIST - '" + linkName + "' Link already created");
            else
                console.error(exception);
        }
        else {
            console.log("Symlink created: " + linkName);
        }
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