// quest.js will want a base url for image resources
window.gridApi = window.gridApi || {};
window.gridApi.onLoad = function () {
	var file = $_GET['file'] || 'test.aslx';
	var ext = file.toLowerCase().substr(file.length - 4);
	if (ext === '.asl' || ext === '.cas') {
		var fileFetcher = function (filename, onSuccess, onFailure) {
			$.ajax({
				url: filename,
				success: onSuccess,
				error: onFailure
			});
		};
		
		var binaryFileFetcher = function (filename, onSuccess, onFailure) {
			var xhr = new XMLHttpRequest();
			xhr.open('GET', filename, true);
			xhr.responseType = 'arraybuffer';
			xhr.onload = function() {
				if (this.status == 200) {
					var result = new Uint8Array(this.response);
					onSuccess(result);
				}
				else {
					onFailure();
				}
			};
			xhr.send();
		};
		
		var game = new LegacyGame(file, file, null, fileFetcher, binaryFileFetcher);
		var onSuccess = function () {
			game.Begin();
		};
		var onFailure = function () {
			console.log("fail");
		};
		quest.sendCommand = game.SendCommand.bind(game);
		quest.endWait = game.EndWait.bind(game);
		quest.setQuestionResponse = game.SetQuestionResponse.bind(game);
		quest.setMenuResponse = game.SetMenuResponse.bind(game);
		quest.tick = game.Tick.bind(game);
		game.Initialise(onSuccess, onFailure);
	}
	else {
		$.get(file, function (data) {
			quest.load(data);
			quest.begin();
		});
	}
};