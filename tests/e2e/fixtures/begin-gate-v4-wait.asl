' A Quest 4 game whose start script waits for a key (verify-wasmplayer-begin-gate.mjs).

define game <BeginGateV4Wait>
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
