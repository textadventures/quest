!include <stdverbs.lib>

define game <Legacy Culture Numbers Test>
	asl-version <410>
	start <room>

	define variable <negvar>
		type numeric
		value <5>
	end define
end define

define room <room>
	script {
		set numeric <negvar; %negvar% - 15>
		msg <Negative: %negvar%>
	}
end define
