#!/usr/bin/env node

'use strict';

const fs = require("fs"),
    path = require("path"),
    process = require("process"),
    globule = require("globule"),
    yaml = require("js-yaml");

function main() {
    var args = require('commander');

    args
        .version('1.0.0')
        .option('-u, --update', 'update headers')
        .option('-r, --remove', 'remove headers')
        .option('-i, --input <file>', 'config file name', 'copyhead.yaml')
        .parse(process.argv);

    if (args.update)
        process(args, updateHeader);
    else if (args.remove)
        process(args, removeHeader);
    else
        args.help();
}

function loadConfig(args) {
    const configFiles = globule.find(args.input);

    if (configFiles.length == 0)
        throw new Error("Config file not found: " + args.input);

    const config = yaml.safeLoad(fs.readFileSync(configFiles[0], 'utf8'));

    config.separator = new RegExp(config.separator);
    return config;
}

function process(args, fileAction) {
    const config = loadConfig(args);

    config.items.forEach(function(item) {
        var files = globule.find(item.pattern);
        files.forEach(function(file) {
            fileAction(file, config, item);
        });
    }, this);
}

function updateHeader(file, config, item) {
    fs.readFile(file, function(err, data) {
        data = item.header + data;
        fs.writeFile(file, data, { flag: 'w' });
    });
}

function removeHeader(file, config, item) {
    fs.readFile(file, function(err, data) {
        // TODO: Remove header from data
        fs.writeFile(file, data, { flag: 'w' });
    });
}

main();