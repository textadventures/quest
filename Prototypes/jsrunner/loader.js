/* global quest */

(function () {
    window.quest = window.quest || {};
    
    var allowedVersions = [500, 510, 520, 530, 540, 550];
    
    var loaders = {
        'game': function () {
            console.log("load game...");
        }
    };
    
    quest.load = function (data) {
        var parser = new DOMParser();
        var doc = parser.parseFromString(data, 'application/xml');
        var asl = doc.childNodes[0];
        window.asl = asl;
        if (asl.nodeName !== 'asl') {
            throw 'File must begin with an ASL element';
        }
        var versionAttribute = asl.attributes['version'];
        if (!versionAttribute) {
            throw 'No ASL version number found';
        }
        var version = parseInt(versionAttribute.value);
        if (allowedVersions.indexOf(version) === -1) {
            throw 'Unrecognised ASL version number';
        }
        
        for (var i = 1; i < asl.childNodes.length; i++) {
            if (asl.childNodes[i].nodeType !== 1) continue;
            var loader = loaders[asl.childNodes[i].nodeName];
            if (loader) {
                loader();
            }
            else {
                console.log('no loader for ' + asl.childNodes[i].nodeName);
            }
        }
    };
})();