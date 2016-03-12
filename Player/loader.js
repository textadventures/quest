var state = require('./state.js');
var scripts = require('./scripts.js');
    
var allowedVersions = [500, 510, 520, 530, 540, 550];
var impliedTypes = {};

var getXmlAttribute = function (node, attributeName) {
    var attribute = node.attributes[attributeName];
    if (!attribute) return null;
    return attribute.value;
};

var attributeLoaders = {
    'string': function (node, element, attributeName) {
        var attributeValue = node.textContent; 
        state.set(element, attributeName, attributeValue);
    },
    'int': function (node, element, attributeName) {
        var attributeValue = node.textContent; 
        state.set(element, attributeName, parseInt(attributeValue, 10));
    },
    'double': function (node, element, attributeName) {
        var attributeValue = node.textContent; 
        state.set(element, attributeName, parseFloat(attributeValue));
    },
    'stringlist': function (node, element, attributeName) {
        var list = state.newAttribute('stringlist');
        for (var i = 0; i < node.childNodes.length; i++) {
            var childNode = node.childNodes[i];
            if (childNode.nodeName != 'value') continue;
            list.value.push(childNode.textContent);
        }
        state.set(element, attributeName, list);
    },
    'boolean': function (node, element, attributeName) {
        var attributeValue = node.textContent;
        if (attributeValue === '' || attributeValue == 'true') {
            state.set(element, attributeName, true);
        }
        else if (attributeValue == 'false') {
            state.set(element, attributeName, false);
        }
        else {
            throw 'Invalid boolean "' + element.name + '" = "' + attributeValue + '"';
        }
    },
    'script': function (node, element, attributeName) {
        var script = scripts.parseScript(node.textContent);
        state.set(element, attributeName, {
            type: 'script',
            script: script
        });
    },
    'simplepattern': function (node, element, attributeName) {
        var value = node.textContent
            .replace(/\(/g, '\\(')
            .replace(/\)/g, '\\)')
            .replace(/\./g, '\\.')
            .replace(/\?/g, '\\?')
            .replace(/#([A-Za-z]\w+)#/g, function (match, group1) {
                return '(?<' + group1 + '>.*)';
            });
        
        if (value.indexOf('#') !== -1)
        {
            throw 'Invalid command pattern ' + element.attributes.name + '.' + attributeName + ' = ' + node.textContent;
        }
        
        var patterns = value.split(/\s*;\s*/).map(function (pattern) {
            return '^' + pattern + '$';
        }).join('|');
        
        state.set(element, attributeName, patterns);
    }
};

var loadElementAttributes = function (element, nodes) {
    for (var i = 0; i < nodes.length; i++) {
        var node = nodes[i];
        if (node.nodeType !== 1) continue;
        var attributeName = node.nodeName;
        if (attributeName == 'inherit') {
            var name = getXmlAttribute(node, 'name');
            state.addInheritedType(element, name);
        }
        else if (attributeName == 'object' || attributeName == 'command') {
            var child = elementLoaders[attributeName](node);
            state.set(child, 'parent', element);
        }
        else {
            if (attributeName == 'attr') {
                attributeName = getXmlAttribute(node, 'name');
            }
            var attributeType = getXmlAttribute(node, 'type');
            if (!attributeType) {
                var key = (element.elementSubType || element.elementType) + '~' + attributeName;
                attributeType = impliedTypes[key];
                
                if (!attributeType) {
                    if (node.textContent.length === 0) {
                        attributeType = 'boolean';
                    }
                    else {
                        attributeType = 'string';
                    }
                }
            }
            var loader = attributeLoaders[attributeType];
            if (loader) {
                loader(node, element, attributeName);
            }
            else {
                console.log('no attribute loader for type ' + attributeType);
            }
        }
    }
};

var elementLoaders = {
    'game': function (node) {
        var element = state.create('game', 'object', 'game');
        var name = getXmlAttribute(node, 'name');
        state.set(element, 'gamename', name);
        loadElementAttributes(element, node.childNodes);
    },
    'function': function (node) {
        var paramList;
        var parameters = getXmlAttribute(node, 'parameters');
        if (parameters) {
            paramList = parameters.split(/, ?/);
        }
        state.addFunction(getXmlAttribute(node, 'name'),
            scripts.parseScript(node.textContent),
            paramList);
    },
    'type': function (node) {
        var name = getXmlAttribute(node, 'name');
        var element = state.create(name, 'type');
        loadElementAttributes(element, node.childNodes);
    },
    'object': function (node) {
        var name = getXmlAttribute(node, 'name');
        var element = state.create(name, 'object', 'object');
        loadElementAttributes(element, node.childNodes);
        return element;
    },
    'command': function (node) {
        var name = getXmlAttribute(node, 'name');
        if (name == null) name = state.getUniqueId();
        var element = state.create(name, 'object', 'command');
        loadElementAttributes(element, node.childNodes);
        return element;
    },
    'implied': function (node) {
        var element = getXmlAttribute(node, 'element');
        var attribute = getXmlAttribute(node, 'property');
        var type = getXmlAttribute(node, 'type');
        impliedTypes[element + '~' + attribute] = type;
    },
    'template': function (node) {
        var name = getXmlAttribute(node, 'name');
        var templateType = getXmlAttribute(node, 'templatetype');
        // TODO: Template overrides - see Templates.cs (AddTemplate)
        var elementName = state.getUniqueId('template');
        var template = state.create(elementName, 'template');
        state.set(template, 'templatename', name);
        state.set(template, 'text', node.textContent);
        state.addTemplate(template);
    }
};

var load = function (data) {
    var parser = new DOMParser();
    var doc = parser.parseFromString(data, 'application/xml');
    var firstNode = 0;
    var i;
    for (i = 0; i < doc.childNodes.length; i++) {
        if (doc.childNodes[i].nodeType === 1) {
            firstNode = i;
            break;
        }
    }
    var asl = doc.childNodes[firstNode];
    if (asl.nodeName !== 'asl') {
        throw 'File must begin with an ASL element';
    }
    var versionAttribute = asl.attributes.version;
    if (!versionAttribute) {
        throw 'No ASL version number found';
    }
    var version = parseInt(versionAttribute.value);
    if (allowedVersions.indexOf(version) === -1) {
        throw 'Unrecognised ASL version number';
    }
    
    for (i = 1; i < asl.childNodes.length; i++) {
        if (asl.childNodes[i].nodeType !== 1) continue;
        var loader = elementLoaders[asl.childNodes[i].nodeName];
        if (loader) {
            loader(asl.childNodes[i]);
        }
        else {
            console.log('no loader for ' + asl.childNodes[i].nodeName);
        }
    }
    
    state.dump();
};

exports.load = load;