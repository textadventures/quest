function startServer(dir, port) {
    var finalhandler = require('finalhandler');
    var http = require('http');
    var serveStatic = require('serve-static');

    var serve = serveStatic(dir, { index: ['index.html'] });

    var server = http.createServer(function(req, res){
        var done = finalhandler(req, res);
        serve(req, res, done);
    });

    server.listen(port);
}

var port = 8282;
startServer(__dirname, port);
console.log('Started http://localhost:' + port + '/'); 