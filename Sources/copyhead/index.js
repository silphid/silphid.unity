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

    if (args.update) {
        const count = processFiles(args, updateHeader);
        console.log('Updated header of ' + count + ' file(s).');
    }
    else if (args.remove) {
        const count = processFiles(args, removeHeader);
        console.log('Removed header from ' + count + ' file(s).');
    }
    else
        args.help();
}

function loadConfig(args) {
    const configFiles = globule.find(args.input);

    if (configFiles.length == 0)
        throw new Error("Config file not found: " + args.input);

    const config = yaml.safeLoad(fs.readFileSync(configFiles[0], 'utf8'));

    config.regexp = new RegExp(config.regexp);
    return config;
}

function processFiles(args, transform) {
    const config = loadConfig(args);
    var count = 0;

    config.items.forEach(function(item) {
        const patterns = item.pattern.split(/,\s*/)
        const files = globule.find(patterns, { matchBase: true });
        files.forEach(function(file) {
            count++;
            fs.readFile(file, 'utf8', function(err, data) {
                data = transform(data, config, item);
                fs.writeFile(file, data, { flag: 'w' }, function(err) {});
            });
        });
    }, this);

    return count;
}

function removeHeader(data, config, item) {
    return data.replace(config.regexp, '');
}

function updateHeader(data, config, item) {
    return item.header + removeHeader(data, config, item);
}

main();