' A Quest 4 game that waits for a key in its intro text and its start script.

define game <StartWait>
	asl-version <410>
	start <room>
	startscript {
		msg <Before the wait.>
		wait <Press a key to go on.>
		msg <After the wait.>
	}
end define

define room <room>
end define

define text <intro>
Intro before the key.|w
Intro after the key.
end define
