!include <stdverbs.lib>

define game <Unit Test 1>
	asl-version <410>
	start <room>
end define

define options
	debug on
	panes on
	abbreviations on
end define

define room <room>

	command <wait> {
		wait <Start wait>
		msg <Done wait>
	}

	command <enter> {
		msg <Enter text>
		enter <test>
		msg <You entered: #test#>
	}

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

