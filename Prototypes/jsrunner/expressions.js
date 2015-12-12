define(['jsep'], function (jsep) {
    jsep.removeUnaryOp('~');
    jsep.addUnaryOp('not');
        
    jsep.removeBinaryOp('>>>');
    jsep.removeBinaryOp('<<');
    jsep.removeBinaryOp('>>');
    
    jsep.removeBinaryOp('==');
    jsep.removeBinaryOp('===');
    jsep.removeBinaryOp('!==');
    jsep.addBinaryOp('=', 6);
    jsep.addBinaryOp('<>');
    
    jsep.addBinaryOp('^', 10);
    
    jsep.removeBinaryOp('||');
    jsep.removeBinaryOp('|');
    jsep.addBinaryOp('or', 1);
    
    jsep.removeBinaryOp('&&');
    jsep.removeBinaryOp('&');
    jsep.addBinaryOp('and', 2);
    
    var parseExpression = function (expr) {
        return {
            expr: expr,
            tree: jsep(expr)
        };
    };
    
    return {
        parseExpression: parseExpression
    };
});