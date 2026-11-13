!include <stdverbs.lib>

define game <v410test>
	asl-version <410>
	start <room>

	command <wait> {
		wait <Press a key...>
		msg <Done>
	}

	command <input> {
		msg <Enter your name...>
		enter <name>
		msg <Hello #name#>
	}

	command <input2> {
		enter <first>
		msg <Then...>
		enter <second>
		msg <#first# and #second#>
	}

	command <pause> {
		pause <5000>
		msg <Paused>
	}

	command <menu> choose <test menu>

	command <ask> {
		if ask <Answer me this> then msg <You said yes> else msg <You said no>
		msg <This is run after asking the question>
	}

end define

define options
	debug on
	panes on
	abbreviations on
end define

define room <room>
end define

define selection <test menu>
	info <Choose one...>
	choice <one> msg <Option 1>
	choice <two> msg <Option 2>
end define
