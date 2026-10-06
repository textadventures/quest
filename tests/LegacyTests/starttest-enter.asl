!include <stdverbs.lib>

define game <Start Enter Test>
	asl-version <410>
	start <room>
end define

define room <room>
	script {
		msg <What is your name?>
		enter <name>
		msg <Hello #name#>
	}
end define
