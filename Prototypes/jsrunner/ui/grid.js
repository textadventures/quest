var scale, gridX, gridY, player;
var playerVector, playerDestination;
var offsetVector, offsetDestination;
var allPaths = new Array();
var customLayerPaths = new Array();
var customLayerObjects = {};
var customLayerSvg = {};
var customLayerImages = {};
var layers = new Array();
var maxLayer = 3;
var currentLayer = 0;
var offset = new Point(0, 0);
var symbols = new Object();
var newShapePoints = new Array();

for (var i = -maxLayer; i <= maxLayer; i++) {
    var layer = new Layer();
    layers.push(project.activeLayer);
}

var customLayer = new Layer();
var customLayerOffset = new Point(0, 0);
customLayer.visible = false;

function activateLayer(index) {
    showCustomLayer(false);
    layers[getLayerIndex(index)].activate();
    layers[getLayerIndex(index)].opacity = 1;
    if (currentLayer != index) {
        layers[getLayerIndex(currentLayer)].opacity = 0.2;
        currentLayer = index;
    }
}

function getLayerIndex(index) {
    if (index < -maxLayer || index > maxLayer) {
        alert("Layer out of bounds. Current layer range: -" + maxLayer + " to " + maxLayer);
    }
    // layers array represents z-indexes from -maxLayer to maxLayer
    return index + maxLayer;
}

activateLayer(currentLayer);

gridApi.setScale = function(newScale) {
    scale = newScale;
    gridX = new Point(scale, 0);
    gridY = new Point(0, scale);
};

gridApi.setZoom = function(zoom) {
    paper.view.zoom = zoom;
};

gridApi.zoomIn = function(amount) {
    paper.view.zoom = paper.view.zoom * (Math.pow(1.1, amount));
};

function onMouseDrag(event) {
    updateOffset(event.delta);
}

function onMouseUp(event) {
    var x = getGridSquareX(event.point);
    var y = getGridSquareY(event.point);
    ASLEvent("JS_GridSquareClick", x + ";" + y);
}

function updateOffset(delta) {
    setOffset(getOffset() + delta);
    var paths;
    if (project.activeLayer == customLayer) {
        paths = customLayerPaths;
    }
    else {
        paths = allPaths;
    }
    for (var i = 0; i < paths.length; i++) {
        paths[i].position += delta;
    }
    if (playerDestination && project.activeLayer != customLayer) {
        playerDestination += delta;
    }
}

function getOffset() {
    if (project.activeLayer == customLayer) {
        return customLayerOffset;
    }
    return offset;
}

function setOffset(value) {
    if (project.activeLayer == customLayer) {
        customLayerOffset = value;
    }
    else {
        offset = value;
    }
}

function onFrame(event) {
    if (playerVector) {
        var distance = player.position - playerDestination;
        if (distance.length > playerVector.length) {
            player.position += playerVector;
        }
        else {
            player.position = playerDestination;
            playerVector = null;
            playerDestination = null;

            var playerPositionAbsolute = player.position - offset;
            offsetDestination = paper.view.center - playerPositionAbsolute;

            offsetVector = (offsetDestination-offset) / 10;
        }
    }
    if (offsetVector) {
        var distance = offset - offsetDestination;
        if (distance.length > offsetVector.length) {
            updateOffset(offsetVector);
        }
        else {
            updateOffset(offsetDestination-offset);
            offsetVector = null;
            offsetDestination = null;
        }
    }
}

gridApi.drawGrid = function(minX, minY, maxX, maxY, border) {

    function gridLine(start, end) {
        var path = new Path();
        path.strokeColor = border;
        path.add(start, end);
        addPathToCurrentLayerList(path);
    }

    // draw the vertical lines
    for (var x = minX; x <= maxX; x++) {
        var start = gridPoint(x, minY);
        var end = gridPoint(x, maxY);
        gridLine(start, end);
    }

    // draw the horizontal lines
    for (var y = minY; y <= maxY; y++) {
        var start = gridPoint(minX, y);
        var end = gridPoint(maxX, y);
        gridLine(start, end);
    }
};

function gridPoint(x, y) {
    return (gridX * x) + (gridY * y) + getOffset();
}

function getGridSquareX(point) {
    return Math.floor(((point - getOffset()) / gridX).x);
}

function getGridSquareY(point) {
    return Math.floor(((point - getOffset()) / gridY).y);
}

function gridPointNudge(x, y, nudgeX, nudgeY) {
    var result = gridPoint(x, y);
    result.x += nudgeX;
    result.y += nudgeY;
    return result;
}

var firstBox = true;

gridApi.drawBox = function(x, y, z, width, height, border, borderWidth, fill, sides) {
    activateLayer(z);
    // if this is the very first room, centre the canvas by updating the offset
    if (firstBox) {
        var centrePoint = gridPoint(x + width / 2, y + height / 2);
        var offsetX = paper.view.center.x - centrePoint.x;
        var offsetY = paper.view.center.y - centrePoint.y;
        updateOffset(new Point(offsetX, offsetY));
        firstBox = false;
    }
    var path = null;
    var points = [gridPoint(x, y), gridPoint(x + width, y), gridPoint(x + width, y + height), gridPoint(x, y + height)];
    // sides is encoded with bits to represent NESW
    var draw = [sides & 8, sides & 4, sides & 2, sides & 1];
    for (var i = 0; i < 4; i++) {
        var next = (i + 1) % 4;
        if (draw[i]) {
            if (path == null) {
                path = new Path();
                allPaths.push(path);
                if (borderWidth > 0) {
                    path.strokeColor = border;
                    path.strokeWidth = borderWidth;
                }
                path.add(points[i]);
            }
            path.add(points[next]);
        } else {
            path = null;
        }
    }
    var fillPath;
    if (sides == 15) {
        fillPath = path;
    } else {
        fillPath = new Path();
        fillPath.add(points[0], points[1], points[2], points[3]);
        allPaths.push(fillPath);
    }
    fillPath.fillColor = fill;
    fillPath.closed = true;
};

gridApi.drawLine = function(x1, y1, x2, y2, border, borderWidth) {
    var path = new Path;
    path.strokeColor = border;
    path.strokeWidth = borderWidth;
    path.add(gridPoint(x1, y1));
    path.add(gridPoint(x2, y2));
    addPathToCurrentLayerList(path);
};

gridApi.drawArrow = function (id, x1, y1, x2, y2, border, borderWidth) {
    clearExistingObject(id);
    
    var linePath = new Path;
    var start = gridPoint(x1, y1);
    var end = gridPoint(x2, y2);
    linePath.strokeColor = border;
    linePath.strokeWidth = borderWidth;
    linePath.add(start);
    linePath.add(end);
    addPathToCurrentLayerList(linePath);

    var vector = end - start;
    var arrowVector = vector.normalize(10);
    var arrowheadPath = new Path([
        end + arrowVector.rotate(150),
        end,
        end + arrowVector.rotate(-150)
    ]);
    arrowheadPath.strokeColor = border;
    arrowheadPath.strokeWidth = borderWidth;
    addPathToCurrentLayerList(arrowheadPath);

    customLayerObjects[id] = [linePath, arrowheadPath];
};

function addPathToCurrentLayerList(path) {
    if (project.activeLayer == customLayer) {
        customLayerPaths.push(path);
    }
    else {
        allPaths.push(path);
    }
}

gridApi.drawPlayer = function(x, y, z, radius, border, borderWidth, fill) {
    activateLayer(z);
    if (!player) {
        player = new Path.Circle(gridPoint(x, y), radius);
        player.strokeColor = border;
        player.strokeWidth = borderWidth;
        player.fillColor = fill;
        player.fillColor = fill;
        allPaths.push(player);

        var playerPositionAbsolute = player.position - offset;
        var offsetDestinationX = paper.view.center.x - playerPositionAbsolute.x;
        var offsetDestinationY = paper.view.center.y - playerPositionAbsolute.y;

        offsetDestination = new Point(offsetDestinationX, offsetDestinationY);
        offsetVector = (offsetDestination - offset);
    } else {
        playerDestination = gridPoint(x, y);
        playerVector = (playerDestination - player.position) / 10;
        // move player to the end of the activeLayer so it gets drawn on top
        project.activeLayer.addChild(player);
    }
};

gridApi.drawLabel = function(x, y, z, text) {
    activateLayer(z);
    var pointText = new PointText(gridPoint(x, y));
    pointText.justification = "center";
    pointText.fillColor = "black";
    pointText.content = text;
    allPaths.push(pointText);
};

function showCustomLayer(visible) {
    if (visible != customLayer.visible) {
        customLayer.visible = visible;
        for (var idx = 0; idx < layers.length; idx++) {
            layers[idx].visible = !visible;
        }
        if (visible) {
            customLayer.activate();
        }
        else {
            layers[getLayerIndex(currentLayer)].activate();
        }
    }
}

gridApi.showCustomLayer = function(visible) {
    showCustomLayer(visible);
};

gridApi.clearCustomLayer = function() {
    customLayer.removeChildren();
};

gridApi.clearAllLayers = function () {
    player = null;
    $.each(layers, function(idx, layer) {
        layer.removeChildren();
    });
};

gridApi.setCentre = function(x, y) {
    var centrePoint = gridPoint(x, y);
    var offsetX = paper.view.center.x - centrePoint.x;
    var offsetY = paper.view.center.y - centrePoint.y;
    var curOffset = getOffset();
    updateOffset(new Point(offsetX, offsetY));
};

gridApi.drawCustomLayerSquare = function(id, x, y, width, height, text, fill) {
    var points = [];
    points.push(gridPointNudge(x, y, 1, 1));
    points.push(gridPointNudge(x + width, y, -1, 1));
    points.push(gridPointNudge(x + width, y + height, -1, -1));
    points.push(gridPointNudge(x, y + height, 1, -1));

    var textPoint = gridPoint(x + width / 2, y + height / 2);
    gridApi.drawCustomLayerObject(id, points, text, textPoint, fill, fill);
};

function clearExistingObject(id) {
    var existing = customLayerObjects[id];
    if (existing) {
        for (var idx in existing) {
            var path = existing[idx];
            // TO DO: Should remove path from layer and layerlist array
            path.visible = false;
        }
    }
}

gridApi.drawCustomLayerObject = function (id, points, text, textPoint, border, fill, opacity) {
    clearExistingObject(id);

    var paths = new Array();
    path = new Path();
    path.strokeColor = border;
    $.each(points, function(index, value) {
        path.add(value);
    });
    path.fillColor = fill;
    path.closed = true;
    if (typeof opacity != "undefined") {
        path.opacity = opacity;
    }
    addPathToCurrentLayerList(path);
    paths.push(path);

    if (text) {
        var pointText = new PointText(textPoint);
        pointText.justification = "center";
        pointText.fillColor = "black";
        pointText.content = text;
        if (typeof opacity != "undefined") {
            pointText.opacity = opacity;
        }
        addPathToCurrentLayerList(pointText);
        paths.push(pointText);
    }

    customLayerObjects[id] = paths;
};

gridApi.loadSvg = function (data, id) {
    var svg = paper.project.importSVG(data);
    if (svg) {
        symbols[id] = new Symbol(svg);
    }
};

gridApi.drawCustomLayerSvg = function (id, symbolId, x, y, width, height) {
    if (symbolId in symbols) {
        var existing = customLayerSvg[id];
        var placedSymbol = existing ? existing : symbols[symbolId].place();
        placedSymbol.scale(gridX.x * width / placedSymbol.bounds.width, gridY.y * height / placedSymbol.bounds.height);
        placedSymbol.position = gridPoint(x, y) + placedSymbol.bounds.size / 2;
        if (!existing) addPathToCurrentLayerList(placedSymbol);
        customLayerSvg[id] = placedSymbol;
    } else {
        console.log("No symbol loaded with id '" + symbolId + "'");
    }
};

gridApi.drawCustomLayerImage = function(id, url, x, y, width, height) {
    var existing = customLayerImages[id];
    var raster = existing ? existing : new Raster(url);
    var resizeRaster = function() {
        raster.scale(gridX.x * width / raster.bounds.width, gridY.y * height / raster.bounds.height);
        raster.position = gridPoint(x, y) + raster.bounds.size / 2;
    };
    if (existing) {
        resizeRaster();
    } else {
        raster.onLoad = resizeRaster;
        addPathToCurrentLayerList(raster);
        customLayerImages[id] = raster;
    }    
}

gridApi.addNewShapePoint = function (x, y) {
    newShapePoints.push([x, y]);
};

gridApi.drawShape = function (id, border, fill, opacity) {
    var points = [];
    for (var idx in newShapePoints) {
        var xy = newShapePoints[idx];
        points.push(gridPoint(xy[0], xy[1]));
    }
    gridApi.drawCustomLayerObject(id, points, null, null, border, fill, opacity);
    newShapePoints = [];
};

gridApi.onLoad();