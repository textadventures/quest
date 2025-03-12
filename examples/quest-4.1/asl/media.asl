' Created with QDK 4.1.5

!include <stdverbs.lib>

define game <>
	asl-version <410>
	start <room>
	game info <Created with QDK 4.1.5>
	command <wav> {
		msg <Playing WAV>
		playwav <Keehar - New Friend Bit.wav>
	}
	command <sync> {
		msg <Playing WAV synchronously...>
		playwav <Keehar - New Friend Bit.wav; sync>
		msg <Finished playing the WAV>
	}
	command <stop> {
		msg <Stopping WAV>
		playwav <>
	}
	command <mp3> {
		msg <Playing MP3>
		playmp3 <Keehar - Twenty Twenty.mp3>
	}
	command <stopmp3> {
		msg <Stopping MP3>
		playmp3 <>
	}
end define

define options
	debug on
	panes on
	abbreviations on
end define

define room <room>
	look <You can type:|n|nWAV|nSYNC|nSTOP|n|nMP3|nSTOPMP3>
end define

