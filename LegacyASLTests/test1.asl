!include <stdverbs.lib>

define game <Unit Test 1>
	asl-version <410>
	start <room>

	define variable <testvar>
		type numeric
		value <0>
		display <Test variable: !>
	end define

	background <black>
	foreground <white>

end define

define options
	debug on
	panes on
	abbreviations on
end define

define room <room>

	south <room2>

	command <wait> {
		wait <Start wait>
		msg <Done wait>
	}

	command <enter> {
		msg <Enter text>
		enter <test>
		msg <You entered: #test#>
	}

	command <ask> {
		msg <Some text>
		if ask <question text> then msg <response yes> else msg <response no>
	}

	command <setstatus> inc <testvar>

	define object <object>
		look <object look desc>
	end define

	define object <twin1>
		alias <twin>
		detail <Twin 1>
		look <It's twin 1>
	end define

	define object <twin2>
		alias <twin>
		detail <Twin 2>
		look <It's twin 2>
	end define

end define

define room <room2>

	north <room>

end define