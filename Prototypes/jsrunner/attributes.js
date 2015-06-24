(function () {
	window.quest = window.quest || {};
	
	var elements = {};
	
	var getElement = function (elementName) {
		var element = elements[elementName];
		if (!element) {
			throw 'No element named ' + elementName;
		}
		return element;	
	};
	
	var set = function (elementName, attribute, value) {
		var element = getElement(elementName);
		element[attribute] = value;
	};
	
	var get = function (elementName, attribute) {
		var element = getElement(elementName);
		return element[attribute];
	};
	
	var isElement = function (elementName) {
		return elementName in elements;
	};
	
	var create = function (elementName) {
		elements[elementName] = {
			name: elementName	
		};
	};
	
	var dump = function () {
		console.log(elements);
	};
	
	quest.set = set;
	quest.get = get;
	quest.isElement = isElement;
	quest.create = create;
	quest.dump = dump;
})();