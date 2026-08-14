' Created with QDK 4.1.5

!include <stdverbs.lib>

define game <>
	asl-version <410>
	start <room>
	game info <Created with QDK 4.1.5>
	startscript {
		msg <Running start script>
		playwav <Keehar - New Friend Bit.wav>
	}
end define

define options
	debug on
	panes on
	abbreviations on
end define

define room <room>
	look <A sound should play at the start of this game.>
end define

