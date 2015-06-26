(function () {
	window.quest = window.quest || {};
	
	var elements = {};
	var functions = {};
	
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
	
	var addFunction = function (functionName, script) {
		functions[functionName] = script;
	};
	
	var functionExists = function (functionName) {
		return functionName in functions;
	};
	
	var getFunction = function (functionName) {
		return functions[functionName];
	};
	
	var dump = function () {
		console.log("Elements:")
		console.log(elements);
		console.log("Functions:")
		console.log(functions);
	};
	
	quest.set = set;
	quest.get = get;
	quest.isElement = isElement;
	quest.create = create;
	quest.addFunction = addFunction;
	quest.functionExists = functionExists;
	quest.getFunction = getFunction;
	quest.dump = dump;
})();