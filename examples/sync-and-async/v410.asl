' Created with QDK 4.1.5

!include <stdverbs.lib>

define game <>
	asl-version <410>
	start <room>
	game info <Created with QDK 4.1.5>
	startscript {
		msg <Enter some text first...>
		enter <somevar>
		msg <OK: #somevar#>
		msg <Commands: PAUSE, WAIT, INPUT, ASK>
		msg <DEC to decrease health>
	}
	command <pause> {
		msg <Pausing...>
		pause <3000>
		msg <Done!>
	}
	command <wait> {
		msg <Press a key...>
		wait <This is the wait prompt>
		msg <Done>
	}
	command <input> {
		msg <Enter your name...>
		enter <name>
		msg <Hello #name#>
	}
	command <ask> {
		if ask <Answer me this> then msg <You said yes> else msg <You said no>
		msg <This is run after asking the question>
	}
	command <dec> {
		set numeric <health; %health% - 50>
		msg <Decremented health>
	}
	define variable <health>
		type numeric
		value <100>
		display <Health: !>
		onchange {
			msg <Health changed to %health%>
			if ( %health% = 0 ) then {
				msg <Press a key...>
				wait <>
				msg <Done waiting in onchange> }
		}
	end define
end define

define options
	debug on
	panes on
	abbreviations on
end define

define room <room>
	east <kitchen>
end define

define room <kitchen>
	west <room>
end define

