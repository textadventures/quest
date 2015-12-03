define(['scriptrunner', 'state'], function (scriptrunner, state) {
    return {
        execute: function (ctx) {
            console.log(ctx.parameters.appliesTo + " => " + ctx.parameters.value);
            ctx.complete();
        }
    };
});