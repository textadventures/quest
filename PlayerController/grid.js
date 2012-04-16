var scale = 50;
var offsetX = 5;
var offsetY = 5;

var gridX = new Point(scale, 0);
var gridY = new Point(0, scale);
var offset = new Point(offsetX * scale, offsetY * scale)
var allPaths = new Array();

function onMouseDrag(event) {
	for (var i = 0; i < allPaths.length; i++) {
		allPaths[i].position += event.delta;
	};
}

gridApi.drawGrid = function(minX, minY, maxX, maxY) {
	function gridLine(start, end) {
		var path = new Path();
		path.strokeColor = "#D0D0D0";
		path.add(start, end);
		allPaths.push(path);
	}

	// draw the vertical lines
	for (var x = minX; x <= maxX; x++) {
		var start = gridPoint(x, minY)
		var end = gridPoint(x, maxY);
		gridLine(start, end);
	};

	// draw the horizontal lines
	for (var y = minY; y <= maxY; y++) {
		var start = gridPoint(minX, y)
		var end = gridPoint(maxX, y);
		gridLine(start, end);
	};
}

function gridPoint(x, y) {
	return (gridX * x) + (gridY * y) + offset;
}

gridApi.drawBox = function(x, y, width, height, border, borderWidth, fill) {
	var path = new Path();
	path.strokeColor = border;
	path.strokeWidth = borderWidth;
	path.fillColor = fill;
	path.add(gridPoint(x, y));
	path.add(gridPoint(x + width, y));
	path.add(gridPoint(x + width, y + height));
	path.add(gridPoint(x, y + height));
	path.closed = true;
	allPaths.push(path);
}

gridApi.onLoad();