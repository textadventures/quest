' Created with QDK 4.1.5

!include <stdverbs.lib>

define game <>
	asl-version <410>
	start <room>
	game info <Created with QDK 4.1.5>
	startscript msg <ON, OFF>
	command <on> {
		timeron <tmr>
		msg <Timer on>
	}
	command <off> {
		timeroff <tmr>
		msg <Timer off>
	}
end define

define options
	debug on
	panes on
	abbreviations on
end define

define room <room>
end define

define timer <tmr>
	interval <5>
	action msg <Tick!>
	disabled
end define

