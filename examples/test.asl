' Created with QDK 4.1.5

!include <stdverbs.lib>

define game <>
	asl-version <410>
	start <room>
	game info <Created with QDK 4.1.5>
	command <test> {
		msg <Enter some words...>
		enter <input>
		msg <You typed: #input#>
		msg <Type something else.>
		enter <input2>
		msg <First #input#, now #input2#.>
	}
	command <test2> {
		wait <Press a key please...>
		msg <Done!>
		wait <Now press a key again...>
		msg <Finished!>
	}
	command <ask> {
		if ask <test question> then msg <You answered YES.> else msg <You answered NO.>
		if ask <another question> then msg <YES to question 2> else msg <NO to question 2>
	}
	command <tmron> timeron <mytimer>
	command <tmroff> timeroff <mytimer>
	command <pause> {
		msg <Pausing for 5 seconds...>
		pause <5000>
		msg <Finished pause>
	}
	command <menu> choose <test menu>
	verb <read> msg <You can't read that.>
end define

define options
	debug on
	panes on
	abbreviations on
end define

define room <room>
	east <kitchen>

	define object <thing>
		displaytype <Object>
		article <it>
		gender <it>
	end define

	define object <book>
		look <Just an ordinary red book.>
		take
		prefix <a red>
		displaytype <Object>
		detail <the red book>
		article <it>
		gender <it>
		properties <read = A very interesting read.>
	end define

	define object <book2>
		alias <book>
		look <It's the blue book.>
		prefix <a blue>
		displaytype <Object>
		detail <the blue book>
		article <it>
		gender <it>
	end define

end define

define room <kitchen>
	west <room>
end define

define timer <mytimer>
	interval <5>
	action msg <Tick>
	disabled
end define

define selection <test menu>
	info <Choose one of these...>
	choice <one> msg <Option 1>
	choice <two> msg <Option 2>
end define

define text <walkthrough test>
	x thing
	x book
	menu:1
	take it
	e
end define
