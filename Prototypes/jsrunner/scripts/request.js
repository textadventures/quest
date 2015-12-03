define(['scriptrunner', 'ui'], function (scriptrunner, ui) {
    return {
        parameters: [2],
        execute: function (ctx) {               
            scriptrunner.evaluateExpression(ctx.parameters[1], function (data) {
                var request = ctx.parameters[0].expr;
                switch (request) {
                    case 'Show':
                        ui.show(data);
                        break;
                    case 'Hide':
                        ui.hide(data);
                        break;
                    default:
                        console.log('Unhandled request type ' + request);
                }
                
                ctx.complete();
            });
        }
    };
});